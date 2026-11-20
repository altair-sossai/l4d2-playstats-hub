# Backups and updates

🌐 **English** | [Português](backups-and-updates.pt.md) | [Español](backups-and-updates.es.md)

Everything the site keeps is in one folder, the data folder. Back it up, and you can rebuild the site on any machine.

[Back to the manual](README.md)

## Contents

- [What is in the data folder](#what-is-in-the-data-folder)
- [Automatic backup to Azure Storage](#automatic-backup-to-azure-storage)
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

## Automatic backup to Azure Storage

The site can back itself up. In [Administration > Backup](administration.md#backup) you give it an Azure Storage container, and every few hours, 8 by default, it uploads one zip file there. The site keeps running: the database is copied while it works.

### What the zip contains

| File | What it is |
|---|---|
| `hub.db` | A copy of the SQLite database, made with the backup function of SQLite while the site runs, so it is consistent even while statistics arrive. |
| `keys/` | The keys that protect the sign-in cookies. |
| `branding/` | The logo. |
| `manifest.json` | The version of the site, the time of the backup in UTC, the database provider, the list of files and how many matches, statistics files, players and settings the site had at that moment. |

- The avatars are left out: the site downloads them again.
- `hub.env` (Linux) or the environment variables (Docker) are outside the data folder and are not in the zip. Keep a copy of them, or run `playstats-hub backup` now and then, which includes `hub.env`.
- With SQL Server, PostgreSQL or MySQL the zip has no `hub.db`: back that database up with its own tools. The zip still keeps the keys and the logo.

The files are named `<prefix>-yyyyMMdd-HHmmss.zip`, in UTC, such as `hub-backup-20261119-160000.zip`. After each upload, the oldest files with the same prefix beyond the number to keep (30 by default) are deleted. Other files in the container are never touched. A backup that fails is tried again 30 minutes later, and the page shows the error.

### Checking that each backup restores

Right after each upload the site downloads the zip again from the container, the same file a restore would use, and checks it in a temporary folder:

- the size is the same as what was uploaded, the zip opens and every file listed in `manifest.json` is there and can be read;
- `hub.db` opens read-only and passes `PRAGMA integrity_check`, the full consistency check of SQLite;
- `hub.db` has the same number of matches, statistics files, players and settings as the site had when the copy was made, as recorded in the manifest.

The temporary folder is deleted afterwards, whatever the result. The result goes in the **Verified** column of **Recent backups**: a failure shows the reason, and it is written to the log without secrets. When the check of the new zip fails, the older files are not deleted, so the last good copies stay in the container. **Verify now**, on the same page, runs the check again on the newest zip of the container.

The check needs SQLite: with SQL Server, PostgreSQL or MySQL the page shows **Not available**. Each backup takes a few seconds more, and the temporary folder needs, for a moment, free space for the zip plus the database.

### Creating the container and its SAS address

The safest destination is a SAS address of one container: with it the site can only use that container.

In the Azure portal:

1. Open the storage account, **Data storage > Containers**, and create a container, such as `hub-backups`, with private access.
2. Open the container and **Settings > Shared access tokens**. Sign with an account key, tick the permissions **Read, Add, Create, Write, Delete and List**, choose a distant expiry date and click **Generate SAS token and URL**.
3. Copy the **Blob SAS URL**. It looks like `https://<account>.blob.core.windows.net/hub-backups?sv=...&sig=...`.

With the Azure CLI:

```bash
az storage container create --account-name <account> --name hub-backups --auth-mode key
```

```bash
az storage container generate-sas --account-name <account> --name hub-backups --permissions racwdl --expiry 2028-12-31T23:59Z --https-only --auth-mode key -o tsv
```

The second command prints the token. The address is `https://<account>.blob.core.windows.net/hub-backups?` followed by it.

Paste the address in **Administration > Backup**, tick **Back up automatically**, save and click **Test connection**. The SAS stops working on its expiry date: create a new one before that, or the backups start to fail, as the page shows.

A connection string of the storage account also works, together with a container name; the site creates the container when it does not exist. It gives access to the whole account, so prefer the SAS address.

### Restoring a zip

Download the zip from the container: in the portal, open the file and click **Download**; with the CLI, `az storage blob download`.

On Linux, stop the site, put the files of the zip in the data folder and start it again. `unzip` may need `sudo apt install unzip`.

```bash
sudo systemctl stop l4d2-playstats-hub
```

```bash
sudo rm -f /var/lib/l4d2-playstats-hub/hub.db-wal /var/lib/l4d2-playstats-hub/hub.db-shm
```

```bash
sudo unzip -o hub-backup-20261119-160000.zip -x manifest.json -d /var/lib/l4d2-playstats-hub
```

```bash
sudo chown -R playstats:playstats /var/lib/l4d2-playstats-hub
```

```bash
sudo systemctl start l4d2-playstats-hub
```

Delete the `-wal` and `-shm` files before extracting: they belong to the database being replaced. On a new machine, run the installer first, then restore, and put your `hub.env` back if you changed it.

With Docker, stop the container, extract the zip into the volume and start it again. The container gives the files to its user when it starts.

```bash
docker compose stop hub
```

```bash
docker run --rm -v playstats_hub-data:/data -v "$PWD":/backup alpine sh -c "rm -f /data/hub.db-wal /data/hub.db-shm && unzip -o /backup/hub-backup-20261119-160000.zip -x manifest.json -d /data"
```

```bash
docker compose start hub
```

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

Images up to v0.3.0 ran the site as root; newer ones run it as the `app` user. A volume created by an older image is handed to that user when the new container starts, so nothing else is needed. [Installing with Docker > Updating](../install-docker.md#updating) has the details.

A backup before an update costs a minute and lets you go back.

The plugins of the game servers are released with the site. Check the release notes to see whether they changed; [Connecting a game server](../game-server.md) explains how to replace them.

## What is lost on a restart

A restart, an update or a restore keeps everything stored. Only what lives in memory starts empty:

- the match in progress on the live server page, until the game servers send the next update;
- the site chat messages and the commands waiting to be delivered;
- the connection status of the game servers, until each one sends its next signal.
