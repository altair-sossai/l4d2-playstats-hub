# Installing with Docker

🌐 **English** | [Português](install-docker.pt.md) | [Español](install-docker.es.md)

Use this if you already run your services in containers. If you just have a Linux VPS, the [installer](install-linux.md) is simpler.

## Contents

- [What you need](#what-you-need)
- [Starting it](#starting-it)
- [The setup code](#the-setup-code)
- [Updating](#updating)
- [Backup](#backup)
- [Configuration](#configuration)
- [Building the image yourself](#building-the-image-yourself)

## What you need

A machine with Docker and the Compose plugin. The image is published at `ghcr.io/altair-sossai/l4d2-playstats-hub` with every release.

## Starting it

Save this as `docker-compose.yml`:

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

Then start it:

```bash
docker compose up -d
```

The site is at `http://YOUR-SERVER-ADDRESS:5080`. The database, the avatars and the keys are in the `hub-data` volume, so they survive updates.

Run only one container of the application. The live match is kept in memory, and two containers would each have their own.

## The setup code

The first page of the site asks for a setup code. Read it from the log:

```bash
docker compose logs hub | grep "setup code"
```

Paste it on the site, sign in with Steam, and follow the pages. [Installing on a Linux VPS](install-linux.md#step-4-follow-the-setup) describes them.

## Updating

```bash
docker compose pull
```

```bash
docker compose up -d
```

## Backup

Stop the container, copy the volume into a file, and start it again:

```bash
docker compose stop hub
```

```bash
docker run --rm -v hub-data:/data -v "$PWD":/backup alpine tar -czf /backup/hub-backup.tar.gz -C /data .
```

```bash
docker compose start hub
```

The volume may be named with your folder as a prefix, such as `myfolder_hub-data`. `docker volume ls` shows the exact name.

## Configuration

Settings are environment variables. Add them under `environment:` in the compose file.

| Variable | Meaning | Default |
|---|---|---|
| `Hub__DataDirectory` | Folder with the database, avatars and keys. | `/data` |
| `Database__Provider` | `Sqlite`, `SqlServer`, `PostgreSql` or `MySql`. | `Sqlite` |
| `Database__ConnectionString` | How to reach the database. | `Data Source=hub.db` |

If you put a reverse proxy with HTTPS in front of the container, keep a plain HTTP address reachable for the game. [The address used inside the game](install-linux.md#the-address-used-inside-the-game) explains why.

## Building the image yourself

From a clone of the repository:

```bash
docker build -t l4d2-playstats-hub .
```
