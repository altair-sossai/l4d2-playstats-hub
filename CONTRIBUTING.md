# Contributing

🌐 **English** | [Português](CONTRIBUTING.pt.md) | [Español](CONTRIBUTING.es.md)

How the code in this repository is organised and the rules every change follows.

## Contents

- [Projects](#projects)
- [Running it](#running-it)
- [Code rules](#code-rules)
- [Where each file goes](#where-each-file-goes)
- [Modules](#modules)
- [Database](#database)
- [Web pages](#web-pages)
- [Game server API](#game-server-api)
- [Tests](#tests)

## Projects

| Folder | Contents |
|---|---|
| `src/L4D2PlayStats.Hub.Core` | Entities, the database context, and the services with the business rules. |
| `src/L4D2PlayStats.Hub.Web` | The ASP.NET Core application: pages, the API used by the game server, the admin area and the background jobs. |
| `src/L4D2PlayStats.Hub.Migrations.*` | Entity Framework Core migrations, one project per database: `Sqlite`, `SqlServer`, `PostgreSql`, `MySql`. |
| `tests/L4D2PlayStats.Hub.Tests` | MSTest tests for the core and for the web application. |
| `plugins/` | The SourceMod plugins. They have their own [guide](plugins/README.md) and license. |
| `deploy/` | The Linux installer and the Docker Compose file. |
| `docs/` | Guides, in English, Portuguese and Spanish. |

## Running it

```bash
dotnet test
```

```bash
dotnet run --project src/L4D2PlayStats.Hub.Web --launch-profile http
```

The site opens on `http://localhost:5080`. On the first run it prints a setup code in the log. In the Development environment the setup page can sign you in without Steam, with any SteamID64.

## Code rules

- **No comments in code.** Names and small methods carry the meaning.
- **Constants live in `AppConsts`**, at the root of the project that uses them. No magic numbers or repeated strings in the code.
- **One type per file.** Every class, record, interface and enum has its own file, named after it.
- **Do not change the result of a call inline.** Write `var player = Find(id); player.Wins++;` instead of `Find(id).Wins++`.
- **English everywhere**: identifiers, log messages, interface texts.
- File-scoped namespaces, primary constructors and nullable reference types, as in the existing files. Namespaces follow the folders.
- The `L4D2PlayStats` NuGet package shares the root namespace with this code. Its `Statistics` and `Player` types are in scope everywhere, so do not name a type or a namespace segment `Statistics` or `Player`. Use an alias such as `using ParsedStatistics = L4D2PlayStats.Statistics;` when you need the parsed file.

## Where each file goes

Each feature is a module: a folder under `Modules/` in the core. Inside a module, files are grouped in subfolders by kind.

| Subfolder | What goes in it |
|---|---|
| The module root | The entities stored in the database. |
| `Configurations/` | `IEntityTypeConfiguration<T>` classes. |
| `Enums/` | Enums. |
| `Exceptions/` | Exceptions. |
| `Extensions/` | Static classes with extension methods. |
| `Helpers/` | Static classes with pure logic, such as calculators. |
| `Models/` | Records and classes that are not stored: results, filters, values kept in memory. |
| `Services/` | Service interfaces and their implementations, each in its own file. |
| `Settings/` | Settings classes saved through `ISettingsService`. |

The web project follows the same idea: one controller per file under `Controllers/`, view models under `Models/`, and the cross-cutting pieces under `Infrastructure/`, grouped by kind.

## Modules

- A class whose name ends in `Service` is registered automatically, as scoped, for the interfaces it implements. This holds for the core and for the web project.
- Anything else, such as a singleton that keeps state in memory, is registered by hand in `CoreInjection` or in `Program.cs`.
- Settings are plain classes saved through `ISettingsService`. Each class is one row, keyed by the class name, so a new settings class needs no schema change.
- A feature the administrator can turn off is a value of `HubModule`. Put `[RequireModule(HubModule.X)]` on its controllers; when the module is off they answer 404.
- Data that changes the statistics rankings calls `HubState.DataChanged()`, which invalidates the cached rankings.
- Writes that must not interleave, such as a statistics upload or a purchase, take `HubState.WriteLock`.
- State of the match in progress stays in memory, one per game server. It is never written to the database.

## Database

- The context is `HubDbContext`. Access a table with `dbContext.Set<T>()`, or add a `DbSet` property.
- Configure an entity with an `IEntityTypeConfiguration<T>` class. The context picks up every configuration in the core assembly.
- Use only LINQ that every relational provider can translate. No raw SQL, no provider-specific functions.
- When the model changes, add a migration to **each** of the four projects. Replace `Sqlite` with `SqlServer`, `PostgreSql` and `MySql`:

```bash
dotnet ef migrations add <Name> --project src/L4D2PlayStats.Hub.Migrations.Sqlite --startup-project src/L4D2PlayStats.Hub.Migrations.Sqlite
```

A test fails when the model and the migrations of the database in use do not match.

## Web pages

- Pages are server-rendered Razor views that keep the look of the site this project replaces: Bootstrap 3 with its dark and light themes, jQuery, Chart.js, diffDOM and lity, all in `wwwroot`. A few administration pages also load htmx from `wwwroot/lib`, and the live server page loads the SignalR client from there. There is no Node build step. A page that existed in the old site is a port of it: read [docs/site-parity](docs/site-parity/README.md) before changing one.
- Styles and scripts live in static files that the browser caches: `wwwroot/css/admin.css`, `administration.css` and `pagination.css` for the shared admin and list styles (loaded by the `_AdminStyles`, `_AdministrationStyles` and `_PaginationStyles` partials), `wwwroot/css/pages` for the styles of each page and partial, and `wwwroot/js/pages` for the script of a large page. Only rules that depend on the theme stay in a small inline `<style>`, and the pages under `/ingame` keep their styles inline, since the game's browser should not load extra files. No view or partial has inline script, except the Google Analytics snippet of the layout: each one has its file in `wwwroot/js/pages`, which reads its translated texts and server values into `var <name>Config` from a `<script type="application/json" id="<name>-config">` block that the view writes just before it, with readable keys such as `refreshInterval` (a test checks that every key a script reads exists in its view). Every local style and script is referenced with `asp-append-version`, which adds a hash of the file to its address: a versioned file is cached for a year, an unversioned style or script is checked again on every load, so a deploy never leaves a browser with an old file. `wwwroot/js/hub.js`, loaded by the layout, has the shared helpers: `Hub.escapeHtml`, `Hub.format`, `Hub.alert`, `Hub.errorText`. A form or button with `data-confirm="…"` asks before it goes on, and `PlayerPicker.open` and `PlayerPicker.swap` (from `_PlayerPickerModal`) choose a player, the second one from inside another window.
- Every text shown to a visitor goes through the localizer: `@L["Text in English"]`. The English text is the key. Translations are in `Resources/pt-BR.json` and `Resources/es-ES.json`. When one English word needs two translations, use a key with a context prefix, such as `Rarity.Common`, and give its English text in `Resources/en-US.json`.
- A form that posts needs `asp-antiforgery="true"`, and its controller needs `[AutoValidateAntiforgeryToken]`. A checkbox bound to a boolean is followed by a hidden input with the same name and the value `false`.
- Admin pages use `[Authorize(Policy = AppConsts.AdministratorPolicy)]` and live under `/admin`.
- Pages under `/ingame` open inside the game, in a browser from 2012. They use no script, no flexbox, no grid and no custom properties. See [docs/in-game-browser.md](docs/in-game-browser.md).

## Game server API

- Endpoints called by the game server plugins live under `/api`.
- Every call from a plugin carries the header `Authorization: <identifier>:<key>`. Put `[GameServerAuthorize]` on the action and read the caller with `HttpContext.GameServer()`.
- Identifiers that are SteamID64 values travel as strings in JSON, because they do not fit in a JavaScript number.
- The plugins are released separately from the site, so an endpoint they call keeps its path and its JSON shape. A test covers each one.

## Tests

- Back-end behaviour is covered by tests. A change without a test for what it changes is not finished.
- Core tests use `TestHub`, which gives a real database and a clock the test controls.
- Web tests use `HubWebFactory`, which hosts the whole application. `Client(user)` signs in as that SteamID64, `ServerClient(credentials)` calls the API as a game server, and `PostFormAsync` posts a form with its antiforgery token.
- `StatisticsBuilder` creates statistics files for any map, date and score from the sample in `Samples/`.
- Test names say what is expected in plain words, such as `ASingleMapDoesNotCountForTheRanking`.

By default the tests use SQLite in memory. To run them on another database, set two environment variables. `{database}` is replaced by a new name for each test, and the database is dropped at the end:

```bash
export HUB_TEST_DATABASE_PROVIDER=PostgreSql
export HUB_TEST_DATABASE_CONNECTION="Host=localhost;Database={database};Username=postgres;Password=YOUR-PASSWORD"
dotnet test
```

The tests in the `Parity` category compare the rankings with a copy of real data and are skipped unless `HUB_PARITY_DUMP` points to that copy.

The [browser tests](tools/e2e/README.md) check the main pages in a headless Chrome or Edge, on a throwaway site with generated data. Run them after changing a view, a style or a script.
