# Contributing

How the code in this repository is organised and the rules every change follows.

## Contents

- [Projects](#projects)
- [Running it](#running-it)
- [Code rules](#code-rules)
- [Modules](#modules)
- [Database](#database)
- [Web pages](#web-pages)
- [Game server API](#game-server-api)
- [Tests](#tests)

## Projects

| Project | Contents |
|---|---|
| `src/L4D2PlayStats.Hub.Core` | Entities, the database context, and the services with the business rules. |
| `src/L4D2PlayStats.Hub.Migrations.Sqlite` | Entity Framework Core migrations for SQLite. |
| `src/L4D2PlayStats.Hub.Web` | The ASP.NET Core application: pages, the API used by the game server, the admin area and the background jobs. |
| `tests/L4D2PlayStats.Hub.Tests` | MSTest tests for the core and for the web application. |

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
- **Do not change the result of a call inline.** Write `var player = Find(id); player.Wins++;` instead of `Find(id).Wins++`.
- **English everywhere**: identifiers, log messages, interface texts.
- File-scoped namespaces, primary constructors and nullable reference types, as in the existing files.
- The `L4D2PlayStats` NuGet package shares the root namespace with this code. Its `Statistics` and `Player` types are in scope everywhere, so do not name a type or a namespace segment `Statistics` or `Player`. Use an alias such as `using ParsedStatistics = L4D2PlayStats.Statistics;` when you need the parsed file.

## Modules

Each feature lives in its own folder under `Modules/` in the core, with its entities, settings and services together.

- A class whose name ends in `Service` is registered automatically, as scoped, for the interfaces it implements. This holds for the core and for the web project.
- Anything else, such as a singleton that keeps state in memory, is registered by hand in `CoreInjection` or in `Program.cs`.
- Settings are plain classes saved through `ISettingsService`. Each class is one row, keyed by the class name, so a new settings class needs no schema change.
- A feature the administrator can turn off is a value of `HubModule`. Put `[RequireModule(HubModule.X)]` on its controllers; when the module is off they answer 404.
- Data that changes rankings or pages calls `HubState.DataChanged()`, which invalidates the cached rankings.
- Writes that must not interleave, such as a statistics upload, take `HubState.WriteLock`.

## Database

- The context is `HubDbContext`. Access a table with `dbContext.Set<T>()`, or add a `DbSet` property.
- Configure an entity with an `IEntityTypeConfiguration<T>` class next to it. The context picks up every configuration in the core assembly.
- Use only LINQ that every relational provider can translate. No raw SQL, no provider-specific functions.
- SQLite is the default database. Migrations are generated per provider:

```bash
dotnet ef migrations add <Name> --project src/L4D2PlayStats.Hub.Migrations.Sqlite --startup-project src/L4D2PlayStats.Hub.Migrations.Sqlite
```

## Web pages

- Pages are server-rendered Razor views with Bootstrap 5 and htmx. Both libraries are in `wwwroot/lib`; there is no Node build step.
- Every text shown to a visitor goes through the localizer: `@L["Text in English"]`. The English text is the key. Translations are in `Resources/pt-BR.json` and `Resources/es-ES.json`.
- A form that posts needs `asp-antiforgery="true"`, and its controller needs `[AutoValidateAntiforgeryToken]`. A checkbox bound to a boolean is followed by a hidden input with the same name and the value `false`.
- Admin pages use `[Authorize(Policy = AppConsts.AdministratorPolicy)]` and live under `/admin`.
- Pages under `/ingame` open inside the game, in a browser from 2012. They use no script, no flexbox, no grid and no custom properties. See [docs/in-game-browser.md](docs/in-game-browser.md).

## Game server API

- Endpoints called by the game server plugins live under `/api`.
- A call that writes carries the header `Authorization: <identifier>:<key>`. Put `[GameServerAuthorize]` on the action and read the caller with `HttpContext.GameServer()`.
- Identifiers that are SteamID64 values travel as strings in JSON, because they do not fit in a JavaScript number.

## Tests

- Back-end behaviour is covered by tests. A change without a test for what it changes is not finished.
- Core tests use `TestHub`, which gives a real SQLite database in memory and a clock the test controls.
- Web tests use `HubWebFactory`, which hosts the whole application. `Client(user)` signs in as that SteamID64, `ServerClient(credentials)` calls the API as a game server, and `PostFormAsync` posts a form with its antiforgery token.
- `StatisticsBuilder` creates statistics files for any map, date and score from the sample in `Samples/`.
- Test names say what is expected in plain words, such as `ASingleMapDoesNotCountForTheRanking`.
