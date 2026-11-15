# Game server plugins

🌐 **English** | [Português](README.pt.md) | [Español](README.es.md)

The SourceMod plugins that connect a Left 4 Dead 2 server to L4D2 PlayStats Hub. They use one address and one token for everything.

## Contents

- [The plugins](#the-plugins)
- [Requirements](#requirements)
- [Installing](#installing)
- [Settings](#settings)
- [Commands](#commands)
- [Checking the connection](#checking-the-connection)
- [Loading them in a Confogl or ZoneMod match mode](#loading-them-in-a-confogl-or-zonemod-match-mode)
- [Moving from the old setup](#moving-from-the-old-setup)
- [What the plugins call](#what-the-plugins-call)
- [Building](#building)
- [License](#license)

## The plugins

| Plugin | What it does | Module of the site |
|---|---|---|
| `l4d2_playstats_sync` | Tells the site every 30 seconds that the server is alive, and uploads the statistics files written by `l4d2_playstats`. It also has the command that checks the connection. | Always on |
| `l4d2_gameinfo_sync` | Sends the live match: configuration, round, scoreboard, players, events and chat. It also brings the site chat into the game and runs the commands sent from the site. | Live server, Site chat, Remote commands |
| `l4d2_ranking` | Opens the in-game ranking page with `!ranking`, and by itself for players who join before a match starts. | Ranking and statistics |
| `l4d2_show_patent_icon` | Shows each player's patent icon above the head before a match starts. | Ranking and statistics |
| `l4d2_weapon_skins` | Applies the weapon skins players bought in the shop. | Weapon skin shop |

`l4d2_playstats_sync` is the one every server needs. The others are optional, and each one works without the others.

## Requirements

- A Left 4 Dead 2 dedicated server with [SourceMod](https://www.sourcemod.net) 1.12 or newer.
- The [REST in Pawn](https://github.com/ErikMinekus/sm-ripext) extension. All five plugins need it.
- [Left 4 DHooks Direct](https://github.com/SilvDev/Left4DHooks) (`left4dhooks`). `l4d2_gameinfo_sync`, `l4d2_ranking` and `l4d2_show_patent_icon` need it.
- The `l4d2_playstats` plugin from [L4D2 Competitive Rework](https://github.com/SirPlease/L4D2-Competitive-Rework), with `sm_stats_writestats 1`. It writes the statistics files that `l4d2_playstats_sync` uploads.
- A game server registered on the site. The site gives it an identifier and a key.

The plugins target servers based on L4D2 Competitive Rework. `l4d2_gameinfo_sync` uses `readyup`, `pause`, `l4d_boss_percent`, `l4d2_hybrid_scoremod` and `l4d2_skill_detect` when they are loaded, and still runs without them.

Two plugins need files that players download from the server (FastDL, `sv_downloadurl`). The files are in [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets):

- `l4d2_show_patent_icon` needs the sprites `materials/sprites/patent_<level>_v<version>.vmt` and `.vtf`.
- `l4d2_weapon_skins` needs the models and materials listed in `configs/weapon_skins.cfg`.

## Installing

1. Get the compiled plugins (`.smx`) from the [releases](https://github.com/altair-sossai/l4d2-playstats-hub/releases), or [build them](#building).
2. Copy the files to the game server, inside the `left4dead2` folder:

   | From | To |
   |---|---|
   | `l4d2_playstats_sync.smx` | `addons/sourcemod/plugins/` |
   | The other `.smx` files | `addons/sourcemod/plugins/`, or `addons/sourcemod/plugins/optional/` when a match mode loads them |
   | `translations/` | `addons/sourcemod/translations/`, keeping the language folders |
   | `configs/weapon_skins.cfg` | `addons/sourcemod/configs/` |
   | `cfg/playstats.cfg.example` | `cfg/playstats.cfg` |

3. Open `cfg/playstats.cfg` and fill in `playstats_url` and `playstats_token` with the values the site shows on the page of the game server.
4. Add `exec playstats.cfg` to `cfg/server.cfg`. You can also paste the two lines straight into `cfg/server.cfg`.
5. Restart the game server or change the map.
6. Run `sm_playstats_test` in the server console.

Keep `l4d2_playstats_sync.smx` in `addons/sourcemod/plugins/` itself. That way it is loaded when the server starts, the two cvars exist when `server.cfg` runs, and the site sees the server even outside a match.

## Settings

Shared by all plugins:

| Cvar | Default | Meaning |
|---|---|---|
| `playstats_url` | empty | Address of the site, without a slash at the end. Example: `https://stats.example.com`. |
| `playstats_token` | empty | Token of this game server, in the form `<identifier>:<key>`. It is protected, so the game never shows its value to players. |

`l4d2_ranking`:

| Cvar | Default | Meaning |
|---|---|---|
| `playstats_ingame_url` | empty | Address the game opens for the in-game pages. When empty, `playstats_url` is used. |
| `ranking_show_motd_panel` | `1` | `0` stops the plugin from opening the page. |

The page it opens is `<address>/ingame?steamid=<SteamID64 of the player>`.

The browser inside the game is Chromium 18, from 2012, and it may not open a modern HTTPS site. If the panel stays blank, serve the site over plain HTTP on another address or port and put that address in `playstats_ingame_url`. Keep the address short. See [docs/in-game-browser.md](../docs/in-game-browser.md).

`l4d2_show_patent_icon`:

| Cvar | Default | Meaning |
|---|---|---|
| `patent_icon_version` | `1` | Version in the name of the sprite files. |
| `patent_icon_max_level` | `15` | Highest patent level that has a sprite. |

`l4d2_gameinfo_sync`:

| Cvar | Default | Meaning |
|---|---|---|
| `gameinfo_related_accounts_chat` | `1` | Announces in chat when a player connects from the same IP address as another account seen before. |
| `gameinfo_boomer_vomit_min_survivors` | `3` | Fewest survivors a boomer has to hit for the event to be sent. |

`l4d2_gameinfo_sync` sends the IP address of each player who connects to the site, which uses it to find accounts that share an address.

`l4d2_weapon_skins`:

| Cvar | Default | Meaning |
|---|---|---|
| `weapon_skins_enabled` | `0` | Turns the skins on. |
| `weapon_skins_kick_no_downloads` | `0` | Kicks players whose game refuses to download the server files. |
| `weapon_skins_pile_radius` | `120.0` | Radius used to find primary weapons dropped close to each other. |
| `weapon_skins_pile_max` | `3` | Most dropped primary weapons kept inside the radius. `0` turns it off. |
| `weapon_skins_preview_all` | `0` | Test mode: every skin is downloaded on the next map, and root admins can use any of them. |

## Commands

| Command | Who | What it does |
|---|---|---|
| `sm_playstats_test` | Server console, root admins | Calls the site right away and prints the result. |
| `sm_ranking` (`!ranking`) | Players | Opens the in-game ranking page. |
| `sm_skin`, `sm_skins` (`!skin`) | Players | Chooses a weapon skin. Type `!skin` in game for the options. |

## Checking the connection

Run `sm_playstats_test` in the server console. It first shows the result of the last automatic call, then calls the site and prints one of these:

| Result | Meaning |
|---|---|
| `Connected to the site "..." as the game server "..."` | Everything is right. The next lines list the modules turned on, the plugins loaded, whether statistics writing is on, and how many statistics files are waiting. |
| `URL not set` | `playstats_url` is empty. |
| `Token not set` | `playstats_token` is empty. |
| `Cannot reach ...` | The address did not answer. Check the address, the network and whether the site is running. |
| `Unknown identifier` | The site has no game server with the identifier in the token. |
| `Wrong key` | The identifier exists, but the key does not match. Generate a new key on the site. |
| `Token not accepted` | The token is not in the form `<identifier>:<key>`. |
| `This game server is disabled on the site` | An administrator turned the server off on the site. |
| `... answered HTTP ...` | The address answers, but it is not L4D2 PlayStats Hub. Check `playstats_url`. |

Over RCON only the first lines come back, because the answer of the site arrives after the command has returned. Use the server console, or run the command in game as a root admin.

The same problems are written once to the SourceMod error log (`addons/sourcemod/logs/errors_*.log`) when they start.

On the site, open the administration area, then the game servers, then the server. Its status shows Waiting, Online, Offline or Invalid key, and a checklist: connection, statistics upload plugin loaded, statistics writing enabled, first statistics file received, live data arriving, and the skins plugin when the shop is on.

## Loading them in a Confogl or ZoneMod match mode

When a match mode starts, Confogl unloads every plugin and loads only the ones listed in the files named in `confogl_match_execcfg_plugins`. So each plugin you want during a match needs a line there.

1. In the plugin list of the match mode, for example `cfg/cfgogl/zonemod/confogl_plugins.cfg`, add these lines before `sm plugins load confoglcompmod.smx`:

   ```
   sm plugins load l4d2_playstats_sync.smx
   sm plugins load optional/l4d2_gameinfo_sync.smx
   sm plugins load optional/l4d2_ranking.smx
   sm plugins load optional/l4d2_show_patent_icon.smx
   sm plugins load optional/l4d2_weapon_skins.smx
   ```

   Leave out the ones you do not use. `l4d2_playstats` itself must be in the list too: `sm plugins load optional/l4d2_playstats.smx`.

2. In the cvars of the match mode, for example `cfg/cfgogl/zonemod/zonemod.cfg`, turn statistics writing on, and set the other plugin cvars you want:

   ```
   confogl_addcvar sm_stats_writestats 1
   confogl_addcvar ranking_show_motd_panel 1
   confogl_addcvar weapon_skins_enabled 1
   ```

3. Keep `playstats_url` and `playstats_token` in `cfg/server.cfg` or `cfg/playstats.cfg`, not in the match mode. Their values stay while plugins are unloaded and loaded again. Do not use `confogl_addcvar` for them, because Confogl puts a cvar it tracks back to its old value when the match mode ends.

## Moving from the old setup

The plugins used to talk to a separate API and website. If the server has that setup:

- Remove `playstats_access_token`, `l4d2_web_url`, `l4d2_web_secret` and `ranking_url` from every `.cfg` file. These cvars no longer exist.
- Point `playstats_url` to the new site and set `playstats_token`.
- If the ranking panel used a plain HTTP address in `ranking_url`, put that kind of address in `playstats_ingame_url`.
- Replace all five `.smx` files and the translation files.

## What the plugins call

Every request carries the header `Authorization: <playstats_token>`. SteamID64 values travel as strings.

| Plugin | Request |
|---|---|
| `l4d2_playstats_sync` | `POST /api/servers/heartbeat`, `POST /api/statistics` |
| `l4d2_gameinfo_sync` | `PUT /api/game-info/configuration`, `PUT /api/game-info/round`, `PUT /api/game-info/scoreboard`, `PUT /api/game-info/players`, `PUT /api/game-info/messages`, `POST /api/game-info/events`, `POST /api/game-info/player-connection-info`, `GET /api/game-info/server-command/dequeue`, `GET /api/external-chat` |
| `l4d2_ranking` | None. It opens `/ingame?steamid=...` in the player's game. |
| `l4d2_show_patent_icon` | `GET /api/players/patents` |
| `l4d2_weapon_skins` | `GET /api/skins`, `GET /api/players/skins`, `GET /api/players/{id}/wallet`, `POST /api/players/{id}/skins/equip` |

The heartbeat is sent when the configuration files have run, then every 30 seconds while the server is not hibernating, and right after `playstats_url` or `playstats_token` changes.

Statistics files are sent one at a time, ten seconds after each round start. A file the site answered "not ready" for is sent again only after it changes, or after 30 minutes. A file is deleted from the game server only when the site says so.

The shared code is in `scripting/include/l4d2_playstats_hub.inc`: the two cvars, the request with the token, and the heartbeat.

## Building

You need the SourceMod 1.12 compiler, `spcomp`, which comes in the SourceMod package under `addons/sourcemod/scripting`. The third-party include files are already in `scripting/include`.

Windows:

```powershell
./build.ps1
```

Linux and macOS:

```bash
./build.sh
```

Both compile every `.sp` file in `scripting/` into `compiled/`, which Git ignores.

The scripts look for the compiler in the `PATH`, and `build.ps1` also looks in `C:\Program Files\SourceMod`. To point to it, set `SPCOMP`, or pass `-Spcomp` to `build.ps1`:

```powershell
./build.ps1 -Spcomp "C:\sourcemod\addons\sourcemod\scripting\spcomp.exe"
```

```bash
SPCOMP=~/sourcemod/addons/sourcemod/scripting/spcomp64 ./build.sh
```

The standard SourceMod include files are taken from the `include` folder next to the compiler. Set `SOURCEMOD_INCLUDE` if they are somewhere else.

Every push and pull request that touches `plugins/` compiles the plugins on GitHub and keeps the `.smx` files as an artifact of the run.

Code rules: no comments, constants as `#define` at the top of the file, and third-party include files stay exactly as they came.

## License

The plugins are released under the [GNU General Public License, version 3](LICENSE), as SourceMod requires. [NOTICE.md](NOTICE.md) lists the third-party include files and where they come from.
