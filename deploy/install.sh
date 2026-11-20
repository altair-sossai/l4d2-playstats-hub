#!/usr/bin/env bash
set -euo pipefail

REPOSITORY="${HUB_REPOSITORY:-altair-sossai/l4d2-playstats-hub}"
VERSION="${HUB_VERSION:-latest}"
PACKAGE="${HUB_PACKAGE:-}"
PORT="${HUB_PORT:-5080}"
DOMAIN="${HUB_DOMAIN:-}"

NAME="l4d2-playstats-hub"
APP_DIR="/opt/${NAME}"
DATA_DIR="/var/lib/${NAME}"
CONFIG_DIR="/etc/${NAME}"
ENV_FILE="${CONFIG_DIR}/hub.env"
SECRETS_KEY_FILE="${CONFIG_DIR}/secrets.key"
SERVICE_FILE="/etc/systemd/system/${NAME}.service"
SOCKET_FILE="/etc/systemd/system/${NAME}.socket"
COMMAND_FILE="/usr/local/bin/playstats-hub"
SERVICE_USER="playstats"
EXECUTABLE="L4D2PlayStats.Hub.Web"

step() {
  printf '\n\033[1;34m==>\033[0m %s\n' "$1"
}

fail() {
  printf '\n\033[1;31mError:\033[0m %s\n' "$1" >&2
  exit 1
}

require_root() {
  if [ "$(id -u)" -ne 0 ]; then
    fail "Run this installer as root, for example: curl -fsSL <address of install.sh> | sudo bash"
  fi
}

detect_runtime() {
  case "$(uname -m)" in
    x86_64 | amd64) RUNTIME="linux-x64" ;;
    aarch64 | arm64) RUNTIME="linux-arm64" ;;
    *) fail "This processor ($(uname -m)) is not supported. Supported: x86_64 and arm64." ;;
  esac
}

install_packages() {
  step "Installing the packages the application needs"

  if command -v apt-get >/dev/null 2>&1; then
    export DEBIAN_FRONTEND=noninteractive
    apt-get update -qq
    apt-get install -y -qq curl tar ca-certificates libicu-dev >/dev/null
  elif command -v dnf >/dev/null 2>&1; then
    dnf install -y -q curl tar ca-certificates libicu >/dev/null
  elif command -v yum >/dev/null 2>&1; then
    yum install -y -q curl tar ca-certificates libicu >/dev/null
  else
    fail "This Linux distribution is not supported by the installer. Use Debian, Ubuntu, Fedora, Rocky or AlmaLinux, or follow the manual steps in the documentation."
  fi
}

cleanup() {
  if [ -n "${WORK_DIR:-}" ] && [ -d "${WORK_DIR}" ]; then
    rm -rf "${WORK_DIR}"
  fi
}

download_package() {
  WORK_DIR="$(mktemp -d)"
  trap cleanup EXIT
  ARCHIVE="${WORK_DIR}/${NAME}.tar.gz"

  if [ -n "${PACKAGE}" ]; then
    step "Using the local package ${PACKAGE}"
    cp "${PACKAGE}" "${ARCHIVE}"
    return
  fi

  if [ "${VERSION}" = "latest" ]; then
    URL="https://github.com/${REPOSITORY}/releases/latest/download/${NAME}-${RUNTIME}.tar.gz"
  else
    URL="https://github.com/${REPOSITORY}/releases/download/${VERSION}/${NAME}-${RUNTIME}.tar.gz"
  fi

  step "Downloading ${URL}"
  curl -fsSL "${URL}" -o "${ARCHIVE}" || fail "Could not download the application. Check the version name and your connection."
}

