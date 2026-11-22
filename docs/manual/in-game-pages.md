# Pages inside the game

🌐 **English** | [Português](in-game-pages.pt.md) | [Español](in-game-pages.es.md)

Players can see the ranking without leaving Left 4 Dead 2. The `!ranking` chat command opens a light version of the site in the game's own panel. With the `l4d2_ranking` plugin it also opens by itself for players who join before a match starts.

[Back to the manual](README.md)

## Contents

- [The four pages](#the-four-pages)
- [Why they look different](#why-they-look-different)
- [The address used by the game](#the-address-used-by-the-game)

## The four pages

The panel has four tabs. The first three show the current season only, and only ranked matches. The bar at the top names that season as the site does, such as **From 11/2026 to 12/2026**.

**Ranking**: the top 50 players, with patent, experience, wins, losses, win rate, MVPs and tank wipes. The row of the player who opened the panel is highlighted.

![The in-game ranking](../images/en/ingame-ranking.webp)

**My summary**: the patent of the player who opened the panel, the experience and the progress towards the next patent, wins, losses, win rate, best win streak, MVPs, tank wipes and rage quits, and the averages per match.

![The summary of the player who opened the panel](../images/en/ingame-player.webp)

A player with no ranked match in the season sees a short message instead.

**Matches**: the last 10 ranked matches of the season, with the date and time, the campaign, the players of each team and the score. The winning score is green.

![The last matches, in the game](../images/en/ingame-matches.webp)

**Commands**: the commands players can use on the server, from the same catalog as the [commands page](commands.md), without the administrator commands. A row of links at the top jumps to each section. Players also open it directly with `!comandos` or `!commands`.

![The commands, in the game](../images/en/ingame-commands.webp)

The pages are at `/ingame`, `/ingame/player`, `/ingame/matches` and `/ingame/commands`. The game adds the SteamID64 of the player to the address, so the site knows who opened the panel. They can be opened in a normal browser too, for example `/ingame?steamid=76561198000000000`.

## Why they look different

The panel of the game is a browser from 2012, 900 × 450 pixels. It runs old JavaScript only and lacks most of today's CSS. These pages are made for it: no script, a fixed width, and only what fits on one screen. [In-game browser](../in-game-browser.md) records what that browser supports.

They are read-only and need the ranking module.

## The address used by the game

The game's browser is not expected to open current HTTPS sites. Give the game a plain HTTP address of the site, such as `http://203.0.113.10:5080`, in the `playstats_ingame_url` setting of the game server. When it is empty, the plugin uses `playstats_url`.

- [Installing on a Linux VPS](../install-linux.md#the-address-used-inside-the-game) explains why the port stays open when you use a domain with HTTPS.
- [Connecting a game server](../game-server.md) shows where the setting goes.
