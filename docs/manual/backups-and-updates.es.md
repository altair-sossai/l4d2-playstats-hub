# Copias de seguridad y actualizaciones

🌐 [English](backups-and-updates.md) | [Português](backups-and-updates.pt.md) | **Español**

Todo lo que guarda el sitio está en una sola carpeta, la carpeta de datos. Haz una copia de seguridad de ella y podrás reconstruir el sitio en cualquier máquina.

[Volver al manual](README.es.md)

## Contenido

- [Qué hay en la carpeta de datos](#qué-hay-en-la-carpeta-de-datos)
- [Copia de seguridad en Linux](#copia-de-seguridad-en-linux)
- [Copia de seguridad con Docker](#copia-de-seguridad-con-docker)
- [Restaurar](#restaurar)
- [Actualizar](#actualizar)
- [Qué se pierde al reiniciar](#qué-se-pierde-al-reiniciar)

## Qué hay en la carpeta de datos

| Elemento | Qué es |
|---|---|
| `hub.db` (y `hub.db-wal`, `hub.db-shm` mientras el sitio está en ejecución) | La base de datos SQLite: cada archivo de estadísticas recibido, las partidas, los ajustes, las skins, los votos, todo. |
| `avatars/` | Los avatares de Steam que descargó el sitio. Se pueden volver a descargar. |
| `branding/` | El logo subido en la configuración. |
| `keys/` | Las claves que protegen las cookies de inicio de sesión. Sin ellas, todos tienen que volver a iniciar sesión. |
| `setup-code.txt` | El código de configuración, solo hasta que termina la configuración inicial. |

La carpeta es `/var/lib/l4d2-playstats-hub` con el instalador para Linux y el volumen `hub-data` (montado en `/data`) con Docker. En Linux, los pocos ajustes que están fuera de la base de datos están en `/etc/l4d2-playstats-hub/hub.env`.

Con SQL Server, PostgreSQL o MySQL, los datos están en esa base de datos: haz la copia de seguridad con las herramientas de esa base de datos y guarda una copia de la carpeta de datos por las claves y el logo. Consulta [Bases de datos](../databases.es.md).

Como se guarda cada archivo de estadísticas, la base de datos alcanza para reconstruir todas las partidas y los rankings.

## Copia de seguridad en Linux

```bash
playstats-hub backup
```

![Una copia de seguridad en Linux](../images/es/install-linux-backup.webp)

El sitio se detiene unos segundos mientras se hace la copia. El archivo queda en `/root` con la fecha en el nombre, y contiene la carpeta de datos y `hub.env`. Indica otra ruta para guardarlo en otro lugar: `playstats-hub backup /backups/hub.tar.gz`. Copia el archivo a otra máquina.

Para hacer una cada noche, agrega una línea al crontab de root (`sudo crontab -e`):

```text
0 4 * * * /usr/local/bin/playstats-hub backup
```

Las copias antiguas no se borran solas.

## Copia de seguridad con Docker

Detén el contenedor, copia el volumen a un archivo y vuelve a iniciarlo:

![Una copia de seguridad con Docker](../images/es/install-docker-backup.webp)

El nombre del volumen lleva delante el nombre de tu carpeta, como `playstats_hub-data`. `docker volume ls` lo muestra. [Instalar con Docker](../install-docker.es.md#copia-de-seguridad) tiene los comandos.

## Restaurar

En Linux, en la misma máquina o en una nueva donde hayas ejecutado el instalador:

```bash
playstats-hub restore /root/l4d2-playstats-hub-backup-20270115-030000.tar.gz
```

Reemplaza los datos actuales y `hub.env` por los del archivo y vuelve a iniciar el sitio.

Con Docker, detén el contenedor, extrae el archivo en el volumen y vuelve a iniciarlo:

```bash
docker compose stop hub
```

```bash
docker run --rm -v playstats_hub-data:/data -v "$PWD":/backup alpine sh -c "rm -rf /data/* && tar -xzf /backup/hub-backup.tar.gz -C /data"
```

```bash
docker compose start hub
```

## Actualizar

Las versiones nuevas se publican en las [versiones publicadas](https://github.com/altair-sossai/l4d2-playstats-hub/releases). Una actualización conserva los datos y los ajustes. Cuando una versión cambia la base de datos, el sitio actualiza las tablas por sí solo al iniciar.

En Linux:

```bash
playstats-hub update
```

![Una actualización en Linux](../images/es/install-linux-update.webp)

Para instalar una versión específica: `playstats-hub update v1.2.0`.

Con Docker:

![Una actualización con Docker](../images/es/install-docker-update.webp)

Una copia de seguridad antes de actualizar lleva un minuto y te permite volver atrás.

Los plugins de los servidores de juego se publican junto con el sitio. Revisa las notas de la versión para ver si cambiaron; [Conectar un servidor de juego](../game-server.es.md) explica cómo reemplazarlos.

## Qué se pierde al reiniciar

Un reinicio, una actualización o una restauración conservan todo lo guardado. Solo lo que vive en memoria empieza vacío:

- la partida en curso en la página del servidor en vivo, hasta que los servidores de juego envíen la próxima actualización;
- los mensajes del chat del sitio y los comandos que esperan ser entregados;
- el estado de la conexión de los servidores de juego, hasta que cada uno envíe su próxima señal.