install_application() {
  step "Installing the application in ${APP_DIR}"

  local staging="${APP_DIR}.new"

  rm -rf "${staging}"
  mkdir -p "${staging}"
  tar -xzf "${ARCHIVE}" -C "${staging}" || { rm -rf "${staging}"; fail "The package could not be extracted. The installed application was not changed."; }
  [ -f "${staging}/${EXECUTABLE}" ] || { rm -rf "${staging}"; fail "The package does not contain ${EXECUTABLE}. The installed application was not changed."; }
  chmod +x "${staging}/${EXECUTABLE}"
  chown -R root:root "${staging}"

  if systemctl list-unit-files "${NAME}.service" >/dev/null 2>&1 && systemctl is-active --quiet "${NAME}"; then
    systemctl stop "${NAME}"
  fi

  if ! id "${SERVICE_USER}" >/dev/null 2>&1; then
    useradd --system --home-dir "${DATA_DIR}" --shell /usr/sbin/nologin "${SERVICE_USER}"
  fi

  rm -rf "${APP_DIR}.old"
  if [ -d "${APP_DIR}" ]; then
    mv "${APP_DIR}" "${APP_DIR}.old"
  fi
  mv "${staging}" "${APP_DIR}"
  rm -rf "${APP_DIR}.old"
  mkdir -p "${DATA_DIR}" "${CONFIG_DIR}"
  chown -R "${SERVICE_USER}:${SERVICE_USER}" "${DATA_DIR}"
  chmod 750 "${DATA_DIR}"
}

configured_port() {
  sed -n 's/^ASPNETCORE_URLS=.*:\([0-9][0-9]*\).*$/\1/p' "${ENV_FILE}" | head -n 1
}

listen_address() {
  local url host

  url="$(sed -n 's/^ASPNETCORE_URLS=//p' "${ENV_FILE}" | head -n 1 | tr -d "\"' ")"

  case "${url}" in
    *";"* | "") return ;;
    http://*:[0-9]*) ;;
    *) return ;;
  esac

  host="${url#http://}"
  host="${host%%/*}"
  host="${host%:*}"

  case "${host}" in
    "+" | "*" | "[::]") echo "${PORT}" ;;
    localhost) echo "127.0.0.1:${PORT}" ;;
    *) echo "${host}:${PORT}" ;;
  esac
}

write_socket() {
  local listen
  listen="$(listen_address)"

  if [ -z "${listen}" ]; then
    if [ -f "${SOCKET_FILE}" ]; then
      systemctl disable --quiet --now "${NAME}.socket" || true
      rm -f "${SOCKET_FILE}"
    fi
    SOCKET_UNIT=""
    return
  fi

  local previous=""
  if [ -f "${SOCKET_FILE}" ]; then
    previous="$(cat "${SOCKET_FILE}")"
  fi

  cat >"${SOCKET_FILE}" <<EOF
[Unit]
Description=L4D2 PlayStats Hub port

[Socket]
ListenStream=${listen}
NoDelay=true

[Install]
WantedBy=sockets.target
EOF

  SOCKET_UNIT="${NAME}.socket"

  if [ -n "${previous}" ] && [ "${previous}" != "$(cat "${SOCKET_FILE}")" ]; then
    SOCKET_CHANGED=1
  fi
}

write_configuration() {
  if [ -f "${ENV_FILE}" ]; then
    step "Keeping the existing configuration in ${ENV_FILE}"

    local kept
    kept="$(configured_port)"

    if [ -n "${kept}" ]; then
      if [ -n "${HUB_PORT:-}" ] && [ "${HUB_PORT}" != "${kept}" ]; then
        printf '  HUB_PORT=%s was ignored: the kept configuration listens on port %s. Edit %s to change it.\n' "${HUB_PORT}" "${kept}" "${ENV_FILE}"
      fi

      PORT="${kept}"
    fi

    add_secrets_key_setting
    return
  fi

  step "Writing the configuration to ${ENV_FILE}"

  cat >"${ENV_FILE}" <<EOF
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:${PORT}
Hub__DataDirectory=${DATA_DIR}
Hub__SecretsKeyFile=${SECRETS_KEY_FILE}
Database__Provider=Sqlite
Database__ConnectionString="Data Source=hub.db"
EOF

  chmod 640 "${ENV_FILE}"
  chown root:"${SERVICE_USER}" "${ENV_FILE}"
}

