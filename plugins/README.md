# Game server plugins

🌐 **English** | [Português](README.pt.md) | [Español](README.es.md)

The five SourceMod plugins that connect a Left 4 Dead 2 server to L4D2 PlayStats Hub. To install them, copy everything inside the `left4dead2` folder of this directory (or of the release zip) into the `left4dead2` folder of your game server, then set two values in `cfg/playstats.cfg`. For a guided walk-through, from registering the server on the site to the first match, see [Connecting a game server](../docs/game-server.md).

## Contents

- [What is in this folder](#what-is-in-this-folder)
- [The plugins](#the-plugins)
- [Requirements](#requirements)
- [Installing](#installing)
- [Settings](#settings)
- [L4D2 Competitive Rework match modes](#l4d2-competitive-rework-match-modes)
- [Updating](#updating)
- [Weapon skin and patent files](#weapon-skin-and-patent-files)
- [Commands](#commands)
- [Checking the installation](#checking-the-installation)
- [Troubleshooting](#troubleshooting)
- [What the plugins call](#what-the-plugins-call)
- [Building](#building)
- [Not yet tested on a game server](#not-yet-tested-on-a-game-server)
- [License](#license)

## What is in this folder

The `left4dead2` folder has the same layout as the game folder of a server, so each file lands in its place when you copy it:

```text
plugins/
├── left4dead2/                      copy what is inside onto the server's left4dead2 folder
│   ├── addons/sourcemod/
│   │   ├── plugins/                 l4d2_gameinfo_sync and l4d2_weapon_skins (.smx)
│   │   │   └── optional/            the other three (.smx), loaded by the match modes
│   │   ├── scripting/               their sources (.sp) and the include files to compile them
│   │   ├── translations/            chat texts: English, pt, es, chi and zho
│   │   └── configs/weapon_skins.cfg the weapons and skins the skins plugin knows
│   └── cfg/playstats.cfg            your settings: the only file you edit
├── build.ps1, build.sh              compile the plugins into left4dead2/addons/sourcemod/plugins and its optional folder
├── README.md, LICENSE, NOTICE.md    this guide and the license (not copied to the server)
```

The game folder of the server is the one with `gameinfo.txt`, `addons/` and `cfg/` inside, for example `/home/steam/l4d2/left4dead2/` or `C:\l4d2\left4dead2\`.

The `.smx` files in the repository are always the result of compiling the sources next to them: every push checks it (see [Building](#building)).

## The plugins

| Plugin | What it does | Module of the site | Needs |
|---|---|---|---|
| `l4d2_playstats_sync` | Tells the site every 30 seconds that the server is alive and uploads the statistics files written by `l4d2_playstats`. It has the `sm_playstats_test` command. Every server needs it. | Always on | REST in Pawn |
| `l4d2_gameinfo_sync` | Sends the live match: configuration, round, scoreboard, players, events and chat. Brings the site chat into the game and runs the commands sent from the site. | Live server, Site chat, Remote commands | REST in Pawn, left4dhooks |
| `l4d2_ranking` | Opens the in-game ranking page with `!ranking`, and by itself for players who join before a match starts. | Ranking | REST in Pawn, left4dhooks |
| `l4d2_show_patent_icon` | Shows each player's patent icon above the head before a match starts. | Ranking | REST in Pawn, left4dhooks, the patent sprites |
| `l4d2_weapon_skins` | Applies the weapon skins players bought in the shop. | Weapon skin shop | REST in Pawn, the skin models and materials |

Only `l4d2_playstats_sync` is required. Each of the others works without the rest; delete the `.smx` of a plugin you do not want before you copy the folder, or after.

## Requirements

| What | Needed by | Comes with L4D2 Competitive Rework | Where to get it |
|---|---|---|---|
| Left 4 Dead 2 dedicated server | All | No | SteamCMD, app 222860 |
| Metamod:Source and SourceMod 1.12 | All | Yes | <https://www.sourcemod.net/downloads.php?branch=stable> |
| REST in Pawn extension 1.3 (`rip.ext`) | All five | **No, you add it** | <https://github.com/ErikMinekus/sm-ripext/releases> |
| Left 4 DHooks Direct (`left4dhooks`) | `l4d2_gameinfo_sync`, `l4d2_ranking`, `l4d2_show_patent_icon` | Yes | <https://github.com/SilvDev/Left4DHooks/releases> |
| `l4d2_playstats` | The statistics files that `l4d2_playstats_sync` uploads | Yes, in `plugins/optional/` | [L4D2 Competitive Rework](https://github.com/SirPlease/L4D2-Competitive-Rework) |
| `readyup`, `pause`, `l4d_boss_percent`, `l4d2_hybrid_scoremod` or `l4d2_hybrid_scoremod_zone`, `l4d2_skill_detect` | Optional: extra live data in `l4d2_gameinfo_sync` (ready-up, pauses, boss percents, bonus, skeets and other events) | Yes, in `plugins/optional/` | [L4D2 Competitive Rework](https://github.com/SirPlease/L4D2-Competitive-Rework) |
| `l4d2_mix`, `l4d2_anti_cheat` | Optional: the "mix started" event and the anti-cheat status in `l4d2_gameinfo_sync` | No | [l4d2-zone-server](https://github.com/altair-sossai/l4d2-zone-server) |
| Patent sprites, skin models and materials, and a FastDL | `l4d2_show_patent_icon`, `l4d2_weapon_skins` | No | [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets), see [Weapon skin and patent files](#weapon-skin-and-patent-files) |
| A game server registered on the site | All | No | The site: **Administration → Game servers** |

The plugins are compiled with SourceMod 1.12.0.7260 and need SourceMod 1.12 or newer. The optional plugins are found by name while the server runs: when one of them is not loaded, `l4d2_gameinfo_sync` still works and only leaves out that part. The include files in `scripting/include` are needed only to compile; `l4d2util` and `colors` are compiled into the plugins and need nothing on the server.

To install REST in Pawn, download `sm-ripext-<version>-linux.zip` or `-windows.zip` and extract it into the `left4dead2` folder of the server. It adds `addons/sourcemod/extensions/rip.ext.so` (or `.dll`) and `addons/sourcemod/configs/ripext/ca-bundle.crt`, the list of certificate authorities it trusts for HTTPS. SourceMod loads it by itself when a plugin needs it.

## Installing

1. Install what the [requirements](#requirements) list. On a server with L4D2 Competitive Rework, that is only REST in Pawn.
2. Copy everything inside `left4dead2/` into the `left4dead2` folder of the server. Nothing in that folder replaces a file of SourceMod or of L4D2 Competitive Rework, except include files in `addons/sourcemod/scripting/include` (see [Updating](#updating)).
3. On the site, open **Administration → Game servers**, create the server and copy the two lines the site shows (`playstats_url` and `playstats_token`).
4. Open `cfg/playstats.cfg` on the server and paste the two lines where the file says.
5. Add this line at the end of `cfg/server.cfg`: `exec playstats.cfg`.
6. On a server with L4D2 Competitive Rework, add the plugins to `cfg/sharedplugins.cfg` (see [L4D2 Competitive Rework match modes](#l4d2-competitive-rework-match-modes)).
7. Restart the game server.
8. Run `sm_playstats_test` in the server console (see [Checking the installation](#checking-the-installation)).

Copy the folder as it is: `l4d2_gameinfo_sync` and `l4d2_weapon_skins` go in `addons/sourcemod/plugins/`, which SourceMod loads when the server starts, and the other three in `addons/sourcemod/plugins/optional/`, which only a match mode loads (see [L4D2 Competitive Rework match modes](#l4d2-competitive-rework-match-modes)). That is where L4D2 Competitive Rework servers keep them, so a server that ran the earlier versions keeps loading them with the lines it already has. While no match mode is loaded, `l4d2_playstats_sync` does not run and the site shows the server as offline; to keep it connected, add `sm plugins load optional/l4d2_playstats_sync.smx` at the end of `cfg/server.cfg`.

## Settings

Every setting of the five plugins is in `cfg/playstats.cfg`, with an explanation above each line. Only the first part of that file has to change; the rest is turned off with `//` and shows the default value.

You must set:

| Cvar | Plugin | Default | Values | What to do |
|---|---|---|---|---|
| `playstats_url` | All five | empty | Address of the site with `https://` or `http://`, without a slash at the end | Paste the line the site shows |
| `playstats_token` | All five | empty | `<identifier>:<key>`, as the site shows it | Paste the line the site shows. Keep it secret |
| `sm_stats_writestats` | `l4d2_playstats` (Competitive Rework) | `0` | `0` off, `1` write the statistics files, `2` also write readable tables | Keep the `1` that `cfg/playstats.cfg` sets, unless a match mode already sets it |

You may change (each line is off in the file, and the plugin uses the default):

| Cvar | Plugin | Default | Values | Meaning |
|---|---|---|---|---|
| `playstats_public_url` | `l4d2_ranking`, `l4d2_weapon_skins` | empty | An address, without a slash at the end | Address of the site that players read in chat: the ranking hint and the skin shop link. Empty means `playstats_url`. Set it when the plugins reach the site by an IP address but players know it by a name, for example `https://stats.example.com`. |
| `playstats_ingame_url` | `l4d2_ranking` | empty | An address, without a slash at the end | Address the in-game browser opens. Empty means `playstats_url`. Set a plain `http://` address when the panel stays blank (see below). |
| `ranking_show_motd_panel` | `l4d2_ranking` | `1` | `0` or `1` | `0` stops the plugin from opening the ranking page, even with `!ranking`. |
| `patent_icon_enabled` | `l4d2_show_patent_icon` | `0` | `0` or `1` | Turns the patent icons on. With `0` the plugin does not ask the site for the patents and players do not download the sprites. Turn it on only after the sprites are on the FastDL. |
| `patent_icon_version` | `l4d2_show_patent_icon` | `2` | `1` or more | Version in the names of the sprite files, `patent_<level>_v<version>`, both with two digits. The files of the assets repository are version `2`. |
| `patent_icon_max_level` | `l4d2_show_patent_icon` | `15` | `1` or more | Highest patent level that has a sprite file. |
| `gameinfo_related_accounts_chat` | `l4d2_gameinfo_sync` | `1` | `0` or `1` | Says in chat when a player connects from the same IP address as another account seen before. `0` prints it only in the console. The site has its own setting for who sees these accounts on the player page ([Settings > Privacy](../docs/manual/administration.md#privacy)). |
| `gameinfo_boomer_vomit_min_survivors` | `l4d2_gameinfo_sync` | `3` | `1` or more | Fewest survivors a boomer must vomit on for the event to reach the site. |
| `weapon_skins_enabled` | `l4d2_weapon_skins` | `0` | `0` or `1` | Turns the skins on. Turn it on only after the skin files are on the server and on the FastDL. |
| `weapon_skins_kick_no_downloads` | `l4d2_weapon_skins` | `0` | `0` or `1` | Kicks players whose game refuses to download the server files (`cl_allowdownload 0`, or `cl_downloadfilter` other than `all` or `nosounds`). With `0` they play, and while one of them is on the server the skins are off in third person for everyone. |
| `weapon_skins_pile_radius` | `l4d2_weapon_skins` | `120.0` | `0` or more | Radius, in game units, used to find primary weapons dropped close to each other. |
| `weapon_skins_pile_max` | `l4d2_weapon_skins` | `3` | `0` or more | Most dropped primary weapons kept inside that radius; the oldest ones are removed. `0` turns it off. |
| `weapon_skins_preview_all` | `l4d2_weapon_skins` | `0` | `0` or `1` | Test mode: every skin is downloaded from the next map on, and root admins can use any of them without buying it and outside the ready-up. Keep `0` on a public server. |

Where to set them:

- In `cfg/playstats.cfg`, run from `cfg/server.cfg` with `exec playstats.cfg`. The game runs `server.cfg` every time a map loads, so the values are applied again after a match mode reloads the plugins. This is the place for every setting that is the same in all match modes.
- With `confogl_addcvar` in the configuration of one match mode, for example `cfg/cfgogl/zonemod/zonemod.cfg`, only for a setting that must differ between match modes. Then delete that line from `cfg/playstats.cfg`: a setting in both places is changed back and forth on every map, and Confogl prints each change in the chat.
- Never put `playstats_url` or `playstats_token` in a match mode or under `confogl_addcvar`: Confogl prints the old and the new value of the settings it tracks in the chat, which would show the token to the players.

The token cvar is protected: the game never sends its value to players, and SourceMod's `sm_cvar` shows it only to admins with the password flag. The plugins never print it.

The page `l4d2_ranking` opens is `<address>/ingame?steamid=<SteamID64 of the player>`. The browser inside the game is Chromium 18, from 2012, and is not expected to open current HTTPS sites. If the panel stays blank, serve the site over plain HTTP on another address or port and put that address in `playstats_ingame_url`. See [docs/in-game-browser.md](../docs/in-game-browser.md).

`l4d2_gameinfo_sync` sends the IP address of each player who connects to the site, which uses it to find accounts that share an address. It also runs, as server commands, the commands that site administrators send from the **Remote commands** page: whoever controls an administrator account of the site can run any server command, like with RCON. It also runs the `sm_aac sync` the site queues after a change on the Anti-Cheat settings, even with the remote commands module off.

## L4D2 Competitive Rework match modes

When a match mode loads, Confogl's plugin manager unloads every plugin and loads only the ones listed in the files named by `confogl_match_execcfg_plugins`. In L4D2 Competitive Rework that cvar is set in `cfg/server.cfg` to `generalfixes.cfg;confogl_plugins.cfg;sharedplugins.cfg`, and `cfg/sharedplugins.cfg` is the file meant for your own plugins, loaded by every match mode.

Add these lines to `cfg/sharedplugins.cfg`, leaving out the plugins you do not use:

```text
sm plugins load optional/l4d2_playstats_sync.smx
sm plugins load l4d2_gameinfo_sync.smx
sm plugins load optional/l4d2_ranking.smx
sm plugins load optional/l4d2_show_patent_icon.smx
sm plugins load l4d2_weapon_skins.smx
```

Without those lines the three plugins in `optional/` never load, and the two in `addons/sourcemod/plugins/` stop when a match mode loads.

The match modes of L4D2 Competitive Rework already load `optional/l4d2_playstats.smx` (in `confogl_plugins.cfg`) and the optional plugins `l4d2_gameinfo_sync` uses. The ZoneMod modes do not set `sm_stats_writestats`, which is why `cfg/playstats.cfg` sets it to `1`. If your server's modes set it with `confogl_addcvar`, delete that line from `cfg/playstats.cfg`.

On the site, **Administration → Game modes** decides which match modes count toward the ranking. Statistics are sent from every mode.

## Updating

Copy the new `left4dead2` folder over the old one again, except `cfg/playstats.cfg`, which holds your token. Then restart the server while it is empty, so every plugin loads in its new version.

| File | Safe to replace on an update |
|---|---|
| `addons/sourcemod/plugins/*.smx` and `addons/sourcemod/plugins/optional/*.smx` | Yes. |
| `addons/sourcemod/translations/**` | Yes, unless you edited the texts. |
| `addons/sourcemod/configs/weapon_skins.cfg` | Yes, unless you added weapons or skins of your own. |
| `addons/sourcemod/scripting/**` | Yes for the server: it is used only to compile. It replaces include files with the same name, such as `left4dhooks.inc` or `readyup.inc`; if you compile other plugins on the server, do not copy this folder. |
| `cfg/playstats.cfg` | **No.** It has your address and token. Skip it, or copy it again and paste your two lines back. |

On Windows, when the copy asks about files that already exist, replace them and skip `playstats.cfg`. On Linux:

```bash
rsync -a --exclude cfg/playstats.cfg left4dead2/ /path/to/server/left4dead2/
```

The skins plugin keeps two cache files in `addons/sourcemod/data/` (`weapon_skins_cache.json` and `weapon_skins_players_cache.json`), so it can apply the skins while the site is down. The update does not touch them.

## Weapon skin and patent files

`l4d2_show_patent_icon` and `l4d2_weapon_skins` show files that are not part of the game, so players must download them. They are in [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets):

| Files | On the game server | On the FastDL |
|---|---|---|
| `models/l4d2br/weapons/<weapon>/` (one combined model per weapon) | Yes, in the game folder | Yes |
| `materials/models/l4d2br/weapons/<weapon>_<id>/` (the textures of each skin) | Yes, in the game folder | Yes |
| `fastdl/materials/sprites/patent_<level>_v02.vmt` and `.vtf` (patent icons) | No | Yes, as `materials/sprites/` |

1. Copy `models/` and `materials/` of the assets repository into the `left4dead2` folder of the server. Every `view_model`, `world_model`, `models` and `materials` path in `addons/sourcemod/configs/weapon_skins.cfg` must exist there; when one does not, the plugin skips that weapon or skin and, with the skins on, logs `models were not found on the server` or `materials were not found on the server`.
2. Put `models/`, `materials/` and the contents of `fastdl/` on a web server that players can reach, in a folder laid out like the game folder. Files are served as they are, without `.bz2`.
3. Point the game at it in `cfg/server.cfg`:

   ```text
   sm_cvar sv_allowdownload 1
   sm_cvar sv_downloadurl "https://files.example.com/fastdl"
   ```

4. Turn the skins on with `weapon_skins_enabled 1` in `cfg/playstats.cfg`, and restart the server while it is empty, so players download the files when they connect and not during a map change.

Without `sv_downloadurl`, the game sends the files itself, very slowly; with large models, players may give up before they finish. The plugins add the files to the download list on every map: the patent sprites always, and for skins, the models of every weapon and the materials of the skins someone has bought (all of them with `weapon_skins_preview_all 1`).

Players need `cl_allowdownload 1` and `cl_downloadfilter all` (or `nosounds`) in their game to download the files. `weapon_skins_kick_no_downloads 1` kicks those who refuse, with a message that tells them what to change.

The skins a player can choose come from the site: `weapon_skins.cfg` maps each skin name to its weapon, model family and materials folder, and the shop of the site sells the skins with the same names.

## Commands

| Command | Who | What it does |
|---|---|---|
| `sm_playstats_test` | Server console, or admins with the root flag | Calls the site right away and prints the result (see below). |
| `sm_ranking` (`!ranking`) | Everyone | Opens the in-game ranking page. |
| `sm_skin`, `sm_skins` (`!skin`) | Everyone | `!skin` shows the help; `!skin next`, `!skin prev`, `!skin <name>` and `!skin default` choose the skin of the weapon in hand, or of the weapon given after it; `!skin status` shows the coins and the skins owned. Survivors change skins during the ready-up. |
| `sm_skin on`, `sm_skin off`, `sm_skin test <skin\|default> [weapon]` | Admins with the root flag; `on` and `off` also from the server console | `on` and `off` override `weapon_skins_enabled` until the plugin is loaded again (a restart, or a match mode that reloads the plugins). `test` tries a skin on yourself until the map changes, without buying it. The access can be changed with the `sm_weaponskins_admin` override in `addons/sourcemod/configs/admin_overrides.cfg`. |

## Checking the installation

1. In the server console, run `sm exts list`. One line must be `REST in Pawn`. If it is missing, REST in Pawn is not installed, or it is not the build for your system.
2. Run `sm plugins list`. It must show `L4D2 - Player Statistics Sync` and the other plugins you copied, none of them marked as failed. When a plugin cannot load, SourceMod lists it with the reason, for example a required extension or plugin that is not running.
3. Run `sm_playstats_test`. It first shows the result of the last automatic call, then calls the site and prints one of the lines below.
4. On the site, open **Administration → Game servers** and the server. Its status changes by itself to **Connected**, with a checklist: connection with this site, statistics upload plugin loaded, statistics writing enabled, first statistics file received (after the first complete map), live match data arriving, and the skins plugin when the shop is on.

| Result of `sm_playstats_test` | Meaning |
|---|---|
| `Connected to the site "..." as the game server "..."` | Everything is right. The next lines list the modules turned on in the site, the plugins loaded, whether statistics writing is on, and how many statistics files are waiting. |
| `URL not set` | `playstats_url` is empty. |
| `Token not set` | `playstats_token` is empty. |
| `Cannot reach ...` | The address did not answer. The text in parentheses is the error of the connection. |
| `Unknown identifier` | The site has no game server with the identifier in the token. |
| `Wrong key` | The identifier exists, but the key does not match. |
| `Token not accepted` | The token is not in the form `<identifier>:<key>`. |
| `This game server is disabled on the site` | An administrator turned the server off on the site. |
| `... answered HTTP ...` | Something answers at that address, but it is not L4D2 PlayStats Hub. |

Over RCON only the first lines come back, because the answer of the site arrives after the command has returned. Use the server console, or run the command in game as a root admin.

The plugins write a problem once to the SourceMod error log (`addons/sourcemod/logs/errors_<date>.log`) when it starts, and a line to the SourceMod log (`addons/sourcemod/logs/L<date>.log`) when it is solved, so a site that is down does not fill the logs.

## Troubleshooting

| What you see | Cause and fix |
|---|---|
| `sm_playstats_test` is an unknown command | `l4d2_playstats_sync.smx` is not loaded. Check that it is in `addons/sourcemod/plugins/optional/`, and look at `sm plugins list` for its error. With L4D2 Competitive Rework, check `cfg/sharedplugins.cfg`. |
| `sm plugins list` says a plugin needs the extension REST in Pawn (`rip.ext`) | REST in Pawn is missing, or it is the build of another system. Install the right zip and restart the server. |
| `sm plugins list` says a plugin needs the plugin `left4dhooks` | `left4dhooks.smx` is not loaded. Install Left 4 DHooks Direct with its gamedata files. |
| `URL not set` or `Token not set` | The lines did not run. Check that `cfg/server.cfg` ends with `exec playstats.cfg`, that the two lines in `cfg/playstats.cfg` do not start with `//`, and restart the server. |
| `Unknown identifier`, `Wrong key` or `Token not accepted` | Paste the token again from the page of the server on the site. If the key was lost, click **Generate a new key** there and paste the new line. |
| `Cannot reach ...` with a timeout or "Couldn't resolve host" | The game server cannot open the site. From the game server machine, run `curl -I <playstats_url>`. Check the address, the DNS, the firewall, and that the site is running. |
| `Cannot reach ...` with "SSL certificate problem" | The certificate of the site is not trusted. Use a certificate from a public authority, such as Let's Encrypt; self-signed certificates do not work. If the certificate is fine, `addons/sourcemod/configs/ripext/ca-bundle.crt` may be old: replace it with the one of the latest REST in Pawn. |
| `... answered HTTP 301`, `302`, `404` or `405` | The address is not the site itself, or it redirects. Use the final address, usually with `https://`: a redirect can turn the plugin's POST into a GET. |
| Connected, but no statistics file arrives | Play a complete map in versus. Check that `l4d2_playstats` is loaded in that match mode and that `sm_playstats_test` says statistics writing is on. |
| Matches arrive but do not count for the ranking | On the site, open **Administration → Game modes** and see which rule the configuration falls into. |
| The plugins work outside matches but stop when a match mode loads | They are not in `cfg/sharedplugins.cfg` (see [L4D2 Competitive Rework match modes](#l4d2-competitive-rework-match-modes)). |
| Confogl prints "Tracked Server CVar ... changed" on every map | The same setting is in `cfg/playstats.cfg` and under `confogl_addcvar`. Keep it in one place. |
| `!ranking` opens an empty panel | Set `playstats_ingame_url` to a plain HTTP address of the site. |
| The chat shows the IP address of the site instead of its name | Set `playstats_public_url` to the address players know, for example `https://stats.example.com`. |
| Patent icons do not show | Check `patent_icon_enabled 1`. Then players may not have the sprites: check `sv_downloadurl` and that `patent_icon_version` matches the file names. |
| Skins never show | Check `weapon_skins_enabled 1`, that the module is on in **Administration → Modules**, that the files are on the server (look for `not found on the server` in the error log) and on the FastDL, and that the skin was bought on the site. |

## What the plugins call

Every request carries the header `Authorization: <playstats_token>`. SteamID64 values travel as strings.

| Plugin | Request |
|---|---|
| `l4d2_playstats_sync` | `POST /api/servers/heartbeat`, `POST /api/statistics` |
| `l4d2_gameinfo_sync` | `PUT /api/game-info/configuration`, `PUT /api/game-info/round`, `PUT /api/game-info/scoreboard`, `PUT /api/game-info/players`, `PUT /api/game-info/messages`, `POST /api/game-info/events`, `POST /api/game-info/player-connection-info`, `GET /api/game-info/server-command/dequeue`, `GET /api/external-chat` |
| `l4d2_ranking` | None. It opens `/ingame?steamid=...` in the player's game. |
| `l4d2_show_patent_icon` | `GET /api/players/patents` |
| `l4d2_weapon_skins` | `GET /api/skins`, `GET /api/players/skins`, `GET /api/players/{id}/wallet`, `POST /api/players/{id}/skins/equip` |
| `l4d2-anti-cheat` (separate, 1.5.0 or newer, with `l4d2_web_url` and `l4d2_web_secret`) | `GET /api/anti-cheat-access/server`, `PUT /api/anti-cheat-access/server/mode`, `POST /api/anti-cheat-access/server/{list}`, `DELETE /api/anti-cheat-access/server/{list}?player=<steamid>` |

The heartbeat is sent when the configuration files have run, then every 30 seconds while the server is not hibernating, and right after `playstats_url` or `playstats_token` changes. Only one heartbeat is in flight at a time, with the default timeouts of REST in Pawn (10 seconds to connect, 30 in total).

Statistics files are sent one at a time, ten seconds after each round start. When the site does not answer, the files stay on the server and are tried again at the next round start. A file the site answered "not ready" for is sent again only after it changes, or after 30 minutes. A file is deleted from the game server only when the site says so.

`l4d2_gameinfo_sync` sends the live data every 5 seconds, with a 3-second connect timeout and a 5-second total timeout. A configuration, round, scoreboard, players, site chat or command request is not sent while the previous one of the same kind is waiting, so a slow site does not pile them up. When a module is turned off on the site, its requests get `404` and the plugins ignore them; the skins plugin logs it once.

The shared code is in `scripting/include/l4d2_playstats_hub.inc`: the two cvars, the request with the token, and the heartbeat.

## Building

You only need this to change the plugins. The compiler is `spcomp`, from the SourceMod package (`addons/sourcemod/scripting/`). The committed plugins are built with **SourceMod 1.12.0.7260**; download that build from <https://github.com/alliedmodders/sourcemod/releases/tag/1.12.0.7260> and unpack it into `plugins/.sourcemod/` (ignored by Git), or point to it with `SPCOMP`.

Windows:

```powershell
./build.ps1
./build.ps1 -Spcomp "C:\sourcemod\addons\sourcemod\scripting\spcomp.exe"
```

Linux and macOS:

```bash
./build.sh
SPCOMP=~/sourcemod/addons/sourcemod/scripting/spcomp64 ./build.sh
```

Both compile every `.sp` file in `left4dead2/addons/sourcemod/scripting/` straight into `left4dead2/addons/sourcemod/plugins/`, and `l4d2_playstats_sync`, `l4d2_ranking` and `l4d2_show_patent_icon` into its `optional/` folder. Commit the `.smx` files together with the sources. The standard SourceMod include files are taken from the `include` folder next to the compiler; set `SOURCEMOD_INCLUDE` if they are somewhere else.

The build is reproducible: the same sources and the same compiler give the same bytes on Windows and Linux. To get there, the scripts compile in a temporary folder with relative include paths, fix the compile date and time that SourceMod embeds (`__DATE__` and `__TIME__`), write the plugins uncompressed (`-z0`, because zlib does not compress the same way on every system) and every plugin sets its stack size with `#pragma dynamic` (otherwise the compiler's estimate changes from one run to the next). Each plugin is also compiled once with `-E`, so any warning fails the build.

`./build.sh --check` (or `./build.ps1 -Check`) compiles into the temporary folder and fails if a committed `.smx` is missing, different from the result, has no source, or is not in its folder. The `Plugins` workflow runs it on every push and pull request that touches `plugins/`, with SourceMod 1.12.0.7260 checked against its SHA-256, and keeps the `left4dead2` folder as an artifact of the run. The release workflow runs the same check and packs the committed `left4dead2` folder, this guide, the license and the notice into `l4d2-playstats-hub-plugins.zip`.

Code rules: no comments, constants as `#define` at the top of the file, and third-party include files stay exactly as they came.

## Not yet tested on a game server

The plugins compile without warnings and the build was checked on Windows and Linux, but this version was not run on a live game server yet. Until it is, treat these points as expected behaviour, not as tested facts:

- the plugins load from `addons/sourcemod/plugins/` and `addons/sourcemod/plugins/optional/` as uncompressed `.smx` files (SourceMod supports them, but this build was not loaded by a server);
- the flow `server.cfg` → `exec playstats.cfg` → `cfg/sharedplugins.cfg` on a server with L4D2 Competitive Rework;
- the changes of this version: the tank and witch percent without `l4d_boss_percent`, the patent refresh after the configs run, and the error log lines written once;
- the exact wording of SourceMod's own messages, which this guide describes instead of quoting.

## License

The plugins are released under the [GNU General Public License, version 3](LICENSE), as SourceMod requires. [NOTICE.md](NOTICE.md) lists the third-party include files and where they come from.
