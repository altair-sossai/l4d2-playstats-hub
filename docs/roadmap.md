# Roadmap

🌐 **English** | [Português](roadmap.pt.md) | [Español](roadmap.es.md)

The plan for bringing the L4D2 PlayStats system into this repository: what exists today, the target design, the decisions already taken, the phases, and what is still open.

## Contents

- [Where things stand today](#where-things-stand-today)
- [Target design](#target-design)
- [Decisions](#decisions)
- [Modules](#modules)
- [Game servers](#game-servers)
- [Seasons and rankings](#seasons-and-rankings)
- [Game modes](#game-modes)
- [Community ranking](#community-ranking)
- [Weapon skin shop](#weapon-skin-shop)
- [Setup wizard and server status](#setup-wizard-and-server-status)
- [Front end](#front-end)
- [Tests](#tests)
- [Deployment](#deployment)
- [Data migration](#data-migration)
- [Phases](#phases)
- [Open questions](#open-questions)

## Where things stand today

| Piece | Repository | How it works today |
|---|---|---|
| Statistics parser | [l4d2-playstats](https://github.com/altair-sossai/l4d2-playstats) | NuGet package that reads the files written by `l4d2_playstats`. |
| API | [l4d2-playstats-api](https://github.com/altair-sossai/l4d2-playstats-api) | Azure Functions. Stores the raw files in Azure Table Storage and rebuilds matches and ranking from them on request. |
| Website | [l4d2-playstats-web](https://github.com/altair-sossai/l4d2-playstats-web) | ASP.NET Core MVC. Reads from the API, keeps the skin shop in Azure Table Storage and the live match in memory. |
| Plugins | [l4d2-zone-server](https://github.com/altair-sossai/l4d2-zone-server) | Five SourceMod plugins talk to the API and to the website. |

Limits that this project removes:

- The season length is fixed at two months, in two separate places.
- Past seasons exist only as saved top-100 lists, so their matches cannot be browsed.
- The website assumes one game server.
- Only 4v4 matches with at least two maps count, and the accepted game modes are a hand-written pattern in a storage row.
- Maps outside a fixed list of campaigns are dropped.
- Installing it takes an Azure subscription, storage rows created by hand and secrets in three projects.

## Target design

One ASP.NET Core application that is the website, the API for the plugins, the admin area and the scheduler.

| Folder | Contents |
|---|---|
| `src/` | The application, the domain logic and the Entity Framework Core data layer, with one migrations project per database. |
| `plugins/` | SourceMod sources, includes, translations and configuration files. |
| `deploy/` | Install script, service definition, reverse proxy and Docker files. |
| `docs/` | Guides in English, Portuguese and Spanish. |

Main points:

- **The raw statistics file stays stored** as the source of truth. Matches, maps and per-player results become tables, so rankings are database queries, and a reprocess command can apply new rules to old files.
- **Settings live in the database** and are edited in the admin area. Only the database provider and connection string stay in a configuration file.
- **One back-end instance** serves every game server. Running several instances is out of scope for now.
- **The live match stays in memory**, never in the database. It is kept separately for each game server.

## Decisions

| Topic | Decision |
|---|---|
| Rankings | Two rankings. The statistics ranking is the main one and opens the site. The community ranking is secondary. |
| Scope of rankings | Both are global. A player's results on every game server add up. |
| Season length | Set in months by the administrator. A change applies from the next season. |
| Past seasons | Ranking, matches and players of any past season can be browsed. |
| Rule changes | Apply from then on. The administrator can reprocess manually. |
| Minimums | The minimum number of maps in a match and the minimum number of matches to appear in the ranking are both settings. |
| Database | Always relational, through Entity Framework Core. SQLite is the default. SQL Server, PostgreSQL and MySQL are options. |
| Back end | A single instance, whatever the number of game servers. |
| Live match | In memory only, separated by game server. |
| Skin refund | Refunding skins at the end of a season is a setting, on by default. |
| Skin catalog | The catalog and thumbnails live in the assets repository, and the application imports them. |
| Plugins | Target servers based on L4D2 Competitive Rework. |
| Remote commands | Sending commands from the site to a game server is included as a module. |
| Anti-cheat | Reserved as a module, turned off, with no implementation for now. |
| Docker | Offered as an alternative to the install script. |
| Old data | Imported and recalculated. There is no backward compatibility with the old storage. |
| Languages | English is the default. Portuguese and Spanish versions follow from it. |
| Tests | Extensive on the back end. Front-end tests only where they pay off. |

## Modules

The core is always on: game server registration, statistics upload, Steam sign-in and the admin area.

| Module | Contents |
|---|---|
| Ranking and statistics | Ranking, matches, players, patents, punishments and season history. |
| Live server | Scoreboard, players, events and chat of the match in progress. |
| Weapon skin shop | Stars, purchases, gifts and equipped skins. |
| Community ranking | Pairwise voting and its ranking. |
| Site chat | Chat between the site and the game, with bans. |
| Remote commands | Commands sent from the site to a game server, with an audit log. |
| Anti-cheat | Reserved. |

A module that is off disappears from the menu and its routes answer as not found. The skin shop depends on the statistics module, because stars come from match results.

## Game servers

- The administrator registers each game server and receives an identifier and a key for it. The key is stored hashed.
- The plugins use one address and one token of the form `id:key`. The token tells the back end which server sent each file, so the statistics file format does not change.
- Files are unique per server and file name.
- The live match, the site chat and the command queue are separated by server. The site lets the visitor pick which server to watch.

## Seasons and rankings

- Seasons are rows in the database, created from the configured length.
- A file dated before the first season creates the earlier seasons it needs, using the same length.
- The statistics ranking keeps the current scoring as default values that the administrator can edit: points for a win, a loss and a rage quit, and bonus points for MVPs.
- Punishments belong to a season.

## Game modes

The statistics file carries the display name of the configuration, such as `ZoneMod v2.9.2b`, and the team size. It does not carry the configuration folder name.

- Every configuration name received is recorded automatically.
- The administrator writes rules on top of those names: a name pattern that ignores the version, the team sizes, whether it counts toward the ranking, and the minimum number of maps.
- Campaigns become an editable table, and a map outside the table is no longer dropped.

## Community ranking

- The voter sees two players with their Steam avatars and answers which one is better.
- The answers are: left, right, same level, or "I don't know one of them".
- Same level counts as a draw in the calculation. "I don't know" changes nothing.
- Voting requires a minimum number of matches played, counted over all seasons. It is a setting, with 10 as the default.
- The number of votes per day is limited by a setting. Players cannot vote on themselves or repeat a pair.
- Candidates are players with recorded matches.
- The ranking does not reset with the season.

## Weapon skin shop

- Players earn stars from MVPs and spend them on skins.
- Purchases, gifts, prices and equipped skins move from Azure Table Storage to the database.
- The season refund follows the same seasons as the ranking, and the administrator can turn it off.
- Models, materials and photos stay in [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets).

## Setup wizard and server status

First-run wizard, shown once after the install:

1. The first administrator signs in with Steam, using a code printed by the installer.
2. Site name, language and Steam Web API key.
3. Modules to turn on.
4. Season length and initial game mode rules.
5. The first game server.

Game server wizard, used for the first server and for each one added later:

1. Name and address.
2. What has to be installed on the game server, with a download link for the plugins.
3. The exact configuration block to paste, and the file it goes in.
4. Validation. The page waits and turns to "connected" when the game server answers.

The game server calls the back end, not the other way around. That works behind NAT and needs no RCON password on the site. Each server shows one of these states:

| State | Meaning |
|---|---|
| Waiting | Never connected. |
| Online | A signal arrived in the last seconds. |
| Offline | Shows when it was last seen. |
| Invalid key | The identifier arrived with the wrong key. |

Each server also has a checklist, with a correction hint on every failed item: connection, plugin versions, statistics writing enabled, first statistics file received, live data arriving, and the skins plugin when that module is on. A console command on the game server runs the same check from that side.

## Front end

The browser embedded in the game is Chromium 18 from 2012. It runs ES5 only and has no flexbox, grid or custom properties. The measurements are in [in-game-browser.md](in-game-browser.md).

Proposed approach, still to be confirmed:

- The main site targets current browsers: server-rendered pages with a modern CSS framework and no Node build step.
- A small set of pages for the game lives under its own route, follows the limits of that browser, is served over plain HTTP, and is what the plugin opens.

## Tests

- **Back end, the priority.** Unit tests for match assembly, scoring, seasons, game mode rules, the community rating and the skin shop wallet. Integration tests against a real SQLite database, and against the other databases in continuous integration. Endpoint tests for everything the plugins call, so the contract with the game server cannot break unnoticed.
- **Plugins.** Every plugin is compiled in continuous integration.
- **Front end.** Only smoke tests that the pages render, plus a check that the in-game pages stay within the old browser's limits.

## Deployment

- **Linux VPS, the main path.** One command downloads an install script. It installs a self-contained build, creates the service, sets up HTTPS when a domain is given, and prints the setup address and the configuration block for the game server. Update and backup commands are included.
- **Azure.** A guide for App Service with a container. That service must run a single instance, and SQL Server is recommended there because its persistent disk is a network share.
- **Docker.** An image for those who prefer it.

## Data migration

A separate console program, outside this repository, moves the production data once:

- It reads the raw statistics files from the old storage and uploads each one through the same endpoint the plugin uses, so everything is recalculated by the normal path.
- Old punishments were deleted at each season change and do not come back, so recalculated history can differ slightly from the old lists.
- Skin gifts, current-season purchases, prices and chat bans are written to the new database.

## Phases

1. **Foundation.** Repository, continuous integration, database, settings and modules, Steam sign-in, first-run wizard, game server registration with the connection signal, and a minimal install script.
2. **Statistics and ranking.** Upload, match assembly, scoring, punishments, seasons and game mode rules.
3. **Website.** The existing pages on top of the new services, with season and server filters.
4. **Live server.** Live state per server, server picker, site chat, command queue and chat bans.
5. **Weapon skin shop.** Database storage, catalog import and season refund.
6. **Plugins.** The five plugins, one address and token, fixes to the upload plugin, compiled releases.
7. **Community ranking.** Voting page, rating and limits.
8. **Deployment and tutorials.** Full install script, Azure and Docker guides, step-by-step tutorials.
9. **Migration.** Import of the production data, switch of the production site, archiving of the old repositories.

## Open questions

- **License.** None chosen yet. The plugins compile against SourceMod includes, which are GPL-3.0.
- **Pages available in game.** Only the ranking, or also player details, last matches and the skin shop.
- **MySQL.** Depends on an Entity Framework Core provider being available for the .NET version in use.
- **HTTPS inside the game.** Not measured yet. See [in-game-browser.md](in-game-browser.md#not-measured-yet).
