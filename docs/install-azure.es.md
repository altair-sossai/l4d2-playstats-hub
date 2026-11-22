# Instalar en Azure

🌐 [English](install-azure.md) | [Português](install-azure.pt.md) | **Español**

Hay dos formas de ejecutar L4D2 PlayStats Hub en Azure. La primera es igual que en cualquier VPS y es la que recomendamos.

> **Estado de esta guía.** El camino de la máquina virtual usa el instalador de Linux, que está probado. El camino de App Service se escribió a partir de la documentación de Azure y todavía no se ha ejecutado de principio a fin, así que esta página no tiene capturas de pantalla de él. Si algún paso no coincide con lo que ves, por favor abre un issue.

## Contenido

- [Opción 1: una máquina virtual](#opción-1-una-máquina-virtual)
- [Opción 2: App Service con un contenedor](#opción-2-app-service-con-un-contenedor)
- [Usar Azure SQL](#usar-azure-sql)

## Opción 1: una máquina virtual

Crea una máquina virtual Linux y sigue [Instalar en un VPS Linux](install-linux.es.md). No cambia nada, y las capturas de pantalla de esa guía muestran lo que vas a ver.

| Ajuste | Valor sugerido |
|---|---|
| Imagen | Ubuntu Server 24.04 LTS |
| Tamaño | B1s (1 CPU, 1 GB) alcanza para empezar |
| Puertos de entrada | 22 para SSH y 5080 para el sitio, o 80 y 443 si usas un dominio |

En el portal, los puertos de entrada están en la página **Redes** de la máquina virtual. El sitio no se abre hasta que el puerto esté permitido allí.

## Opción 2: App Service con un contenedor

App Service ejecuta la imagen de Docker que publica cada versión. Necesitas la [CLI de Azure](https://learn.microsoft.com/cli/azure/install-azure-cli), con la sesión iniciada mediante `az login`.

Elige un nombre para el sitio. Debe ser único en Azure, porque la dirección pasa a ser `https://NAME.azurewebsites.net`. Los comandos de abajo usan `my-playstats`.

Crea un grupo de recursos y un plan Linux:

```bash
az group create --name playstats --location brazilsouth
```

```bash
az appservice plan create --resource-group playstats --name playstats-plan --is-linux --sku B1
```

Crea el sitio a partir de la imagen:

```bash
az webapp create --resource-group playstats --plan playstats-plan --name my-playstats --container-image-name ghcr.io/altair-sossai/l4d2-playstats-hub:latest
```

Indícale a App Service en qué puerto escucha la aplicación, y guarda los datos en el disco persistente:

```bash
az webapp config appsettings set --resource-group playstats --name my-playstats --settings WEBSITES_PORT=8080 WEBSITES_ENABLE_APP_SERVICE_STORAGE=true Hub__DataDirectory=/home/data
```

La clave de la API de Steam, el destino de la copia de seguridad y el código de configuración se guardan cifrados en la base de datos, con una clave de secretos guardada fuera de la carpeta de datos, para que el zip de copia de seguridad del sitio no los revele. En App Service la imagen guarda esa clave en `/home/secrets/secrets.key`, en el disco persistente, y la crea la primera vez que arranca. Para tener tú mismo la clave, lo que facilita guardar una copia, dala en su lugar como configuración de la aplicación antes del primer arranque, 32 bytes aleatorios en base64:

```bash
az webapp config appsettings set --resource-group playstats --name my-playstats --settings Hub__SecretsKey="$(openssl rand -base64 32)"
```

Guarda una copia de ese valor en un lugar seguro, o usa una referencia de Key Vault para él. Si el sitio ya creó `/home/secrets/secrets.key`, deja fuera esa configuración o usa el contenido de ese archivo como valor: una clave distinta no puede leer los secretos guardados antes. Una copia de seguridad restaurada sin la clave original deja vacíos la clave de la API de Steam y el destino de la copia de seguridad, y vuelves a escribirlos en el área de administración.

Mantén el sitio en una sola instancia. La partida en vivo se guarda en memoria, y dos instancias tendrían cada una la suya:

```bash
az appservice plan update --resource-group playstats --name playstats-plan --number-of-workers 1
```

Activa los WebSockets, que la [página del servidor en vivo](manual/live-server.es.md) usa para sus actualizaciones. Sin ellos la página sigue funcionando, por Server-Sent Events o long polling:

```bash
az webapp config set --resource-group playstats --name my-playstats --web-sockets-enabled true
```

Lee el código de configuración en el registro:

```bash
az webapp log config --resource-group playstats --name my-playstats --docker-container-logging filesystem
```

```bash
az webapp log tail --resource-group playstats --name my-playstats
```

Busca la línea que dice `Setup is pending`. Luego abre `https://my-playstats.azurewebsites.net`, pega el código, inicia sesión con Steam y sigue las páginas. [Configuración inicial](manual/first-run.es.md) muestra cada una de ellas.

Dos cosas que debes saber sobre App Service:

- **La base de datos.** El disco persistente de App Service es un recurso compartido de red, y SQLite no es confiable en recursos compartidos de red. Para cualquier cosa más allá de una prueba, usa Azure SQL como se describe más abajo.
- **El juego.** El comando `!ranking` dentro del juego necesita una dirección HTTP simple, porque no se espera que el navegador integrado en el juego abra los sitios HTTPS actuales. Permite HTTP simple en el sitio con el comando de abajo, y usa `http://my-playstats.azurewebsites.net` como dirección para el juego.

```bash
az webapp update --resource-group playstats --name my-playstats --https-only false
```

## Usar Azure SQL

Crea un servidor y una base de datos. El nivel Basic es suficiente.

```bash
az sql server create --resource-group playstats --name my-playstats-sql --admin-user hubadmin --admin-password "CHOOSE-A-STRONG-PASSWORD"
```

```bash
az sql db create --resource-group playstats --server my-playstats-sql --name hub --service-objective Basic
```

Permite que los servicios de Azure lleguen al servidor:

```bash
az sql server firewall-rule create --resource-group playstats --server my-playstats-sql --name azure --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```

Apunta el sitio a la base de datos:

```bash
az webapp config appsettings set --resource-group playstats --name my-playstats --settings Database__Provider=SqlServer Database__ConnectionString="Server=tcp:my-playstats-sql.database.windows.net,1433;Database=hub;User ID=hubadmin;Password=CHOOSE-A-STRONG-PASSWORD;Encrypt=True"
```

La aplicación crea sus tablas cuando se inicia. [Bases de datos](databases.es.md) tiene más información sobre cada base de datos compatible.
