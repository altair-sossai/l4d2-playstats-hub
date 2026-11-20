# Instalar con Docker

🌐 [English](install-docker.md) | [Português](install-docker.pt.md) | **Español**

Usa esta opción si ya ejecutas tus servicios en contenedores. Si solo tienes un VPS Linux, el [instalador](install-linux.es.md) es más sencillo.

Las capturas de pantalla de esta página son la salida real de los comandos, grabada en una máquina Ubuntu 24.04 limpia con Docker 29 y Compose 5.8, en una carpeta llamada `playstats`. Tus horarios y tu código de configuración serán distintos.

## Contenido

- [Lo que necesitas](#lo-que-necesitas)
- [Cómo iniciarlo](#cómo-iniciarlo)
- [El código de configuración](#el-código-de-configuración)
- [Comprobación de estado](#comprobación-de-estado)
- [Actualizar](#actualizar)
- [Copia de seguridad](#copia-de-seguridad)
- [Agregar un administrador](#agregar-un-administrador)
- [Configuración](#configuración)
- [Construir la imagen tú mismo](#construir-la-imagen-tú-mismo)

## Lo que necesitas

Una máquina con Docker y el plugin Compose. La imagen se publica en `ghcr.io/altair-sossai/l4d2-playstats-hub` con cada versión.

## Cómo iniciarlo

Guarda esto como `docker-compose.yml`:

```yaml
services:
  hub:
    image: ghcr.io/altair-sossai/l4d2-playstats-hub:latest
    restart: unless-stopped
    ports:
      - "5080:8080"
    volumes:
      - hub-data:/data

volumes:
  hub-data:
```

Luego inícialo:

```bash
docker compose up -d
```

La primera vez, Docker descarga la imagen y luego crea la red, el volumen y el contenedor:

![docker compose up -d, la primera vez](images/es/install-docker-up.webp)

El sitio queda en `http://YOUR-SERVER-ADDRESS:5080`. La base de datos, los avatares y las claves están en el volumen `hub-data`, así que se conservan entre actualizaciones.

Ejecuta un solo contenedor de la aplicación. La partida en vivo se guarda en memoria, y dos contenedores tendrían cada uno la suya.

La aplicación se ejecuta con el usuario sin privilegios `app` de la imagen de .NET, no como root. Cuando el contenedor inicia, primero le da a ese usuario la propiedad de la carpeta de datos y después inicia el sitio.

## El código de configuración

La primera página del sitio pide un código de configuración. Léelo en el registro:

```bash
docker compose logs hub | grep "setup code"
```

![El código de configuración en el registro, y el contenedor en ejecución](images/es/install-docker-setup-code.webp)

`docker compose ps` muestra el contenedor y su puerto. Abre el sitio, pega el código, inicia sesión con Steam y sigue las páginas. [Configuración inicial](manual/first-run.es.md) muestra cada una de ellas.

![La primera página del sitio pide el código de configuración](images/es/wizard-setup-code.webp)

## Comprobación de estado

La imagen tiene una comprobación de estado que no necesita herramientas adicionales: cada 30 segundos Docker ejecuta `dotnet L4D2PlayStats.Hub.Web.dll health` dentro del contenedor, que le pide `/api/ping` al sitio en su propio puerto. `docker compose ps` muestra el contenedor como `healthy` cuando el sitio responde, y como `unhealthy` después de tres fallos seguidos. Para ejecutar la comprobación a mano:

```bash
docker compose exec hub dotnet L4D2PlayStats.Hub.Web.dll health
```

Muestra la respuesta y termina con 0 cuando el sitio está funcionando, o con 1 cuando no.

## Actualizar

```bash
docker compose pull
```

```bash
docker compose up -d
```

`pull` descarga la imagen más reciente, y `up -d` vuelve a crear el contenedor cuando la imagen cambió. Los datos del volumen se conservan, y el sitio actualiza sus tablas por sí solo al iniciar. Abajo, la imagen ya era la más reciente, así que el contenedor siguió en ejecución:

![Actualizar con Docker](images/es/install-docker-update.webp)

Para quedarte en una versión específica, reemplaza `latest` en el archivo de Compose por la versión, como `v1.2.0`.

Las imágenes hasta la v0.3.0 ejecutaban la aplicación como root, así que los archivos de un volumen existente pertenecen a root. Un contenedor más nuevo cambia su propietario a `app` al iniciar, así que la actualización no necesita nada más. Si inicias el contenedor con un `user:` fijo en el archivo de Compose, no puede cambiar el propietario; corrígelo una vez con `docker run --rm -v hub-data:/data alpine chown -R 1654:1654 /data` (1654 es el usuario `app` de la imagen).

## Copia de seguridad

Detén el contenedor, copia el volumen a un archivo y vuelve a iniciarlo:

```bash
docker compose stop hub
```

```bash
docker run --rm -v hub-data:/data -v "$PWD":/backup alpine tar -czf /backup/hub-backup.tar.gz -C /data .
```

```bash
docker compose start hub
```

El volumen puede llevar el nombre de tu carpeta como prefijo, por ejemplo `myfolder_hub-data`. `docker volume ls` muestra el nombre exacto. En el ejemplo de abajo la carpeta es `playstats`, así que el volumen es `playstats_hub-data`:

![Una copia de seguridad con Docker](images/es/install-docker-backup.webp)

Para restaurar, detén el contenedor, vacía el volumen y extrae el archivo en él, y luego vuelve a iniciarlo:

```bash
docker compose stop hub
```

```bash
docker run --rm -v hub-data:/data -v "$PWD":/backup alpine sh -c "rm -rf /data/* && tar -xzf /backup/hub-backup.tar.gz -C /data"
```

```bash
docker compose start hub
```

## Agregar un administrador

Si pierdes el acceso a todas las cuentas de administrador, agrega una desde la máquina:

```bash
docker compose exec -u app hub dotnet L4D2PlayStats.Hub.Web.dll admin add <SteamID64>
```

`-u app` ejecuta el comando con el mismo usuario que el sitio, así que los archivos que toca conservan el propietario correcto.

## Configuración

Los ajustes son variables de entorno. Agrégalas bajo `environment:` en el archivo de Compose.

| Variable | Significado | Valor predeterminado |
|---|---|---|
| `Hub__DataDirectory` | Carpeta con la base de datos, los avatares y las claves. | `/data` |
| `Database__Provider` | `Sqlite`, `SqlServer`, `PostgreSql` o `MySql`. | `Sqlite` |
| `Database__ConnectionString` | Cómo conectarse a la base de datos. | `Data Source=hub.db` |
| `Hub__TrustedProxies` | Proxies inversos cuyas cabeceras `X-Forwarded-For`, `X-Forwarded-Proto` y `X-Forwarded-Host` acepta el sitio: direcciones IP o redes como `172.16.0.0/12`, separadas por comas. El propio contenedor (`127.0.0.1` y `::1`) siempre es de confianza. | vacío |

Si pones un proxy inverso con HTTPS delante del contenedor, mantén una dirección HTTP simple accesible para el juego. [La dirección usada dentro del juego](install-linux.es.md#la-dirección-usada-dentro-del-juego) explica por qué.

Un proxy inverso nunca está en la dirección de loopback del contenedor: tanto si se ejecuta en otro contenedor como en el host, el sitio normalmente ve una dirección privada de la red de Docker, como `172.18.0.2` o la puerta de enlace `172.18.0.1`. Mientras esa dirección no esté en `Hub__TrustedProxies`, el sitio ignora las cabeceras reenviadas, así que ve `http` en lugar de `https` y la dirección del proxy en lugar de la del visitante. Confía en la red de tus contenedores:

```yaml
services:
  hub:
    environment:
      Hub__TrustedProxies: "172.16.0.0/12"
```

`docker network inspect` muestra la subred exacta si prefieres un rango más estrecho. No confíes en una red a la que los visitantes llegan directamente, o cualquiera de ellos podría fingir que viene de otro lugar.

La [página del servidor en vivo](manual/live-server.es.md) recibe sus actualizaciones por un WebSocket en `/hubs/live`, así que deja que el proxy pase WebSockets (en nginx, las líneas de [Cambiar desde el sitio antiguo](switch-from-old-site.es.md#parte-3-el-cambio)). Sin eso la página sigue funcionando, por Server-Sent Events o long polling.

## Construir la imagen tú mismo

Desde un clon del repositorio:

```bash
docker build -t l4d2-playstats-hub .
```
