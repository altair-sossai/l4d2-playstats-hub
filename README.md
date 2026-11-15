# L4D2 PlayStats Hub

🌐 **English** | [Português](README.pt.md) | [Español](README.es.md)

One self-hosted web application for competitive Left 4 Dead 2 servers: ranking, match statistics, live server view, weapon skin shop and a community ranking, together with the SourceMod plugins that feed it.

> **Status: early development.** Nothing in this repository can be installed yet. The existing repositories listed below still run the production site ([l4d2.com.br](https://l4d2.com.br/)) and remain the source of truth until the migration is finished.

## Contents

- [Why this project exists](#why-this-project-exists)
- [What it will include](#what-it-will-include)
- [What stays outside this repository](#what-stays-outside-this-repository)
- [Repositories being consolidated](#repositories-being-consolidated)
- [Roadmap](#roadmap)
- [Documentation](#documentation)
- [License](#license)

## Why this project exists

The system started small and grew into several repositories: a statistics parser, an API, a website with a skin shop, and a set of game server plugins. Running it today takes Azure resources, hand-made storage rows and secrets spread over three projects.

This repository brings the logic together so that any server owner can run the same ranking and statistics system with one application, one database file and one install command.

The migration happens step by step. Each feature moves here only when it is ready, and the old repositories are archived at the end.

## What it will include

- **One web application** (ASP.NET Core) that is the website, the API used by the game server plugins, the admin area and the scheduled jobs.
- **Relational database through Entity Framework Core.** SQLite is the default, so a new install needs no database server. SQL Server, PostgreSQL and MySQL are planned as options.
- **Modules the administrator turns on or off:** ranking and statistics, live server view, weapon skin shop, community ranking, site chat.
- **Several game servers on one back end.** Each server has its own key, and every player's statistics add up across all servers.
- **Configurable seasons.** The administrator sets how many months a season lasts, and past seasons stay browsable: ranking, matches and players.
- **Configurable game modes.** The administrator chooses which modes count toward the ranking and the minimum size of a competitive match.
- **Community ranking.** Players vote on pairs of players, and the votes build a second ranking next to the statistics one.
- **Setup wizard and server status.** A first-run guide walks through the configuration, and each registered game server shows whether it is connected and correctly set up.
- **SourceMod plugins** that upload statistics, send the live match, show the ranking in game and apply weapon skins.
- **Simple deployment.** One install script for a Linux VPS, with a guide for Azure and a Docker image as alternatives.
- **Automated tests**, with the back end as the priority.

## What stays outside this repository

| Repository | Why it stays separate |
|---|---|
| [l4d2-playstats](https://github.com/altair-sossai/l4d2-playstats) | NuGet package that parses the statistics files. It is useful on its own and this project uses it as a dependency. |
| [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets) | Weapon skin models, materials and photos. They are over 500 MB, so only the skin shop logic lives here. |

## Repositories being consolidated

| Repository | What it is today | What happens to it |
|---|---|---|
| [l4d2-playstats-api](https://github.com/altair-sossai/l4d2-playstats-api) | Azure Functions API that stores the statistics files and computes matches, ranking and stars. | Replaced by this application, then archived. |
| [l4d2-playstats-web](https://github.com/altair-sossai/l4d2-playstats-web) | Website with ranking, statistics, live server page and weapon skin shop. | Replaced by this application, then archived. |
| [l4d2-zone-server](https://github.com/altair-sossai/l4d2-zone-server) | Competitive server setup with custom plugins. | The plugins below move here. |

Plugins that move from `l4d2-zone-server`:

| Plugin | What it does |
|---|---|
| `l4d2_playstats_sync` | Uploads the statistics files written by `l4d2_playstats`. |
| `l4d2_gameinfo_sync` | Sends the live match: scoreboard, players, events and chat. |
| `l4d2_ranking` | Opens the ranking inside the game. |
| `l4d2_show_patent_icon` | Shows each player's patent icon in game. |
| `l4d2_weapon_skins` | Applies the weapon skins players own. |

`l4d2_playstats` itself, which writes the statistics files, comes from [L4D2 Competitive Rework](https://github.com/SirPlease/L4D2-Competitive-Rework) and is not copied here.

## Roadmap

1. Foundation: application skeleton, database, settings and modules, Steam sign-in, setup wizard, game server registration.
2. Statistics upload, matches, seasons and ranking.
3. Website pages, with season and server filters.
4. Live server view for several servers.
5. Weapon skin shop.
6. Game server plugins.
7. Community ranking.
8. Install script, deployment guides and tutorials.
9. Data migration and archiving of the old repositories.

The full plan, with every decision taken so far, is in [docs/roadmap.md](docs/roadmap.md).

## Documentation

| Document | Contents |
|---|---|
| [docs/roadmap.md](docs/roadmap.md) | Architecture, decisions, phases and open questions. |
| [docs/in-game-browser.md](docs/in-game-browser.md) | What the browser embedded in Left 4 Dead 2 supports, and what that means for the front end. |

English is the default language of this repository. Every document also has a Portuguese (`.pt.md`) and a Spanish (`.es.md`) version.

## License

The application is released under the [MIT License](LICENSE). Anyone can use, change and share it, including commercially, as long as the copyright notice is kept.

The SourceMod plugins, once they arrive in `plugins/`, are released under GPL-3.0, because SourceMod requires that of plugins built with its includes.
