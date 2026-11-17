# Installing with Docker

🌐 **English** | [Português](install-docker.pt.md) | [Español](install-docker.es.md)

Use this if you already run your services in containers. If you just have a Linux VPS, the [installer](install-linux.md) is simpler.

The screenshots on this page are the real output of the commands, recorded on a clean Ubuntu 24.04 machine with Docker 29 and Compose 2.40, in a folder named `playstats`. Your times and setup code will be different.

## Contents

- [What you need](#what-you-need)
- [Starting it](#starting-it)
- [The setup code](#the-setup-code)
- [Updating](#updating)
- [Backup](#backup)
- [Adding an administrator](#adding-an-administrator)
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

The first time, Docker downloads the image, then creates the network, the volume and the container:

![docker compose up -d, the first time](images/en/install-docker-up.webp)

The site is at `http://YOUR-SERVER-ADDRESS:5080`. The database, the avatars and the keys are in the `hub-data` volume, so they survive updates.

Run only one container of the application. The live match is kept in memory, and two containers would each have their own.

## The setup code

The first page of the site asks for a setup code. Read it from the log:

```bash
docker compose logs hub | grep "setup code"
```

![The setup code in the log, and the running container](images/en/install-docker-setup-code.webp)

`docker compose ps` shows the container and its port. Open the site, paste the code, sign in with Steam, and follow the pages. [First-run setup](manual/first-run.md) shows each of them.

![The first page of the site asks for the setup code](images/en/wizard-setup-code.webp)

## Updating

```bash
docker compose pull
```

```bash
docker compose up -d
```

`pull` downloads the newest image, and `up -d` recreates the container when the image changed. The data in the volume is kept, and the site updates its tables by itself when it starts. Below, the image was already the newest, so the container kept running:

![Updating with Docker](images/en/install-docker-update.webp)

To stay on a specific version, replace `latest` in the compose file with the version, such as `v1.2.0`.

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

The volume may be named with your folder as a prefix, such as `myfolder_hub-data`. `docker volume ls` shows the exact name. In the example below the folder is `playstats`, so the volume is `playstats_hub-data`:

![A backup with Docker](images/en/install-docker-backup.webp)

To restore, stop the container, empty the volume and extract the file into it, then start it again:

```bash
docker compose stop hub
```

```bash
docker run --rm -v hub-data:/data -v "$PWD":/backup alpine sh -c "rm -rf /data/* && tar -xzf /backup/hub-backup.tar.gz -C /data"
```

```bash
docker compose start hub
```

## Adding an administrator

If you lose access to every administrator account, add one from the machine:

```bash
docker compose exec hub dotnet L4D2PlayStats.Hub.Web.dll admin add <SteamID64>
```

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
