# Questions and problems

🌐 **English** | [Português](faq.pt.md) | [Español](faq.es.md)

Common questions from players and administrators, and what to do when something does not work. Problems with the installation itself are also listed at the end of each installation guide: [Linux](../install-linux.md#if-something-goes-wrong), [Docker](../install-docker.md), and the game server in [Connecting a game server](../game-server.md).

[Back to the manual](README.md)

## Contents

- [For players](#for-players)
- [For administrators](#for-administrators)
- [Troubleshooting](#troubleshooting)

## For players

**I played a match and it is not in the ranking.**
Only ranked matches count. A match is ranked when its configuration and team size fit a game mode rule that counts, and it has enough maps: with the default rule, 4v4 and at least 2 maps of the same campaign in a row. Single maps, 1v1 and other modes are listed as **Casual**. The match also appears only after its last map is uploaded, a few seconds after the map ends.

**My win counted as a rage quit.**
A player who was in the first map of the match and is not in the last map is counted as a rage quit, even if they came back later or lost connection. See [Ranking > Wins, losses, draws and rage quits](ranking.md#wins-losses-draws-and-rage-quits).

**I joined in the middle of a match and got no win.**
Wins and losses go to the players who were in the first map. Players who joined later get only their MVP points.

**My experience did not change after a draw.**
A draw gives no win or loss, only the MVP points.

**Why did my experience reset?**
A new season started. The past season is in **More > History** and in the period selector.

**My name is old or wrong.**
The site shows the name you used most often in the period. Your other names are under **Previous names** on your page.

**How do I get stars?**
One star for each SI MVP and each CI MVP award in a ranked match. See [Weapon skin shop > Stars](skin-shop.md#stars).

**My skins disappeared.**
If the site refunds skins at the end of a season, bought skins are taken back when a new season starts, and the stars return to you. Buy them again. Gifts are never taken back.

**I bought a skin and I do not see it in the game.**
It appears from the next map change. In the game, `!skin status` shows what you have.

**The site says I need more matches to vote.**
The community ranking asks for a minimum number of matches, counted over all seasons: 10 by default.

**The live server page is behind the game.**
It is about 15 seconds behind, on purpose.

## For administrators

**How do I add another administrator?**
In [Administration > Administrators](administration.md#administrators), with their SteamID64. If you cannot sign in any more, use `playstats-hub add-admin <SteamID64>` on Linux.

**How do I add a second game server?**
In [Administration > Game servers](administration.md#game-servers). Each server gets its own token. Players' results add up across all servers.

**I lost the token of a game server.**
The site does not keep the key. Open the page of the server and click **Generate a new key**, then put the new token in the game server's configuration.

**I changed the game modes and old matches did not change.**
Rules apply to new files. Use **Reprocess all matches** in the [overview](administration.md#overview).

**I changed the points and every season changed.**
That is expected: rankings are calculated from the matches with the current points. Seasons have no frozen copy.

**Can I change the season length in the middle of a season?**
Yes. The current season keeps its dates; the new length applies from the next one.

**Is there a way to remove a match?**
No. A match comes from the statistics files. To fix a wrong result, [edit the score of the map](matches.md#editing-the-score-of-a-map).

**How do I import the data of my old site?**
The data migration is a separate program, outside this repository, that uploads the old statistics files through the same path the plugins use. See [the roadmap](../roadmap.md#data-migration).

**Can I run two copies of the site behind a load balancer?**
No. The live match is kept in the memory of one process. Run one copy.

**Who sees the related accounts of a player?**
Everyone, by default, as on the old site. [Settings > Privacy](administration.md#privacy) can limit them to administrators. The game chat announcement is a separate setting of the plugin, `gameinfo_related_accounts_chat`.

## Troubleshooting

| What you see | What to do |
|---|---|
| Every page leads to the setup page | The setup is not finished. Sign in with the administrator account and finish it. See [First-run setup](first-run.md). |
| The setup code is refused | Copy it again: `playstats-hub setup-code`, or `docker compose logs hub \| grep "setup code"`. It works only until the first administrator signs in. |
| The game server stays **Waiting for the first connection** | Check the two lines in `cfg/playstats.cfg` and the `exec playstats.cfg` line in `cfg/server.cfg` (or the two lines in `cfg/server.cfg`), that `l4d2_playstats_sync.smx` is in `addons/sourcemod/plugins/` itself and loaded in `cfg/sharedplugins.cfg`, and that the game server can reach the address of the site. Run `sm_playstats_test` in the server console. |
| The game server shows **Invalid key** | The token in the game server's configuration is not the current one. Copy the full token again, or generate a new key. |
| Connected, but no statistics file arrives | Check **Statistics writing enabled** in the checklist (`sm_stats_writestats 1`), and that a map was played to the end. |
| Matches arrive but are all **Casual** | No game mode rule counts them. The overview warns about configurations with no rule. See [Game modes](game-modes-and-campaigns.md#game-modes). |
| A custom campaign makes one match per map | The campaign is not in the list, and unknown maps are joined only when they follow each other within 2 hours. [Add the campaign](game-modes-and-campaigns.md#campaigns), then reprocess. |
| The live server page is empty | The live server module must be on, `l4d2_gameinfo_sync` must be loaded, and a match must be in progress. After a restart of the site, wait for the next update from the game server. |
| `!ranking` opens an empty panel | The game cannot open the HTTPS address. Set `playstats_ingame_url` to a plain HTTP address of the site. See [Pages inside the game](in-game-pages.md#the-address-used-by-the-game). |
| Avatars do not load | They are downloaded in the background. Wait a few minutes, or set a Steam Web API key in [Settings](administration.md#site). |
| Players were signed out after moving the site | The `keys` folder was not copied. They only need to sign in again. |
| Skin pictures do not load | Check **Address of the assets** in the [shop settings](skin-shop.md#shop-settings). |
| A page shows **Page not found** | The address is wrong, or its module is off. See [Modules](administration.md#modules). |
| A page shows **Something went wrong** | The site hit an unexpected error. The details are in the log: `playstats-hub logs`, or `docker compose logs hub`. |
| Behind a reverse proxy, the site sees `http` instead of `https`, or the address of the proxy instead of the visitor | The proxy is not on the same machine, so the site ignores its forwarded headers. Add its address or network to `Hub__TrustedProxies`. See [Installing with Docker > Configuration](../install-docker.md#configuration). |
| `docker compose ps` shows the container as `unhealthy` | The site does not answer `/api/ping` on its own port. Read `docker compose logs hub`, and check that `ASPNETCORE_URLS` still has an `http` address. |
