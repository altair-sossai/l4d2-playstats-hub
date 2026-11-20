# Installing on Azure

🌐 **English** | [Português](install-azure.pt.md) | [Español](install-azure.es.md)

There are two ways to run L4D2 PlayStats Hub on Azure. The first is the same as on any VPS and is the one we recommend.

> **Status of this guide.** The virtual machine path uses the Linux installer, which is tested. The App Service path was written from the Azure documentation and has not been run end to end yet, so this page has no screenshots of it. If a step does not match what you see, please open an issue.

## Contents

- [Option 1: a virtual machine](#option-1-a-virtual-machine)
- [Option 2: App Service with a container](#option-2-app-service-with-a-container)
- [Using Azure SQL](#using-azure-sql)

## Option 1: a virtual machine

Create a Linux virtual machine and follow [Installing on a Linux VPS](install-linux.md). Nothing is different, and the screenshots of that guide show what you will see.

| Setting | Suggested value |
|---|---|
| Image | Ubuntu Server 24.04 LTS |
| Size | B1s (1 CPU, 1 GB) is enough to start |
| Inbound ports | 22 for SSH and 5080 for the site, or 80 and 443 if you use a domain |

In the portal, the inbound ports are in the **Networking** page of the virtual machine. The site does not open until the port is allowed there.

## Option 2: App Service with a container

App Service runs the Docker image that every release publishes. You need the [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli), signed in with `az login`.

Choose a name for the site. It must be unique in Azure, because the address becomes `https://NAME.azurewebsites.net`. The commands below use `my-playstats`.

Create a resource group and a Linux plan:

```bash
az group create --name playstats --location brazilsouth
```

```bash
az appservice plan create --resource-group playstats --name playstats-plan --is-linux --sku B1
```

Create the site from the image:

```bash
az webapp create --resource-group playstats --plan playstats-plan --name my-playstats --container-image-name ghcr.io/altair-sossai/l4d2-playstats-hub:latest
```

Tell App Service which port the application listens on, and keep the data on the persistent disk:

```bash
az webapp config appsettings set --resource-group playstats --name my-playstats --settings WEBSITES_PORT=8080 WEBSITES_ENABLE_APP_SERVICE_STORAGE=true Hub__DataDirectory=/home/data
```

Keep the site on a single instance. The live match is kept in memory, and two instances would each have their own:

```bash
az appservice plan update --resource-group playstats --name playstats-plan --number-of-workers 1
```

Turn on WebSockets, which the [live server page](manual/live-server.md) uses for its updates. Without them the page still works, through Server-Sent Events or long polling:

```bash
az webapp config set --resource-group playstats --name my-playstats --web-sockets-enabled true
```

Read the setup code from the log:

```bash
az webapp log config --resource-group playstats --name my-playstats --docker-container-logging filesystem
```

```bash
az webapp log tail --resource-group playstats --name my-playstats
```

Look for the line that says `Setup is pending`. Then open `https://my-playstats.azurewebsites.net`, paste the code, sign in with Steam and follow the pages. [First-run setup](manual/first-run.md) shows each of them.

Two things to know about App Service:

- **The database.** The persistent disk of App Service is a network share, and SQLite is not reliable on network shares. For anything beyond a trial, use Azure SQL as described below.
- **The game.** The `!ranking` command inside the game needs a plain HTTP address, because the browser built into the game is not expected to open current HTTPS sites. Allow plain HTTP on the site with the command below, and use `http://my-playstats.azurewebsites.net` as the address for the game.

```bash
az webapp update --resource-group playstats --name my-playstats --https-only false
```

## Using Azure SQL

Create a server and a database. The Basic tier is enough.

```bash
az sql server create --resource-group playstats --name my-playstats-sql --admin-user hubadmin --admin-password "CHOOSE-A-STRONG-PASSWORD"
```

```bash
az sql db create --resource-group playstats --server my-playstats-sql --name hub --service-objective Basic
```

Allow Azure services to reach the server:

```bash
az sql server firewall-rule create --resource-group playstats --server my-playstats-sql --name azure --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```

Point the site to it:

```bash
az webapp config appsettings set --resource-group playstats --name my-playstats --settings Database__Provider=SqlServer Database__ConnectionString="Server=tcp:my-playstats-sql.database.windows.net,1433;Database=hub;User ID=hubadmin;Password=CHOOSE-A-STRONG-PASSWORD;Encrypt=True"
```

The application creates its tables when it starts. [Databases](databases.md) has more about each supported database.
