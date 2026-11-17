# Backups and updates

🌐 **English** | [Português](backups-and-updates.pt.md) | [Español](backups-and-updates.es.md)

Everything the site keeps is in one folder, the data folder. Back it up, and you can rebuild the site on any machine.

[Back to the manual](README.md)

## Contents

- [What is in the data folder](#what-is-in-the-data-folder)
- [Backing up on Linux](#backing-up-on-linux)
- [Backing up with Docker](#backing-up-with-docker)
- [Restoring](#restoring)
- [Updating](#updating)
- [What is lost on a restart](#what-is-lost-on-a-restart)

## What is in the data folder

| Item | What it is |
|---|---|
| `hub.db` (and `hub.db-wal`, `hub.db-shm` while the site runs) | The SQLite database: every statistics file received, the matches, the settings, the skins, the votes, everything. |
| `avatars/` | The Steam avatars the site downloaded. They can be downloaded again. |
| `branding/` | The logo uploaded in the settings. |
| `keys/` | The keys that protect the sign-in cookies. Without them, everybody has to sign in again. |
| `setup-code.txt` | The setup code, only until the setup is done. |

The folder is `/var/lib/l4d2-playstats-hub` with the Linux installer and the `hub-data` volume (mounted at `/data`) with Docker. On Linux, the few settings outside the database are in `/etc/l4d2-playstats-hub/hub.env`.

With SQL Server, PostgreSQL or MySQL, the data is in that database: back it up with the tools of that database, and keep a copy of the data folder for the keys and the logo. See [Databases](../databases.md).

Because every statistics file is kept, the database is enough to rebuild all the matches and rankings.

## Backing up on Linux

```bash
playstats-hub backup
```

![A backup on Linux](../images/en/install-linux-backup.webp)

The site stops for a few seconds while the copy is made. The file goes to `/root` with the date in its name, and contains the data folder and `hub.env`. Give another path to save it elsewhere: `playstats-hub backup /backups/hub.tar.gz`. Copy the file to another machine.

To make one every night, add a line to the root crontab (`sudo crontab -e`):

```text
0 4 * * * /usr/local/bin/playstats-hub backup
```

Old backups are not deleted by themselves.

## Backing up with Docker

Stop the container, copy the volume into a file, start it again:

![A backup with Docker](../images/en/install-docker-backup.webp)

The volume name has the name of your folder in front of it, such as `playstats_hub-data`. `docker volume ls` shows it. [Installing with Docker](../install-docker.md#backup) has the commands.

## Restoring

On Linux, on the same machine or on a new one where you ran the installer:

```bash
playstats-hub restore /root/l4d2-playstats-hub-backup-20270115-030000.tar.gz
```

It replaces the current data and `hub.env` with the ones in the file and starts the site again.

With Docker, stop the container, extract the file into the volume, and start it again:

```bash
docker compose stop hub
```

```bash
docker run --rm -v playstats_hub-data:/data -v "$PWD":/backup alpine sh -c "rm -rf /data/* && tar -xzf /backup/hub-backup.tar.gz -C /data"
```

```bash
docker compose start hub
```

## Updating

New versions are published in the [releases](https://github.com/altair-sossai/l4d2-playstats-hub/releases). An update keeps the data and the settings. When a version changes the database, the site updates the tables by itself when it starts.

On Linux:

```bash
playstats-hub update
```

![An update on Linux](../images/en/install-linux-update.webp)

To install a specific version: `playstats-hub update v1.2.0`.

With Docker:

![An update with Docker](../images/en/install-docker-update.webp)

A backup before an update costs a minute and lets you go back.

The plugins of the game servers are released with the site. Check the release notes to see whether they changed; [Connecting a game server](../game-server.md) explains how to replace them.

## What is lost on a restart

A restart, an update or a restore keeps everything stored. Only what lives in memory starts empty:

- the match in progress on the live server page, until the game servers send the next update;
- the site chat messages and the commands waiting to be delivered;
- the connection status of the game servers, until each one sends its next signal.
