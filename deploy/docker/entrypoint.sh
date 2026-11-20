#!/bin/sh
set -e

if [ "${WEBSITES_ENABLE_APP_SERVICE_STORAGE:-}" = "true" ] && [ "${Hub__SecretsKeyFile:-}" = "/secrets/secrets.key" ]; then
    Hub__SecretsKeyFile=/home/secrets/secrets.key
    export Hub__SecretsKeyFile
fi

if [ "$(id -u)" = "0" ]; then
    data="${Hub__DataDirectory:-/data}"
    mkdir -p "$data"
    find "$data" \( ! -user app -o ! -group app \) -exec chown -h app:app {} +

    secrets="$(dirname "${Hub__SecretsKeyFile:-/secrets/secrets.key}")"
    case "$secrets" in
        /secrets | /home/secrets)
            mkdir -p "$secrets"
            chown app:app "$secrets" || true
            chmod 700 "$secrets" || true
            find "$secrets" -maxdepth 1 -type f \( ! -user app -o ! -group app \) -exec chown -h app:app {} + || true
            ;;
    esac

    exec setpriv --reuid=app --regid=app --init-groups -- dotnet /app/L4D2PlayStats.Hub.Web.dll "$@"
fi

exec dotnet /app/L4D2PlayStats.Hub.Web.dll "$@"
