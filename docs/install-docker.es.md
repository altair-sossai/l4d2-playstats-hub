# Instalar con Docker

🌐 [English](install-docker.md) | [Português](install-docker.pt.md) | **Español**

Usa esta opción si ya ejecutas tus servicios en contenedores. Si solo tienes un VPS Linux, el [instalador](install-linux.es.md) es más sencillo.

## Contenido

- [Lo que necesitas](#lo-que-necesitas)
- [Cómo iniciarlo](#cómo-iniciarlo)
- [El código de configuración](#el-código-de-configuración)
- [Actualizar](#actualizar)
- [Copia de seguridad](#copia-de-seguridad)
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

El sitio queda en `http://YOUR-SERVER-ADDRESS:5080`. La base de datos, los avatares y las claves están en el volumen `hub-data`, así que se conservan entre actualizaciones.

Ejecuta un solo contenedor de la aplicación. La partida en vivo se guarda en memoria, y dos contenedores tendrían cada uno la suya.

## El código de configuración

La primera página del sitio pide un código de configuración. Léelo en el registro:

```bash
docker compose logs hub | grep "setup code"
```

Pégalo en el sitio, inicia sesión con Steam y sigue las páginas. [Instalar en un VPS Linux](install-linux.es.md#paso-4-sigue-la-configuración) las describe.

## Actualizar

```bash
docker compose pull
```

```bash
docker compose up -d
```

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

El volumen puede llevar el nombre de tu carpeta como prefijo, por ejemplo `myfolder_hub-data`. `docker volume ls` muestra el nombre exacto.

## Configuración

Los ajustes son variables de entorno. Agrégalas bajo `environment:` en el archivo de Compose.

| Variable | Significado | Valor predeterminado |
|---|---|---|
| `Hub__DataDirectory` | Carpeta con la base de datos, los avatares y las claves. | `/data` |
| `Database__Provider` | `Sqlite`, `SqlServer`, `PostgreSql` o `MySql`. | `Sqlite` |
| `Database__ConnectionString` | Cómo conectarse a la base de datos. | `Data Source=hub.db` |

Si pones un proxy inverso con HTTPS delante del contenedor, mantén una dirección HTTP simple accesible para el juego. [La dirección usada dentro del juego](install-linux.es.md#la-dirección-usada-dentro-del-juego) explica por qué.

## Construir la imagen tú mismo

Desde un clon del repositorio:

```bash
docker build -t l4d2-playstats-hub .
```
