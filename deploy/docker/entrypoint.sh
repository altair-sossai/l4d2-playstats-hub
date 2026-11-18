#!/bin/sh
set -e

if [ "$(id -u)" = "0" ]; then
    data="${Hub__DataDirectory:-/data}"
    mkdir -p "$data"
    find "$data" \( ! -user app -o ! -group app \) -exec chown -h app:app {} +
    exec setpriv --reuid=app --regid=app --init-groups -- dotnet /app/L4D2PlayStats.Hub.Web.dll "$@"
fi

exec dotnet /app/L4D2PlayStats.Hub.Web.dll "$@"
