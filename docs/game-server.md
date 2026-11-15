# Connecting a game server

🌐 **English** | [Português](game-server.pt.md) | [Español](game-server.es.md)

How to make a Left 4 Dead 2 server send its matches to your site. You repeat these steps for each game server; one site can receive any number of them.

## Contents

- [What the game server needs](#what-the-game-server-needs)
- [Step 1: register the server on the site](#step-1-register-the-server-on-the-site)
- [Step 2: copy the plugins](#step-2-copy-the-plugins)
- [Step 3: paste the two lines](#step-3-paste-the-two-lines)
- [Step 4: check the connection](#step-4-check-the-connection)
- [Step 5: load the plugins in your match modes](#step-5-load-the-plugins-in-your-match-modes)
- [The ranking inside the game](#the-ranking-inside-the-game)
- [What each status means](#what-each-status-means)
- [Coming from the old setup](#coming-from-the-old-setup)
- [If something goes wrong](#if-something-goes-wrong)

## What the game server needs

The plugins are made for servers based on [L4D2 Competitive Rework](https://github.com/SirPlease/L4D2-Competitive-Rework), such as ZoneMod servers.

| Requirement | Why |
|---|---|
| SourceMod 1.12 | Runs the plugins. |
| [REST in Pawn](https://github.com/ErikMinekus/sm-ripext) extension | Lets the plugins call the site. |
| `l4d2_playstats` plugin | Comes with L4D2 Competitive Rework. It writes the statistics file of each map. |
| `sm_stats_writestats 1` | Tells `l4d2_playstats` to write those files. The match modes of L4D2 Competitive Rework already set it. |

## Step 1: register the server on the site

1. Sign in to the site as an administrator.
2. Open **Administration → Game servers**.
3. Type a name and click **Create**.

The site shows two lines with the address of the site and the token of this server:

```text
playstats_url "https://stats.example.com"
playstats_token "my-server:3f9c…"
```

Copy them now. The key inside the token is shown only once. If you lose it, click **Generate a new key** on the same page.

## Step 2: copy the plugins

Download `l4d2-playstats-hub-plugins.zip` from the [releases page](https://github.com/altair-sossai/l4d2-playstats-hub/releases). The folders inside it match the folders of the game server, so extract it over the `left4dead2` folder.

| File | Where it goes |
|---|---|
| `l4d2_playstats_sync.smx` | `addons/sourcemod/plugins/` |
| The other `.smx` files | `addons/sourcemod/plugins/`, or `plugins/optional/` if a match mode loads them |
| `translations/` | `addons/sourcemod/translations/` |
| `configs/weapon_skins.cfg` | `addons/sourcemod/configs/` |

Keep `l4d2_playstats_sync.smx` in `plugins/` itself, not in a subfolder. It has to load when the server starts, so the site sees the server even when no match is being played.

| Plugin | What it does | Module of the site |
|---|---|---|
| `l4d2_playstats_sync` | Sends the statistics files and tells the site the server is alive. | Always needed |
| `l4d2_ranking` | Opens the ranking inside the game with `!ranking`. | Ranking |
| `l4d2_show_patent_icon` | Shows each player's patent icon in game. | Ranking |
| `l4d2_gameinfo_sync` | Sends the live match: scoreboard, players, events and chat. | Live server |
| `l4d2_weapon_skins` | Applies the weapon skins players own. | Weapon skin shop |

You only need the plugins of the modules you turned on.

## Step 3: paste the two lines

Open `cfg/server.cfg` on the game server and paste the two lines from step 1 at the end.

Restart the game server, or change the map.

## Step 4: check the connection

On the site, the page of the game server updates by itself. In a few seconds it should say **Connected**, with a list of what was found:

- Connection with this site
- Statistics upload plugin loaded
- Statistics writing enabled
- First statistics file received (after the first complete map)

You can also check from the game server. Type this in its console:

```text
sm_playstats_test
```

It answers with the name of the server and of the site, or says exactly what is wrong.

## Step 5: load the plugins in your match modes

Match modes of L4D2 Competitive Rework unload and load plugins when they start. If you put a plugin in `plugins/optional/`, add it to the plugin list of each mode where it should run, for example in `cfg/cfgogl/zonemod/confogl_plugins.cfg`:

```text
sm plugins load optional/l4d2_ranking.smx
sm plugins load optional/l4d2_show_patent_icon.smx
```

Settings of those plugins go in the same mode, with `confogl_addcvar`. [The plugins guide](../plugins/README.md) lists every setting.

Statistics are sent from every mode. On the site, **Administration → Game modes** decides which modes count toward the ranking.

## The ranking inside the game

`!ranking` opens a simple version of the ranking in the game's own browser. That browser is from 2012 and is not expected to open current HTTPS sites, so give it a plain HTTP address of your site:

```text
confogl_addcvar playstats_ingame_url "http://203.0.113.10:5080"
```

When `playstats_ingame_url` is empty, the plugin uses `playstats_url`. [Installing on a Linux VPS](install-linux.md#the-address-used-inside-the-game) explains which address to use.

## What each status means

| Status | Meaning | What to do |
|---|---|---|
| Waiting | The server never reached the site. | Check the two lines, and that `l4d2_playstats_sync.smx` is in `plugins/`. Run `sm_playstats_test`. |
| Connected | A signal arrived in the last seconds. | Nothing. |
| Offline | The server was seen before but not now. | Normal while the game server is off or empty and hibernating. |
| Invalid key | The identifier arrived with the wrong key. | Paste the token again, or generate a new key on the site. |

## Coming from the old setup

If the game server used the earlier plugins, remove their settings, or they keep pointing to the old addresses:

| Old setting | Now |
|---|---|
| `playstats_url` pointing to the old API | The address of this site |
| `playstats_access_token` | `playstats_token` |
| `l4d2_web_url`, `l4d2_web_secret` | Removed. Everything uses `playstats_url` and `playstats_token`. |
| `ranking_url` | `playstats_ingame_url`, only if the game needs a different address |

Look for them in `cfg/server.cfg`, in your secrets file and in the `shared_cvars.cfg` of each match mode.

## If something goes wrong

| What you see | What to do |
|---|---|
| `sm_playstats_test` is an unknown command | `l4d2_playstats_sync.smx` is not loaded. Check the folder and run `sm plugins list`. |
| "URL not set" | The two lines did not run. They must be in a file the server executes at startup, after the plugin is loaded. |
| "Cannot reach the address" | The game server cannot open the site. Test the address from that machine, and check the firewall. |
| Connected, but no statistics file arrives | Play a complete map in versus. Check `sm_stats_writestats`, and that `l4d2_playstats` is loaded in that mode. |
| Matches arrive but do not count for the ranking | On the site, open **Administration → Game modes** and see which rule the configuration falls into. |
| `!ranking` opens an empty panel | Set `playstats_ingame_url` to a plain HTTP address of the site. |
