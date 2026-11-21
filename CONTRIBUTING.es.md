# Contribuir

🌐 [English](CONTRIBUTING.md) | [Português](CONTRIBUTING.pt.md) | **Español**

Cómo está organizado el código de este repositorio y las reglas que sigue cada cambio.

## Contenido

- [Proyectos](#proyectos)
- [Cómo ejecutarlo](#cómo-ejecutarlo)
- [Reglas de código](#reglas-de-código)
- [Dónde va cada archivo](#dónde-va-cada-archivo)
- [Módulos](#módulos)
- [Base de datos](#base-de-datos)
- [Páginas web](#páginas-web)
- [API del servidor de juego](#api-del-servidor-de-juego)
- [Pruebas](#pruebas)

## Proyectos

| Carpeta | Contenido |
|---|---|
| `src/L4D2PlayStats.Hub.Core` | Las entidades, el contexto de la base de datos y los servicios con las reglas de negocio. |
| `src/L4D2PlayStats.Hub.Web` | La aplicación ASP.NET Core: las páginas, la API que usa el servidor de juego, el área de administración y las tareas en segundo plano. |
| `src/L4D2PlayStats.Hub.Migrations.*` | Migraciones de Entity Framework Core, un proyecto por base de datos: `Sqlite`, `SqlServer`, `PostgreSql`, `MySql`. |
| `tests/L4D2PlayStats.Hub.Tests` | Pruebas con MSTest para el núcleo y para la aplicación web. |
| `plugins/` | Los plugins de SourceMod. Tienen su propia [guía](plugins/README.es.md) y su propia licencia. |
| `deploy/` | El instalador para Linux y el archivo de Docker Compose. |
| `docs/` | Guías, en inglés, portugués y español. |

## Cómo ejecutarlo

```bash
dotnet test
```

```bash
dotnet run --project src/L4D2PlayStats.Hub.Web --launch-profile http
```

El sitio abre en `http://localhost:5080`. En la primera ejecución muestra un código de configuración en el log. En el entorno Development, la página de configuración permite iniciar sesión sin Steam, con cualquier SteamID64.

## Reglas de código

- **Sin comentarios en el código.** Los nombres y los métodos pequeños transmiten el significado.
- **Las constantes viven en `AppConsts`**, en la raíz del proyecto que las usa. Nada de números mágicos ni de cadenas repetidas en el código.
- **Un tipo por archivo.** Cada clase, record, interfaz y enum tiene su propio archivo, con su mismo nombre.
- **No modifiques el resultado de una llamada en la misma línea.** Escribe `var player = Find(id); player.Wins++;` en lugar de `Find(id).Wins++`.
- **Inglés en todas partes**: identificadores, mensajes de log, textos de la interfaz.
- Namespaces con alcance de archivo, constructores primarios y tipos de referencia anulables, como en los archivos existentes. Los namespaces siguen las carpetas.
- El paquete NuGet `L4D2PlayStats` comparte el namespace raíz con este código. Sus tipos `Statistics` y `Player` están en alcance en todas partes, así que no nombres un tipo ni un segmento de namespace `Statistics` o `Player`. Usa un alias como `using ParsedStatistics = L4D2PlayStats.Statistics;` cuando necesites el archivo analizado.

## Dónde va cada archivo

Cada funcionalidad es un módulo: una carpeta dentro de `Modules/` en el núcleo. Dentro de un módulo, los archivos se agrupan en subcarpetas por tipo.

| Subcarpeta | Qué va en ella |
|---|---|
| La raíz del módulo | Las entidades que se guardan en la base de datos. |
| `Configurations/` | Clases `IEntityTypeConfiguration<T>`. |
| `Enums/` | Enums. |
| `Exceptions/` | Excepciones. |
| `Extensions/` | Clases estáticas con métodos de extensión. |
| `Helpers/` | Clases estáticas con lógica pura, como las calculadoras. |
| `Models/` | Records y clases que no se guardan: resultados, filtros, valores que se mantienen en memoria. |
| `Services/` | Interfaces de servicio y sus implementaciones, cada una en su propio archivo. |
| `Settings/` | Clases de ajustes que se guardan mediante `ISettingsService`. |

El proyecto web sigue la misma idea: un controlador por archivo dentro de `Controllers/`, los view models dentro de `Models/` y las piezas transversales dentro de `Infrastructure/`, agrupadas por tipo.

## Módulos

- Una clase cuyo nombre termina en `Service` se registra automáticamente, como scoped, para las interfaces que implementa. Esto vale para el núcleo y para el proyecto web.
- Todo lo demás, como un singleton que mantiene estado en memoria, se registra a mano en `CoreInjection` o en `Program.cs`.
- Los ajustes son clases simples que se guardan mediante `ISettingsService`. Cada clase es una fila, cuya clave es el nombre de la clase, así que una clase de ajustes nueva no necesita ningún cambio de esquema.
- Una funcionalidad que el administrador puede desactivar es un valor de `HubModule`. Pon `[RequireModule(HubModule.X)]` en sus controladores; cuando el módulo está desactivado, responden 404.
- Los datos que cambian los rankings de estadísticas llaman a `HubState.DataChanged()`, que invalida los rankings en caché.
- Las escrituras que no deben intercalarse, como una subida de estadísticas o una compra, toman `HubState.WriteLock`.
- El estado de la partida en curso se queda en memoria, uno por servidor de juego. Nunca se escribe en la base de datos.

## Base de datos

- El contexto es `HubDbContext`. Accede a una tabla con `dbContext.Set<T>()`, o agrega una propiedad `DbSet`.
- Configura una entidad con una clase `IEntityTypeConfiguration<T>`. El contexto toma todas las configuraciones del ensamblado del núcleo.
- Usa solo LINQ que todos los proveedores relacionales puedan traducir. Nada de SQL directo ni de funciones específicas de un proveedor.
- Cuando el modelo cambia, agrega una migración a **cada uno** de los cuatro proyectos. Reemplaza `Sqlite` por `SqlServer`, `PostgreSql` y `MySql`:

```bash
dotnet ef migrations add <Name> --project src/L4D2PlayStats.Hub.Migrations.Sqlite --startup-project src/L4D2PlayStats.Hub.Migrations.Sqlite
```

Una prueba falla cuando el modelo y las migraciones de la base de datos en uso no coinciden.

## Páginas web

- Las páginas son vistas Razor renderizadas en el servidor que mantienen el aspecto del sitio que este proyecto reemplaza: Bootstrap 3 con sus temas oscuro y claro, jQuery, Chart.js, diffDOM y lity, todos en `wwwroot`. Algunas páginas de administración también cargan htmx desde `wwwroot/lib`, y la página del servidor en vivo carga desde ahí el cliente de SignalR. No hay ningún paso de compilación con Node. Una página que existía en el sitio antiguo es un porte de ella: lee [docs/site-parity](docs/site-parity/README.es.md) antes de cambiar una.
- Los estilos y los scripts están en archivos estáticos que el navegador guarda en caché: `wwwroot/css/admin.css`, `administration.css` y `pagination.css` para los estilos compartidos de la administración y de las listas (cargados por las partials `_AdminStyles`, `_AdministrationStyles` y `_PaginationStyles`), y `wwwroot/css/pages` y `wwwroot/js/pages` para una página grande. El script de una página lee sus textos traducidos y los valores del servidor de un pequeño objeto `HubPage` que la vista escribe justo antes. `wwwroot/js/hub.js`, cargado por el layout, tiene los helpers compartidos: `Hub.escapeHtml`, `Hub.format`, `Hub.alert`, `Hub.errorText`. Un formulario o botón con `data-confirm="…"` pregunta antes de seguir, y `PlayerPicker.open` y `PlayerPicker.swap` (de `_PlayerPickerModal`) eligen un jugador, el segundo desde dentro de otra ventana.
- Todo texto que se muestra a un visitante pasa por el localizador: `@L["Text in English"]`. El texto en inglés es la clave. Las traducciones están en `Resources/pt-BR.json` y `Resources/es-ES.json`. Cuando una palabra en inglés necesita dos traducciones, usa una clave con un prefijo de contexto, como `Rarity.Common`, y pon su texto en inglés en `Resources/en-US.json`.
- Un formulario que hace POST necesita `asp-antiforgery="true"`, y su controlador necesita `[AutoValidateAntiforgeryToken]`. Un checkbox enlazado a un booleano va seguido de un input oculto con el mismo nombre y el valor `false`.
- Las páginas de administración usan `[Authorize(Policy = AppConsts.AdministratorPolicy)]` y viven bajo `/admin`.
- Las páginas bajo `/ingame` se abren dentro del juego, en un navegador de 2012. No usan scripts, flexbox, grid ni propiedades personalizadas. Consulta [docs/in-game-browser.es.md](docs/in-game-browser.es.md).

## API del servidor de juego

- Los endpoints que llaman los plugins del servidor de juego viven bajo `/api`.
- Cada llamada de un plugin lleva el encabezado `Authorization: <identifier>:<key>`. Pon `[GameServerAuthorize]` en la acción y lee quién llama con `HttpContext.GameServer()`.
- Los identificadores que son valores SteamID64 viajan como cadenas en el JSON, porque no caben en un número de JavaScript.
- Los plugins se publican por separado del sitio, así que un endpoint que ellos llaman conserva su ruta y la forma de su JSON. Una prueba cubre cada uno.

## Pruebas

- El comportamiento del back-end está cubierto por pruebas. Un cambio sin una prueba para lo que cambia no está terminado.
- Las pruebas del núcleo usan `TestHub`, que ofrece una base de datos real y un reloj que la prueba controla.
- Las pruebas web usan `HubWebFactory`, que aloja la aplicación completa. `Client(user)` inicia sesión como ese SteamID64, `ServerClient(credentials)` llama a la API como un servidor de juego, y `PostFormAsync` envía un formulario con su token antiforgery.
- `StatisticsBuilder` crea archivos de estadísticas para cualquier mapa, fecha y puntuación a partir del ejemplo que está en `Samples/`.
- Los nombres de las pruebas dicen qué se espera en palabras simples, como `ASingleMapDoesNotCountForTheRanking`.

De forma predeterminada, las pruebas usan SQLite en memoria. Para ejecutarlas en otra base de datos, define dos variables de entorno. `{database}` se reemplaza por un nombre nuevo en cada prueba, y la base de datos se elimina al final:

```bash
export HUB_TEST_DATABASE_PROVIDER=PostgreSql
export HUB_TEST_DATABASE_CONNECTION="Host=localhost;Database={database};Username=postgres;Password=YOUR-PASSWORD"
dotnet test
```

Las pruebas de la categoría `Parity` comparan los rankings con una copia de datos reales y se omiten a menos que `HUB_PARITY_DUMP` apunte a esa copia.

Las [pruebas en el navegador](tools/e2e/README.es.md) verifican las páginas principales en un Chrome o Edge headless, en un sitio desechable con datos generados. Ejecútelas después de cambiar una vista, un estilo o un script.
