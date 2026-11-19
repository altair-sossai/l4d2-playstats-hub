# Switching from the old site

🌐 **English** | [Português](switch-from-old-site.pt.md) | [Español](switch-from-old-site.es.md)

How to replace a running copy of the earlier site (`l4d2-playstats-web` with `l4d2-playstats-api`) with this one, on the same machine, without changing the addresses players already use. The example is the setup of l4d2.com.br: one Ubuntu VPS at `124.198.128.43` where nginx serves `https://l4d2.com.br` and passes it to the old site on port `5000`, the game opens `http://124.198.128.43:5000`, and the game server is based on [l4d2-zone-server](https://github.com/altair-sossai/l4d2-zone-server). Put your own names and addresses in their place.

## Contents

- [The plan](#the-plan)
- [Part 1: install the new site beside the old one](#part-1-install-the-new-site-beside-the-old-one)
- [Part 2: test with a game server](#part-2-test-with-a-game-server)
- [Part 3: the switch](#part-3-the-switch)
- [Part 4: going back](#part-4-going-back)
- [The old data](#the-old-data)

## The plan

| Part | What changes | What players notice |
|---|---|---|
| 1 | The new site runs on port `5090`, next to the old one. nginx and the old site are not touched. | Nothing. |
| 2 | One game server sends its matches to the new site. | The matches of that server stop reaching the old site. |
| 3 | The old site stops and the new one takes its port, `5000`. nginx keeps passing `https://l4d2.com.br` there. | The new pages, at the same addresses. |
| 4 | Only if something goes wrong: the old site comes back. | The old pages again. |

Keep `/home/l4d2-playstats-web`, the folder of the old site, as it is until you are sure: going back needs it.

## Part 1: install the new site beside the old one

1. Connect to the VPS with SSH, as a user that can run `sudo`.
2. Run the installer without a domain. Without `HUB_DOMAIN` it does not install Caddy, so nginx and ports 80 and 443 stay as they are:

   ```bash
   curl -fsSL https://raw.githubusercontent.com/altair-sossai/l4d2-playstats-hub/main/deploy/install.sh | sudo HUB_PORT=5090 bash
   ```

3. Open port `5090`. It is not the installer's default `5080` because on this VPS that port is already taken by the anti-cheat API (`127.0.0.1:5080`). When UFW is active the installer already did it; otherwise run `sudo ufw allow 5090/tcp`. If your hosting provider has its own firewall, open the port there too.
4. Open `http://124.198.128.43:5090`, paste the setup code the installer printed, and sign in with Steam. That account becomes the first administrator.
5. Follow the wizard ([First-run setup](manual/first-run.md)). On the **Modules** page turn on what the old site had: ranking, live server, site chat, remote commands, skin shop and Anti-Cheat page. On the **Game server** page create the game server and copy the two lines it shows, `playstats_url` and `playstats_token`: the key is shown only once.
6. Go through the rest of the administration ([Administration](manual/administration.md)): **Settings → Privacy** (who sees the related accounts), **Administration → Game modes** (which configurations count for the ranking), the seasons, the Steam Web API key, the other administrators and the skin catalog of the shop. Leave **Public address of the site** empty for now.

[Installing on a Linux VPS](install-linux.md) explains each step of the installer and the `playstats-hub` commands.

## Part 2: test with a game server

A game server sends its matches to one site only. While it points to the new site, its matches no longer reach the old one, and they do not go there later. Test with a second game server if you have one, or at a quiet time with the main one.

### Back up what the plugins replace

On the game server, save a copy of the files the new plugins replace, for [Part 4](#part-4-going-back):

```bash
cd /path/to/the/server/left4dead2
tar -czf ~/playstats-plugins-backup.tar.gz addons/sourcemod/plugins addons/sourcemod/translations addons/sourcemod/configs/weapon_skins.cfg cfg
```

If the game server is deployed from a repository, such as `l4d2-zone-server`, make the changes below in the repository too, or the next deploy brings the old plugins and settings back.

### Copy the plugins

Copy everything inside the `left4dead2` folder of the plugins (in the release zip `l4d2-playstats-hub-plugins.zip`, or [plugins/left4dead2](../plugins/left4dead2) in this repository) over the `left4dead2` folder of the server:

| File | What it replaces |
|---|---|
| `addons/sourcemod/plugins/l4d2_gameinfo_sync.smx` and `l4d2_weapon_skins.smx` | The old ones in the same place. |
| `addons/sourcemod/plugins/optional/l4d2_playstats_sync.smx`, `l4d2_ranking.smx` and `l4d2_show_patent_icon.smx` | The old ones in the same place, which the `shared_plugins.cfg` of the match modes already loads. |
| `addons/sourcemod/translations/**` | The texts of the old versions. Copy them together with the `.smx` files: the ranking texts changed. |
| `addons/sourcemod/configs/weapon_skins.cfg` | The same weapons and skins. |
| `addons/sourcemod/scripting/**` | The sources, only used to compile. |
| `cfg/playstats.cfg` | Nothing: it is new. |

There must be only one copy of each plugin. None of the five may stay in another folder, such as an old `l4d2_playstats_sync.smx` in `addons/sourcemod/plugins/` itself.

### Change the settings

| File | Change |
|---|---|
| `cfg/cfgogl/zonemod/shared_cvars.cfg` and `cfg/cfgogl/neomod/shared_cvars.cfg` | Delete `confogl_addcvar playstats_url ...`, the old API, which would point the five plugins back at it on every match, and `confogl_addcvar ranking_url ...`, which no longer exists. Keep the other lines: `patent_icon_enabled`, `weapon_skins_*`, `gameinfo_*` and `anti_cheat_*` work as before. |
| `cfg/server_secrets.cfg` | Delete `playstats_access_token`. Add `playstats_url` and `playstats_token` (below). Set `l4d2_web_secret` to the same token as `playstats_token`. |
| `cfg/server.cfg` | Set `l4d2_web_url` to the address of the new site. Optionally add `sm plugins load optional/l4d2_playstats_sync.smx` at the end, so the site sees the server while no match mode is loaded. |
| `cfg/sharedplugins.cfg` | Optional: the `sm plugins load` lines of [Step 5](game-server.md#step-5-load-the-plugins-in-your-match-modes), for the match modes that do not load these plugins yet (on `l4d2-zone-server`, every mode except ZoneMod and NeoMod). |

The lines for `cfg/server_secrets.cfg`, with your token in place of the example:

```text
playstats_url "http://124.198.128.43:5090"
playstats_token "my-server:3f9c…"
l4d2_web_secret "my-server:3f9c…"
```

And the line for `cfg/server.cfg`:

```text
sm_cvar l4d2_web_url "http://124.198.128.43:5090"
```

- `playstats_url` is a plain HTTP address, so the browser inside the game opens it too and `playstats_ingame_url` can stay empty.
- `l4d2_web_url` and `l4d2_web_secret` are read only by the anti-cheat plugin (1.5.0 or newer), for the lists of [Who must use the Anti-Cheat](manual/anti-cheat.md#who-must-use-the-anti-cheat). The old lists are not copied: add the players again on the new site before you turn the anti-cheat on.
- Leave `playstats_public_url` off for now. It is set in Part 3; before that, the chat would send players to the shop of the old site.
- With these lines in the secrets file you do not need `exec playstats.cfg`: the rest of `cfg/playstats.cfg` only shows the defaults, and the match modes of `l4d2-zone-server` already set `sm_stats_writestats`.
- Never set `playstats_token` or `l4d2_web_secret` with `confogl_addcvar`: Confogl prints the values it tracks in the chat.

### Check

1. Restart the game server while it is empty, and load a match mode, for example with `!match`.
2. In the server console, `sm plugins list` must show the five plugins, none of them failed and none of them twice. Then `sm_playstats_test` must answer `Connected to the site`.
3. On the new site, open **Administration → Game servers** and the server. It must say **Connected**, with the checklist of [Step 6](game-server.md#step-6-check-the-connection).
4. Play a complete map. The match appears in **Matches**, the round in **Server**, and the site chat reaches the game.
5. In the game, `!ranking` opens the ranking panel. With `weapon_skins_enabled 1`, `!skin status` answers; with `patent_icon_enabled 1`, the patent icons show before the match.

## Part 3: the switch

Do it while nobody is playing. Both sites listen on port `5000` after this, so the old one stops first.

1. Stop the old site, keeping its files: `screen -S l4d2-playstats-web -X quit`, as the user that started it. If `screen -ls` does not list it, run both commands with `sudo`.
2. Stop it from starting at boot: run `crontab -e` as `devops` and delete the `@reboot` line of `l4d2-playstats-web`, or put `#` in front of it.
3. Turn off the automatic deploy of the old site, the Azure Pipelines pipeline of `l4d2-playstats-web`. Otherwise a push starts it again on port `5000`.
4. Move the new site to port `5000`: in `/etc/l4d2-playstats-hub/hub.env`, change `ASPNETCORE_URLS=http://0.0.0.0:5090` to `ASPNETCORE_URLS=http://0.0.0.0:5000`, then run `sudo playstats-hub restart` and `playstats-hub status`.
5. Check it: `curl -s http://127.0.0.1:5000/api/ping` answers, `https://l4d2.com.br` and `http://124.198.128.43:5000` show the new site, and the Steam sign-in works at `https://l4d2.com.br`.
6. In **Administration → Settings → Site**, set **Public address of the site** to `https://l4d2.com.br`.
7. On the game server, point everything at port `5000` and set the address players read in chat: the first two lines in `cfg/server_secrets.cfg`, the last one in `cfg/server.cfg`. Then change the map or restart the server, and run `sm_playstats_test` again:

   ```text
   playstats_url "http://124.198.128.43:5000"
   playstats_public_url "https://l4d2.com.br"
   sm_cvar l4d2_web_url "http://124.198.128.43:5000"
   ```

8. Close port `5090`, which is no longer used: `sudo ufw delete allow 5090/tcp`, and in the firewall of the provider if you opened it there.

The plugins could also use `https://l4d2.com.br`; the plain address keeps them working whatever happens to the certificate. nginx needs no change: it still passes `https://l4d2.com.br` to port `5000`, and the new site believes the `X-Forwarded-*` headers of a proxy on the same machine. If the Steam sign-in comes back to an `http://` address or to the IP address, the `location` of nginx must pass these headers:

```nginx
proxy_set_header Host $host;
proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
proxy_set_header X-Forwarded-Proto $scheme;
```

## Part 4: going back

If something does not work and cannot be fixed right away:

1. Stop the new site and keep it from starting at boot: `sudo systemctl disable --now l4d2-playstats-hub`. Its data stays in `/var/lib/l4d2-playstats-hub`.
2. Start the old site as before: `cd /home/l4d2-playstats-web && screen -d -m -S l4d2-playstats-web sh -c './L4D2PlayStats.Web'`. Put its `@reboot` line back and turn its pipeline on again.
3. On the game server, extract the backup in the `left4dead2` folder (`tar -xzf ~/playstats-plugins-backup.tar.gz`) and restart the server. It brings back the old plugins and every setting under `cfg`.

The matches played while the game server pointed to the new site stay only there; the old site does not get them. To try again later, set `ASPNETCORE_URLS` back to port `5090` in `/etc/l4d2-playstats-hub/hub.env` and run `sudo systemctl enable --now l4d2-playstats-hub`.

## The old data

The new site starts empty. Bringing the old statistics is optional: a separate program uploads the old statistics files through the same path the plugins use, and the site builds the matches, the ranking and the history from them ([Data migration](roadmap.md#data-migration)). It can run before or after the switch, and the matches the new site already has stay. Nothing else of the old site is copied: the skin purchases, gifts and equipped skins, the chat bans and the Anti-Cheat lists start empty, so add the chat bans and the Anti-Cheat players again on the new site.
