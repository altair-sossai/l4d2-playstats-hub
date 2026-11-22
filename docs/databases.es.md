# Bases de datos

🌐 [English](databases.md) | [Português](databases.pt.md) | **Español**

L4D2 PlayStats Hub guarda sus datos en una base de datos relacional. Tú eliges cuál con dos ajustes.

## Contenido

- [Cuál usar](#cuál-usar)
- [Los dos ajustes](#los-dos-ajustes)
- [SQLite](#sqlite)
- [PostgreSQL](#postgresql)
- [MySQL](#mysql)
- [SQL Server](#sql-server)
- [Cambiar de base de datos más adelante](#cambiar-de-base-de-datos-más-adelante)

## Cuál usar

| Base de datos | Cuándo |
|---|---|
| SQLite | La opción predeterminada. No hay nada que instalar, y toda la base de datos es un solo archivo. Adecuada para casi todas las comunidades. |
| PostgreSQL, MySQL, SQL Server | Ya usas una de ellas, o el sitio está alojado donde un archivo en disco no es confiable, como Azure App Service. |

Las mismas pruebas automatizadas pasan en las cuatro. Las versiones usadas en esas pruebas son PostgreSQL 16, MySQL 8.4 y SQL Server 2022.

## Los dos ajustes

| Ajuste | Valores |
|---|---|
| `Database__Provider` | `Sqlite`, `PostgreSql`, `MySql` o `SqlServer` |
| `Database__ConnectionString` | Cómo llegar a la base de datos. El formato depende del proveedor. |

Dónde ponerlos:

| Instalación | Lugar |
|---|---|
| Instalador para Linux | `/etc/l4d2-playstats-hub/hub.env`, y luego `playstats-hub restart` |
| Docker | Dentro de `environment:` en el archivo compose |
| Azure App Service | Configuración de la aplicación |

La aplicación crea la base de datos si no existe, y crea o actualiza las tablas cada vez que se inicia. El usuario de la cadena de conexión necesita permiso para hacerlo.

## SQLite

```text
Database__Provider=Sqlite
Database__ConnectionString="Data Source=hub.db"
```

Un nombre de archivo sin carpeta se crea dentro de la carpeta de datos (`Hub__DataDirectory`). Para hacer una copia de seguridad, usa `playstats-hub backup`, o detén la aplicación y copia el archivo.

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

Para Azure SQL, usa la cadena de conexión que el portal muestra en **Connection strings → ADO.NET**. [Instalación en Azure](install-azure.es.md#usar-azure-sql) tiene los comandos.

## Cambiar de base de datos más adelante

No hay ninguna herramienta que copie los datos de una base de datos a otra. Cuando cambias el proveedor, la aplicación arranca con una base de datos vacía y vuelve a mostrar la página de configuración.

Los archivos de estadísticas son el origen de todo lo que muestran los rankings, pero el servidor de juego borra cada archivo después de enviarlo, y la única copia queda entonces dentro de la base de datos. Conserva una copia de seguridad de la base de datos anterior hasta que estés seguro de que ya no la necesitas. Las compras en la tienda de skins, los votos de la comunidad, las sanciones y los ajustes no están en esos archivos y habría que volver a ingresarlos.
