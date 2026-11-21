# Connecting a game server

🌐 **English** | [Português](game-server.pt.md) | [Español](game-server.es.md)

How to make a Left 4 Dead 2 server send its matches to your site, step by step. You repeat these steps for each game server; one site can receive any number of them. Every setting, command and file is described in detail in [the plugins guide](../plugins/README.md).

## Contents

- [What the game server needs](#what-the-game-server-needs)
- [Step 1: register the server on the site](#step-1-register-the-server-on-the-site)
- [Step 2: install REST in Pawn](#step-2-install-rest-in-pawn)
- [Step 3: copy the plugins](#step-3-copy-the-plugins)
- [Step 4: set the address and the token](#step-4-set-the-address-and-the-token)
- [Step 5: load the plugins in your match modes](#step-5-load-the-plugins-in-your-match-modes)
- [Step 6: check the connection](#step-6-check-the-connection)
- [Optional: weapon skins and patent icons](#optional-weapon-skins-and-patent-icons)
- [Optional: the Anti-Cheat plugin](#optional-the-anti-cheat-plugin)
- [The ranking inside the game](#the-ranking-inside-the-game)
- [What each status means](#what-each-status-means)
- [Updating the plugins](#updating-the-plugins)
- [Coming from the old setup](#coming-from-the-old-setup)
- [If something goes wrong](#if-something-goes-wrong)

## What the game server needs

The plugins are made for servers based on [L4D2 Competitive Rework](https://github.com/SirPlease/L4D2-Competitive-Rework), such as ZoneMod servers. Competitive Rework already brings almost everything they need:

| Requirement | Why | Comes with Competitive Rework |
|---|---|---|
| SourceMod 1.12 or newer | Runs the plugins. | Yes |
| [REST in Pawn](https://github.com/ErikMinekus/sm-ripext/releases) extension | Lets the plugins call the site. | **No: step 2** |
| [Left 4 DHooks Direct](https://github.com/SilvDev/Left4DHooks/releases) (`left4dhooks`) | Used by the live match, ranking and patent plugins. | Yes |
| `l4d2_playstats` plugin | Writes the statistics file of each map. | Yes |
| `readyup`, `pause`, `l4d_boss_percent`, `l4d2_hybrid_scoremod`, `l4d2_skill_detect` | Optional: more details in the live match. | Yes |

On a server without Competitive Rework, install SourceMod 1.12 and Left 4 DHooks Direct first; the statistics need `l4d2_playstats`, which comes only with Competitive Rework. The full list, with links, is in [the requirements of the plugins guide](../plugins/README.md#requirements).

## Step 1: register the server on the site

1. Sign in to the site as an administrator.
2. Open **Administration → Game servers**.
3. Type a name and click **Create**.

The site shows two lines with the address of the site and the token of this server:

```text
playstats_url "https://stats.example.com"
playstats_token "my-server:3f9c…"
```

Copy them now. The key inside the token is shown only once. If you lose it, click **Generate a new key** on the same page; the old key stops working at once.

## Step 2: install REST in Pawn

1. Download the latest `sm-ripext-<version>-linux.zip` (Linux server) or `sm-ripext-<version>-windows.zip` (Windows server) from the [REST in Pawn releases](https://github.com/ErikMinekus/sm-ripext/releases).
2. Extract it into the `left4dead2` folder of the server, the one with `gameinfo.txt`. It adds `addons/sourcemod/extensions/rip.ext.so` (or `.dll`) and `addons/sourcemod/configs/ripext/ca-bundle.crt`.

Skip this step if `sm exts list`, in the server console, already shows `REST in Pawn`.

## Step 3: copy the plugins

Get the plugins in one of two ways:

- download `l4d2-playstats-hub-plugins.zip` from the [releases page](https://github.com/altair-sossai/l4d2-playstats-hub/releases) and extract it, or
- download or clone [this repository](https://github.com/altair-sossai/l4d2-playstats-hub) and open its `plugins` folder.

Both have a folder named `left4dead2` laid out like the game folder of the server. **Copy everything inside that `left4dead2` folder into the `left4dead2` folder of the server.** Every file lands in its place:

| File | What it is |
|---|---|
| `addons/sourcemod/plugins/*.smx` | `l4d2_gameinfo_sync` and `l4d2_weapon_skins`. |
| `addons/sourcemod/plugins/optional/*.smx` | `l4d2_playstats_sync`, `l4d2_ranking` and `l4d2_show_patent_icon`, which the match modes load. |
| `addons/sourcemod/scripting/` | Their sources, only for compiling. |
| `addons/sourcemod/translations/` | The chat texts. |
| `addons/sourcemod/configs/weapon_skins.cfg` | The weapons and skins of the skins plugin. |
| `cfg/playstats.cfg` | Your settings. You edit it in the next step. |

The three plugins in `optional/` load only with a match mode (step 5), where Competitive Rework servers already keep them. So that the site sees the server while no match is being played, step 4 also loads `l4d2_playstats_sync` from `cfg/server.cfg`.

`l4d2_playstats_sync.smx` is the only one every server needs. Delete the `.smx` of the plugins you do not want:

| Plugin | What it does | Module of the site |
|---|---|---|
| `l4d2_playstats_sync` | Sends the statistics files and tells the site the server is alive. | Always needed |
| `l4d2_gameinfo_sync` | Sends the live match: scoreboard, players, events and chat. | Live server |
| `l4d2_ranking` | Opens the ranking inside the game with `!ranking`. | Ranking |
| `l4d2_show_patent_icon` | Shows each player's patent icon in game. | Ranking |
| `l4d2_weapon_skins` | Applies the weapon skins players own. | Weapon skin shop |

## Step 4: set the address and the token

1. Open `cfg/playstats.cfg` on the server.
2. Paste the two lines from step 1 where the file says `Paste the two lines here`.
3. Open `cfg/server.cfg` and add these lines at the end:

   ```text
   exec playstats.cfg
   sm plugins load optional/l4d2_playstats_sync.smx
   ```

That is all you must change. The rest of `cfg/playstats.cfg` lists every other setting of the five plugins, turned off with `//` and with its default value; each one has an explanation above it.

`cfg/playstats.cfg` also turns on `sm_stats_writestats 1`, which makes `l4d2_playstats` write the statistics files. Its default is `0`, and the ZoneMod match modes of Competitive Rework do not change it. If your match modes already set it with `confogl_addcvar`, delete that line from `cfg/playstats.cfg`.

Each setting belongs in one place: `cfg/playstats.cfg` for the ones that are the same in every match mode, or `confogl_addcvar` in a match mode for one that must differ. Never set `playstats_url` or `playstats_token` with `confogl_addcvar`: Confogl prints the old and the new value of the settings it tracks in the chat, and the players would see the token.

## Step 5: load the plugins in your match modes

When a match mode of Competitive Rework loads, it unloads every plugin and loads only the ones in its lists. The list shared by every mode is `cfg/sharedplugins.cfg`. Add these lines to it, leaving out the plugins you deleted:

```text
sm plugins load optional/l4d2_playstats_sync.smx
sm plugins load l4d2_gameinfo_sync.smx
sm plugins load optional/l4d2_ranking.smx
sm plugins load optional/l4d2_show_patent_icon.smx
sm plugins load l4d2_weapon_skins.smx
```

Then restart the game server.

Statistics are sent from every mode. On the site, **Administration → Game modes** decides which modes count toward the ranking.

## Step 6: check the connection

On the site, the page of the game server updates by itself. In a few seconds it should say **Connected**, with a list of what was found:

- Connection with this site
- Statistics upload plugin loaded
- Statistics writing enabled (it turns on when a match mode loads `l4d2_playstats`)
- First statistics file received (after the first complete map)
- Live match data arriving (when the live server module is on)
- Weapon skins plugin loaded (when the skin shop module is on)

You can also check from the game server. Type these in its console:

```text
sm exts list
sm plugins list
sm_playstats_test
```

`sm exts list` must show `REST in Pawn`, and `sm plugins list` must show the plugins you copied, none of them marked as failed. `sm_playstats_test` answers with the name of the server and of the site, the modules turned on and the plugins loaded, or says exactly what is wrong. [The plugins guide](../plugins/README.md#checking-the-installation) explains each answer.

## Optional: weapon skins and patent icons

The patent icons and the weapon skins are files that are not part of the game, so players download them from your server. They are in [l4d2-zone-server-assets](https://github.com/altair-sossai/l4d2-zone-server-assets).

1. Copy its `models/` and `materials/` folders into the `left4dead2` folder of the server.
2. Put `models/`, `materials/` and the contents of `fastdl/` on a web server players can reach (a FastDL), laid out like the game folder.
3. In `cfg/server.cfg`, set `sm_cvar sv_allowdownload 1` and `sm_cvar sv_downloadurl "<address of that folder>"`.
4. In `cfg/playstats.cfg`, turn the skins on: delete the `//` in front of `weapon_skins_enabled` and set it to `"1"`.
5. To show the patent icons too, do the same with `patent_icon_enabled`.
6. Restart the server while it is empty.

The details, and how to check that the files are found, are in [Weapon skin and patent files](../plugins/README.md#weapon-skin-and-patent-files).

## Optional: the Anti-Cheat plugin

The anti-cheat plugin (`l4d2-anti-cheat.smx`, version 1.5.0 or newer) is not part of this package: it comes with the Anti-Cheat program and its own service. Connected to the site, it reads from the site who must use the Anti-Cheat, and the changes made in the game with `!aac config` are saved on the site. See [Who must use the Anti-Cheat](manual/anti-cheat.md#who-must-use-the-anti-cheat).

1. On the site, turn on the **Anti-Cheat** module in **Administration → Modules**.
2. Add these two lines to `cfg/playstats.cfg`, with the same address and token as `playstats_url` and `playstats_token`:

   ```text
   l4d2_web_url "https://stats.example.com"
   l4d2_web_secret "my-server:3f9c…"
   ```

3. Choose the mode the game server uses until one is saved on the site, with `anti_cheat_mode` in the `shared_cvars.cfg` of your match modes, for example `confogl_addcvar anti_cheat_mode 1`.
4. Restart the server while it is empty.

| Setting | What it means |
|---|---|
| `l4d2_web_url` | The address of this site. The plugin calls `<address>/api/anti-cheat-access/server`. |
| `l4d2_web_secret` | The token of this game server, the same value as `playstats_token`. Each game server has its own. |
| `anti_cheat_mode` | `0`: everyone must use the Anti-Cheat, except the players on the whitelist. `1`: only the players on the required list must use it (the default of the plugin). The mode saved on **Administration → Anti-Cheat settings** wins; this value counts only until a mode is saved there. |

With `anti_cheat_enabled 0` in the match modes, version 1.6.0 of the plugin lets the players turn the Anti-Cheat on for the match with `!aac vote`; see [Turned on by the players](manual/anti-cheat.md#turned-on-by-the-players).

Never set `l4d2_web_secret` with `confogl_addcvar`, for the same reason as `playstats_token`. The other settings of the plugin, such as the address and the token of its own service, do not change.

After a change on the site, `l4d2_gameinfo_sync` runs `sm_aac sync` on the game server within seconds. Without the live server module, the plugin reads the site by itself every minute. To check the connection, type `sm_aac sync` and then `sm_aac list` in the console of the game server, or ask the site from that machine:

```text
curl -H "Authorization: my-server:3f9c…" https://stats.example.com/api/anti-cheat-access/server
```

The answer has the mode (`null` until one is saved) and the two lists. `401` means the token is wrong, and `404` that the Anti-Cheat module is off.

## The ranking inside the game

`!ranking` opens the ranking of the home page of the site in the game's own browser; `ranking_motd_path "/ingame"` opens a simpler, compact version instead. Pages opened from the game do not show the invitation to vote in the community ranking. That browser is from 2012 and is not expected to open current HTTPS sites, so give it a plain HTTP address of your site in `cfg/playstats.cfg`:

```text
playstats_ingame_url "http://203.0.113.10:5080"
```

When `playstats_ingame_url` is empty, the plugin uses `playstats_url`. Do not use an HTTPS address or one that redirects to HTTPS: the panel stays empty. [Installing on a Linux VPS](install-linux.md#the-address-used-inside-the-game) explains which address to use.

The address players read in chat, in the ranking hint and in the skin shop link, is `playstats_public_url`; when it is empty, the plugins show `playstats_url`. Set it when the plugins reach the site by an IP address but players know it by a name:

```text
playstats_public_url "https://stats.example.com"
```

## What each status means

| Status | Meaning | What to do |
|---|---|---|
| Waiting for the first connection | The server never reached the site. | Check `cfg/playstats.cfg` and the `exec playstats.cfg` line, and that `l4d2_playstats_sync.smx` is loaded. Run `sm_playstats_test`. |
| Connected | A heartbeat arrived in the last seconds. | Nothing. |
| Offline | The server was seen before but not now. | Normal while the game server is off, or empty and hibernating. |
| Invalid key | The identifier arrived with the wrong key. | Paste the token again, or generate a new key on the site. |

## Updating the plugins

Copy the new `left4dead2` folder over the old one, **except `cfg/playstats.cfg`**, which has your address and token. Every other file can be replaced; see [Updating](../plugins/README.md#updating) for the exceptions and a command that skips that file. Then restart the server while it is empty.

## Coming from the old setup

If the game server used the earlier plugins, remove their settings, or they keep pointing to the old addresses:

| Old setting | Now |
|---|---|
| `playstats_url` pointing to the old API | The address of this site |
| `playstats_access_token` | `playstats_token` |
| `l4d2_web_url`, `l4d2_web_secret` | The PlayStats plugins no longer read them: they use `playstats_url` and `playstats_token`. Only the anti-cheat plugin still does; see [Optional: the Anti-Cheat plugin](#optional-the-anti-cheat-plugin). |
| `ranking_url` | `playstats_ingame_url`, only if the game needs a different address |
| The site name in the ranking hint and the address in the skin shop link (`l4d2_web_url`) | `playstats_public_url`, only if players know the site by another address |

Look for them in `cfg/server.cfg`, in your secrets file, and in the `shared_cvars.cfg` of each match mode. Old settings set with `confogl_addcvar playstats_url` must go too, so that `cfg/playstats.cfg` is the only place for the address and the token. Copy the new `.smx` files over the old ones: the three in `addons/sourcemod/plugins/optional/` replace the earlier ones there, so the match modes keep loading them with the lines they already have. If an old copy of one of them is in `addons/sourcemod/plugins/` itself, delete it, so there is only one copy of each plugin.

For the whole move from the earlier site to this one, with the order of the steps and how to go back, see [Switching from the old site](switch-from-old-site.md).

## If something goes wrong

| What you see | What to do |
|---|---|
| `sm_playstats_test` is an unknown command | `l4d2_playstats_sync.smx` is not loaded. Check the folder and run `sm plugins list` to see its error. |
| A plugin needs REST in Pawn | Do step 2 again with the zip of the system of the server. |
| "URL not set" or "Token not set" | The lines did not run. Check that `cfg/server.cfg` ends with `exec playstats.cfg` and that your two lines in `cfg/playstats.cfg` do not start with `//`. |
| "Unknown identifier", "Wrong key" or "Token not accepted" | Paste the token again, or generate a new key on the site. |
| "Cannot reach ..." | The game server cannot open the site. Test the address from that machine with `curl -I`, check the firewall, and see [Troubleshooting](../plugins/README.md#troubleshooting) for HTTPS errors. |
| Works outside matches, stops when a match mode loads | Do step 5. |
| Connected, but no statistics file arrives | Play a complete map in versus. Check that `sm_playstats_test` says statistics writing is on during the match. |
| Matches arrive but do not count for the ranking | On the site, open **Administration → Game modes** and see which rule the configuration falls into. |
| `!ranking` opens an empty panel | Set `playstats_ingame_url` to a plain HTTP address of the site. |