add_secrets_key_setting() {
  if grep -q '^Hub__SecretsKey\(File\)\?=' "${ENV_FILE}"; then
    return
  fi

  printf '  Adding Hub__SecretsKeyFile=%s to %s\n' "${SECRETS_KEY_FILE}" "${ENV_FILE}"
  printf '\nHub__SecretsKeyFile=%s\n' "${SECRETS_KEY_FILE}" >>"${ENV_FILE}"
}

write_secrets_key() {
  if grep -q '^Hub__SecretsKey=' "${ENV_FILE}"; then
    step "Using the secrets key set in ${ENV_FILE}"
    return
  fi

  local configured
  configured="$(sed -n 's/^Hub__SecretsKeyFile=//p' "${ENV_FILE}" | tail -n 1 | tr -d '"')"
  SECRETS_KEY_FILE="${configured:-${SECRETS_KEY_FILE}}"
  mkdir -p "$(dirname "${SECRETS_KEY_FILE}")"

  if [ -s "${SECRETS_KEY_FILE}" ]; then
    step "Keeping the secrets key in ${SECRETS_KEY_FILE}"
  else
    step "Creating the secrets key in ${SECRETS_KEY_FILE}"

    (umask 077; head -c 32 /dev/urandom | base64 | tr -d '\n' >"${SECRETS_KEY_FILE}")
  fi

  chown "${SERVICE_USER}:${SERVICE_USER}" "${SECRETS_KEY_FILE}"
  chmod 600 "${SECRETS_KEY_FILE}"
}

write_service() {
  step "Registering the ${NAME} service"

  write_socket

  cat >"${SERVICE_FILE}" <<EOF
[Unit]
Description=L4D2 PlayStats Hub
After=network-online.target ${SOCKET_UNIT}
Wants=network-online.target
Requires=${SOCKET_UNIT}

[Service]
Type=simple
User=${SERVICE_USER}
Group=${SERVICE_USER}
WorkingDirectory=${APP_DIR}
EnvironmentFile=${ENV_FILE}
ExecStart=${APP_DIR}/${EXECUTABLE}
Restart=always
RestartSec=5
NoNewPrivileges=true
ProtectSystem=full
ProtectHome=true
PrivateTmp=true
ReadWritePaths=${DATA_DIR}

[Install]
WantedBy=multi-user.target
EOF

  if [ -z "${SOCKET_UNIT}" ]; then
    sed -i '/^Requires=$/d' "${SERVICE_FILE}"
  fi

  systemctl daemon-reload

  if [ -n "${SOCKET_UNIT}" ]; then
    if [ "${SOCKET_CHANGED:-0}" = "1" ]; then
      systemctl stop "${NAME}" "${SOCKET_UNIT}"
    fi
    systemctl enable --quiet --now "${SOCKET_UNIT}"
  fi

  systemctl enable --quiet "${NAME}"
  systemctl restart "${NAME}"
}

