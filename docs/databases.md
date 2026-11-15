# Databases

🌐 **English** | [Português](databases.pt.md) | [Español](databases.es.md)

L4D2 PlayStats Hub stores its data in a relational database. You choose which one with two settings.

## Contents

- [Which one to use](#which-one-to-use)
- [The two settings](#the-two-settings)
- [SQLite](#sqlite)
- [PostgreSQL](#postgresql)
- [MySQL](#mysql)
- [SQL Server](#sql-server)
- [Changing database later](#changing-database-later)

## Which one to use

| Database | When |
|---|---|
| SQLite | The default. Nothing to install, and the whole database is one file. Right for almost every community. |
| PostgreSQL, MySQL, SQL Server | You already run one of them, or the site is hosted where a file on disk is not reliable, such as Azure App Service. |

The same automated tests pass on all four. The versions used in those tests are PostgreSQL 16, MySQL 8.4 and SQL Server 2022.

## The two settings

| Setting | Values |
|---|---|
| `Database__Provider` | `Sqlite`, `PostgreSql`, `MySql` or `SqlServer` |
| `Database__ConnectionString` | How to reach the database. The form depends on the provider. |

Where to put them:

| Installation | Place |
|---|---|
| Linux installer | `/etc/l4d2-playstats-hub/hub.env`, then `playstats-hub restart` |
| Docker | Under `environment:` in the compose file |
| Azure App Service | Application settings |

The application creates the database if it does not exist, and creates or updates the tables each time it starts. The user in the connection string needs permission to do that.

## SQLite

```text
Database__Provider=Sqlite
Database__ConnectionString="Data Source=hub.db"
```

A file name without a folder is created inside the data folder (`Hub__DataDirectory`). To back it up, use `playstats-hub backup`, or stop the application and copy the file.

## PostgreSQL

```text
Database__Provider=PostgreSql
Database__ConnectionString="Host=localhost;Database=hub;Username=hub;Password=YOUR-PASSWORD"
```

## MySQL

```text
Database__Provider=MySql
Database__ConnectionString="Server=localhost;Database=hub;User=hub;Password=YOUR-PASSWORD"
```

## SQL Server

```text
Database__Provider=SqlServer
Database__ConnectionString="Server=localhost;Database=hub;User ID=hub;Password=YOUR-PASSWORD;TrustServerCertificate=True"
```

For Azure SQL, use the connection string the portal shows under **Connection strings → ADO.NET**. [Installing on Azure](install-azure.md#using-azure-sql) has the commands.

## Changing database later

There is no tool that copies the data from one database to another. When you change the provider, the application starts with an empty database and shows the setup page again.

The statistics files are the source of everything the rankings show, but the game server deletes each file after sending it, and the only copy is then inside the database. Keep a backup of the old database until you are sure you no longer need it. Purchases in the skin shop, community votes, punishments and settings are not in those files and would have to be entered again.
