# Roadmap

🌐 **English** | [Português](roadmap.pt.md) | [Español](roadmap.es.md)

The plan for bringing the L4D2 PlayStats system into this repository: where it came from, the design, the decisions taken, what is done, and what is still open.

## Contents

- [Where it came from](#where-it-came-from)
- [Design](#design)
- [Decisions](#decisions)
- [Modules](#modules)
- [Game servers](#game-servers)
- [Seasons and rankings](#seasons-and-rankings)
- [Game modes](#game-modes)
- [Community ranking](#community-ranking)
- [Weapon skin shop](#weapon-skin-shop)
- [Live server](#live-server)
- [Setup wizard and server status](#setup-wizard-and-server-status)
- [Front end](#front-end)
- [Tests](#tests)
- [Deployment](#deployment)
- [Data migration](#data-migration)
- [Progress](#progress)
- [Open questions](#open-questions)

## Where it came from

| Piece | Repository | How it worked |
|---|---|---|
| Statistics parser | [l4d2-playstats](https://github.com/altair-sossai/l4d2-playstats) | NuGet package that reads the files written by `l4d2_playstats`. |
| API | [l4d2-playstats-api](https://github.com/altair-sossai/l4d2-playstats-api) | Azure Functions. Stored the raw files in Azure Table Storage and rebuilt matches and ranking from them on request. |
| Website | [l4d2-playstats-web](https://github.com/altair-sossai/l4d2-playstats-web) | ASP.NET Core MVC. Read from the API, kept the skin shop in Azure Table Storage and the live match in memory. |
| Plugins | [l4d2-zone-server](https://github.com/altair-sossai/l4d2-zone-server) | Five SourceMod plugins talked to the API and to the website, with different addresses and secrets. |

Limits of that setup, which this project removes:

- The season length was fixed at two months, in two separate places.
- Past seasons existed only as saved top-100 lists, so their matches could not be browsed.
- The website assumed one game server.
- Only 4v4 matches with at least two maps counted, and the accepted game modes were a hand-written pattern in a storage row.
- Maps outside a fixed list of campaigns were dropped.
- Installing it took an Azure subscription, storage rows created by hand and secrets in three projects.

## Design

One ASP.NET Core application that is the website, the API for the plugins, the admin area and the scheduler.

- **The raw statistics file stays stored** as the source of truth. Matches, maps and per-player results are tables built from it, and a reprocess command rebuilds them when rules change.
- **Settings live in the database** and are edited in the admin area. Only the database provider, the connection string and the data folder are set outside it.
- **One back-end instance** serves every game server. Running several instances is out of scope.
- **The live match stays in memory**, never in the database, and is kept separately for each game server.
- **Each feature is a module** in the code and, where it makes sense, something the administrator can turn off.

[CONTRIBUTING.md](../CONTRIBUTING.md) describes the folders and the code rules.

## Decisions

| Topic | Decision |
|---|---|
| Rankings | Two rankings. The statistics ranking is the main one and opens the site. The community ranking is secondary. |
| Scope of rankings | Both are global. A player's results on every game server add up. |
| Season length | Set in months by the administrator. A change applies from the next season. |
| Season boundaries | Follow the clock of the game server that sent each file, as the old system did. |
| Past seasons | Ranking, matches and players of any past season can be browsed, as well as a whole year and all time. |
| Rule changes | Apply from then on. The administrator can reprocess every match. |
| Minimums | The minimum number of maps in a match and the minimum number of matches to appear in the ranking are both settings. |
| Unknown maps | A match on maps outside the campaign list is kept. A setting turns that off. |
| Database | Always relational, through Entity Framework Core. SQLite is the default. SQL Server, PostgreSQL and MySQL are options. |
| Entity Framework Core version | 10, the long-term support version. The PostgreSQL and MySQL providers are not available for version 11 yet. |
| Back end | A single instance, whatever the number of game servers. |
| Live match | In memory only, separated by game server. |
| Skin refund | Refunding skins at the end of a season is a setting, on by default. |
| Skin catalog | The catalog and the pictures live in the assets repository, and the application imports the catalog. |
| Plugins | Target servers based on L4D2 Competitive Rework. One address and one token for all of them. |
| Remote commands | Sending commands from the site to a game server is a module, off by default, with an audit log. |
| Anti-cheat | A module, off by default, that shows the Anti-Cheat page with the download and the instructions. The Anti-Cheat program and its service are separate projects. |
| Front end | Two layers: the main site for current browsers, and a few pages made for the browser inside the game. |
| In-game pages | Only the essentials: the season ranking, the summary of the player who opened the panel, and the latest matches. |
| Admin area | Server-rendered like the rest of the site. A single-page framework such as Angular is allowed if a screen needs it, and is not used. |
| Docker | Offered as an alternative to the install script. The container runs the site as an unprivileged user and has a health check. |
| Old data | Imported and recalculated. There is no backward compatibility with the old storage. |
| Languages | English is the default. Portuguese and Spanish versions follow from it. |
| Code layout | One type per file, grouped by module and then by kind. |
| Tests | Extensive on the back end. Front-end tests only where they pay off. |
| License | MIT for the application. The plugins are GPL-3.0, as SourceMod requires. |

## Modules

The core is always on: game server registration, statistics upload, Steam sign-in and the admin area.

| Module | Contents | Default |
|---|---|---|
| Ranking and statistics | Ranking, matches, players, patents, punishments and season history. | On |
| Live server | Scoreboard, players and events of the match in progress. | On |
| Site chat | Chat between the site and the game, with bans. Needs the live server. | Off |
| Remote commands | Commands sent from the site to a game server. Needs the live server. | Off |
| Weapon skin shop | Stars, purchases, gifts and equipped skins. Needs the ranking. | Off |
| Community ranking | Pairwise voting and its ranking. | Off |
| Anti-Cheat | The page with the Anti-Cheat download and instructions. | Off |

A module that is off disappears from the menu and its pages answer as not found.

## Game servers

- The administrator registers each game server and receives an identifier and a key. Only a hash of the key is stored.
- The plugins use one address and one token of the form `identifier:key`. The token tells the site which server sent each file, so the statistics file format did not change.
- Files are unique per server and file name.
- The live match, the site chat and the command queue are separated by server. The visitor picks which server to watch.

## Seasons and rankings

- Seasons are rows in the database, created from the configured length. A two-month season starts in January, March, May and so on.
- A file dated before the first season creates the earlier seasons it needs.
- The scoring keeps the old values as defaults the administrator can edit: 85 for a win, -80 for a loss, -150 for a rage quit, 8 and 6 for each MVP. Experience never goes below zero.
- Punishments belong to a season.
- Matches never cross a season boundary, a change of configuration or a change of game server.

## Game modes

The statistics file carries the display name of the configuration, such as `ZoneMod v2.9.2b`, and the team size. It does not carry the configuration folder name.

- Every configuration name received is recorded automatically and listed in the admin area.
- The administrator writes rules on top of those names: a pattern where `*` stands for any text, the team size, whether it counts toward the ranking, and the minimum number of maps.
- The first rule that fits a match decides. A match that fits no rule does not count.
- Campaigns are an editable table, used to tell which maps belong to the same match.

## Community ranking

- The voter sees two players with their avatars and answers which one is better: left, right, same level, or "I don't know" one or both of them.
- Same level counts as a draw. "I don't know" changes nothing and hides that player from that voter afterwards.
- The rating is Elo, and it does not reset with the season.
- Voting needs a minimum number of matches played, counted over all seasons: 10 by default.
- Votes per day are limited: 20 by default. Players cannot vote on themselves or on the same pair twice.
- The site chooses the pair, preferring players the voter has played with or against, and accepts an answer only for the pair it offered.

## Weapon skin shop

- Players earn one star for each MVP in ranked matches, over their whole history, and spend stars on skins.
- The catalog is imported from a file in the assets repository. The pictures are loaded from an address the administrator sets.
- At the start of a season, skins bought earlier are refunded and gifts are kept. The administrator can turn the refund off.
- Models, materials and photos stay in [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets).

## Live server

- The game server sends the configuration, the round, the scoreboard, the players, the chat and the events of the match.
- The site shows them a few seconds late, as the old site did.
- Connections by IP address are stored to point out accounts that share an address. The address itself is never shown.

## Setup wizard and server status

The first-run wizard runs once, after the install: the first administrator signs in with Steam using a code printed by the installer, then chooses the site settings, the modules and the first game server.

For each game server, the site shows the two lines to paste into its configuration and then waits for the server to answer. The game server calls the site, not the other way around, which works behind NAT and needs no RCON password.

| State | Meaning |
|---|---|
| Waiting | Never connected. |
| Connected | A signal arrived in the last 90 seconds. |
| Offline | Shows when it was last seen. |
| Invalid key | The identifier arrived with the wrong key in the last 10 minutes. |

A checklist shows what was found: connection, upload plugin, statistics writing, first file received, live data and the skins plugin. The `sm_playstats_test` command runs the same check from the game server console.

## Front end

The browser inside the game is Chromium 18 from 2012. It runs ES5 only and has no flexbox, grid or custom properties. The measurements are in [in-game-browser.md](in-game-browser.md).

- **Main site.** Server-rendered pages that keep the look and the content of the site this project replaces: Bootstrap 3 with its dark and light themes, jQuery, Chart.js and diffDOM, with no Node build step. Every page of the old site was ported item by item; the checklists are in [site-parity](site-parity/README.md). The pages that are new here (wizard, administration, community ranking) follow the same look.
- **In-game pages.** Three read-only pages under `/ingame`, with no script, that fit the game's panel. A test fails if they start using something that browser lacks.

## Tests

- **Back end.** Unit tests for match assembly, scoring, seasons, game mode rules, the Elo rating and the skin shop. Tests through HTTP for every page and for every endpoint the plugins call.
- **Parity.** A test compares the rankings with a copy of the production data. It matches the old API exactly for the seasons computed with the current rules.
- **Databases.** The same tests run on SQLite, SQL Server, PostgreSQL and MySQL, and check that the migrations of each one match the model.
- **Plugins.** Every plugin is compiled on each change.

## Deployment

- **Linux VPS.** One command installs a self-contained build as a service, with optional HTTPS through Caddy, and prints the setup code. Update, backup and restore are single commands.
- **Docker.** An image published with every release.
- **Azure.** A virtual machine with the Linux installer, or App Service with the image.

## Data migration

A separate console program, outside this repository, moves the production data once:

- It reads the raw statistics files from the old storage, without changing it, and uploads each one through the same endpoint the plugin uses, so everything is recalculated by the normal path.
- Old punishments were deleted at each season change and do not come back.
- Rankings saved before August 2026 were computed with an older rule for the minimum number of maps, so recalculated history differs from those lists.

## Progress

| Phase | Contents | Status |
|---|---|---|
| 1. Foundation | Database, settings and modules, Steam sign-in, wizard, game server registration and status. | Done |
| 2. Statistics and ranking | Upload, match assembly, scoring, punishments, seasons and game mode rules. | Done |
| 3. Website | Ranking, history, matches, players, patents, Anti-Cheat page, admin area and the in-game pages, in three languages. | Done; every screen of the old site ported and checked item by item |
| 4. Live server | Live state per server, server picker, site chat, remote commands and chat bans. | Done, not yet seen with a real game server |
| 5. Weapon skin shop | Shop, gifts, refunds, catalog import and season refund. | Done, not yet tried with the real assets |
| 6. Plugins | The five plugins with one address and token, heartbeat and connection test. | Compiled, not yet run on a game server |
| 7. Community ranking | Voting page, rating, limits and admin page. | Done |
| 8. Deployment | Linux installer, Docker image, release workflow and guides. | Done; published since v0.1.1, installer and image tried from the published version |
| 9. Migration | Import of the production data, switch of the production site, archiving of the old repositories. | Import tried locally; the switch is pending |

## Open questions

- **Plugins on a live server.** They compile, and the site accepts what they send, but they have not been run in a real match.
- **HTTPS inside the game.** Not measured. See [in-game-browser.md](in-game-browser.md#not-measured-yet).
- **Assets repository.** The shop expects thumbnails at `skins/{id}.jpg` and photos at `skin-shots/{id}/`, under one address. The assets repository does not have the thumbnails yet.
- **Patent icons in game.** The sprites used by `l4d2_show_patent_icon` are not in this repository.
- **Azure App Service.** The guide was written from the Azure documentation and not run end to end.