write_command() {
  local units="${NAME}"

  if [ -n "${SOCKET_UNIT:-}" ]; then
    units="${SOCKET_UNIT} ${NAME}"
  fi

  cat >"${COMMAND_FILE}" <<EOF
#!/usr/bin/env bash
set -euo pipefail

case "\${1:-help}" in
  status) systemctl status ${NAME} --no-pager ;;
  logs) journalctl -u ${NAME} -n "\${2:-100}" --no-pager ;;
  follow) journalctl -u ${NAME} -f ;;
  restart) systemctl restart ${NAME} ;;
  stop) systemctl stop ${units} ;;
  start) systemctl start ${units} ;;
  setup-code)
    if [ -f "${DATA_DIR}/setup-code.txt" ]; then
      cat "${DATA_DIR}/setup-code.txt"; echo
    else
      echo "The setup was already done. There is no setup code."
    fi
    ;;
  add-admin)
    steam_id="\${2:?Usage: playstats-hub add-admin <SteamID64>}"
    (set -a; . "${ENV_FILE}"; set +a; cd "${APP_DIR}"; runuser -u ${SERVICE_USER} -- "${APP_DIR}/${EXECUTABLE}" admin add "\${steam_id}")
    ;;
  backup)
    target="\${2:-/root/${NAME}-backup-\$(date +%Y%m%d-%H%M%S).tar.gz}"
    systemctl stop ${units}
    trap 'systemctl start ${units}' EXIT
    key=()
    if [ -f "${SECRETS_KEY_FILE}" ]; then
      key=(-C "$(dirname "${SECRETS_KEY_FILE}")" "$(basename "${SECRETS_KEY_FILE}")")
    fi
    (umask 077; tar -czf "\${target}" -C "${DATA_DIR}" . -C "${CONFIG_DIR}" hub.env \${key[@]+"\${key[@]}"})
    echo "Backup written to \${target}"
    if [ \${#key[@]} -gt 0 ]; then
      echo "It also holds the secrets key, which decrypts the secrets saved in the settings: keep this file private."
    else
      echo "Warning: the secrets key is not in this file, because it is set in ${ENV_FILE}. Keep a copy of it: without it the secrets saved in the settings cannot be read."
    fi
    ;;
  restore)
    source="\${2:?Usage: playstats-hub restore <backup file>}"
    if ! tar -tzf "\${source}" 2>/dev/null | grep -qx 'hub.env'; then
      echo "\${source} is not a backup made by playstats-hub backup. Nothing was changed." >&2
      exit 1
    fi
    systemctl stop ${units}
    find "${DATA_DIR}" -mindepth 1 -delete
    tar -xzf "\${source}" -C "${DATA_DIR}" --exclude=hub.env --exclude="$(basename "${SECRETS_KEY_FILE}")"
    tar -xzf "\${source}" -C "${CONFIG_DIR}" hub.env
    if tar -tzf "\${source}" | grep -x "$(basename "${SECRETS_KEY_FILE}")" >/dev/null; then
      mkdir -p "$(dirname "${SECRETS_KEY_FILE}")"
      (umask 077; tar -xzf "\${source}" -O "$(basename "${SECRETS_KEY_FILE}")" >"${SECRETS_KEY_FILE}")
      chown ${SERVICE_USER}:${SERVICE_USER} "${SECRETS_KEY_FILE}"
      chmod 600 "${SECRETS_KEY_FILE}"
    else
      echo "This backup has no secrets key, so the current one in ${SECRETS_KEY_FILE} was kept. If the backup comes from another machine, copy the secrets key of that machine there, or the secrets saved in the settings stay empty."
    fi
    if ! grep -q '^Hub__SecretsKey\(File\)\?=' "${ENV_FILE}"; then
      printf '\nHub__SecretsKeyFile=%s\n' "${SECRETS_KEY_FILE}" >>"${ENV_FILE}"
    fi
    chown -R ${SERVICE_USER}:${SERVICE_USER} "${DATA_DIR}"
    chown root:${SERVICE_USER} "${ENV_FILE}"
    systemctl start ${units}
    echo "Restored from \${source}"
    ;;
  update)
    curl -fsSL "https://raw.githubusercontent.com/${REPOSITORY}/main/deploy/install.sh" | HUB_VERSION="\${2:-latest}" bash
    ;;
  *)
    echo "Usage: playstats-hub <command>"
    echo "  status        Show whether the application is running"
    echo "  logs [lines]  Show the latest log lines"
    echo "  follow        Follow the log"
    echo "  restart       Restart the application"
    echo "  stop | start  Stop or start the application"
    echo "  setup-code    Show the code asked by the first-run setup page"
    echo "  add-admin <id> Make a SteamID64 an administrator of the site"
    echo "  backup [file] Save the database, avatars, configuration and secrets key in one file"
    echo "  restore <file> Replace the current data with a backup file"
    echo "  update [tag]  Install the latest version, or the given one, keeping the data"
    ;;
esac
EOF

  chmod +x "${COMMAND_FILE}"
}

wait_for_application() {
  step "Waiting for the application to answer"

  for _ in $(seq 1 60); do
    if curl -fsS "http://127.0.0.1:${PORT}/api/ping" >/dev/null 2>&1; then
      return
    fi
    sleep 1
  done

  journalctl -u "${NAME}" -n 30 --no-pager || true
  fail "The application did not start. The last log lines are above. Run: playstats-hub logs"
}

open_firewall() {
  if command -v ufw >/dev/null 2>&1 && ufw status 2>/dev/null | grep -q "Status: active"; then
    step "Opening the firewall"
    ufw allow "${PORT}/tcp" >/dev/null

    if [ -n "${DOMAIN}" ]; then
      ufw allow 80/tcp >/dev/null
      ufw allow 443/tcp >/dev/null
    fi
  fi
}

install_proxy() {
  if [ -z "${DOMAIN}" ]; then
    return
  fi

  step "Installing Caddy to serve https://${DOMAIN}"

  if ! command -v caddy >/dev/null 2>&1; then
    if command -v apt-get >/dev/null 2>&1; then
      apt-get install -y -qq debian-keyring debian-archive-keyring apt-transport-https gnupg >/dev/null
      curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/gpg.key' | gpg --dearmor --yes -o /usr/share/keyrings/caddy-stable-archive-keyring.gpg
      curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/debian.deb.txt' >/etc/apt/sources.list.d/caddy-stable.list
      apt-get update -qq
      apt-get install -y -qq caddy >/dev/null
    else
      dnf install -y -q 'dnf-command(copr)' >/dev/null
      dnf copr enable -y -q @caddy/caddy >/dev/null
      dnf install -y -q caddy >/dev/null
    fi
  fi

  cat >/etc/caddy/Caddyfile <<EOF
${DOMAIN} {
    encode gzip
    reverse_proxy 127.0.0.1:${PORT}
}
EOF

  systemctl enable --quiet caddy
  systemctl restart caddy
}

print_summary() {
  local address
  local code="${DATA_DIR}/setup-code.txt"

  address="$(hostname -I 2>/dev/null | awk '{print $1}')"
  address="${address:-YOUR-SERVER-ADDRESS}"

  printf '\n\033[1;32mL4D2 PlayStats Hub is running.\033[0m\n\n'

  if [ -n "${DOMAIN}" ]; then
    printf '  Site address:            https://%s\n' "${DOMAIN}"
    printf '  Address for the game:    http://%s:%s  (plain HTTP, for the browser inside the game)\n' "${address}" "${PORT}"
  else
    printf '  Site address:            http://%s:%s\n' "${address}" "${PORT}"
  fi

  if [ -f "${code}" ]; then
    printf '\n  Next step: open the site address. It asks for this setup code:\n\n'
    printf '      \033[1;33m%s\033[0m\n\n' "$(cat "${code}")"
    printf '  Then sign in with Steam. That account becomes the administrator,\n'
    printf '  and the site guides you through the rest, including your game server.\n'
  else
    printf '\n  The setup was already done. Your data and configuration were kept.\n'
  fi

  printf '\n  Useful commands:\n'
  printf '      playstats-hub status       is it running?\n'
  printf '      playstats-hub logs         latest log lines\n'
  printf '      playstats-hub setup-code   show the setup code again\n'
  printf '      playstats-hub backup       save the database, configuration and secrets key\n'
  printf '      playstats-hub update       install the latest version\n'
  printf '\n  Files:\n'
  printf '      %s   application\n' "${APP_DIR}"
  printf '      %s   database, avatars and keys\n' "${DATA_DIR}"
  printf '      %s   configuration\n' "${ENV_FILE}"
  if [ -f "${SECRETS_KEY_FILE}" ]; then
    printf '      %s   secrets key: keep a copy, the backup zip of the site does not have it\n' "${SECRETS_KEY_FILE}"
  fi
  printf '\n'
}

require_root
detect_runtime
install_packages
download_package
install_application
write_configuration
write_secrets_key
write_service
write_command
wait_for_application
open_firewall
install_proxy
print_summary
