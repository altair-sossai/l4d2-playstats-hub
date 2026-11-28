# Modules

🌐 **English** | [Português](modules.pt.md) | [Español](modules.es.md)

What each module of L4D2 PlayStats Hub is for, what it does, who uses it and what it needs. A module is a feature the administrator turns on or off; a few parts are always on. This page is the summary: each section links to the page of the [user manual](manual/README.md) with every detail and every setting.

## Contents

- [The modules at a glance](#the-modules-at-a-glance)
- [Turning a module on or off](#turning-a-module-on-or-off)
- [Ranking and statistics](#ranking-and-statistics)
- [Live server](#live-server)
- [Site chat](#site-chat)
- [Remote commands](#remote-commands)
- [Weapon skin shop](#weapon-skin-shop)
- [Community ranking](#community-ranking)
- [Championships](#championships)
- [Bans](#bans)
- [Anti-Cheat](#anti-cheat)
- [Always on](#always-on)
  - [Game servers and statistics upload](#game-servers-and-statistics-upload)
  - [Notifications](#notifications)
  - [Commands page](#commands-page)
  - [Forced names](#forced-names)
  - [Administration](#administration)
  - [Backups](#backups)
  - [Languages, themes and phones](#languages-themes-and-phones)

## The modules at a glance

| Module | What it is for | Default | Needs | Plugin of the game server |
|---|---|---|---|---|
| [Ranking and statistics](#ranking-and-statistics) | The ranking, the matches, the players and the seasons. | On | | `l4d2_ranking` and `l4d2_show_patent_icon` (optional) |
| [Live server](#live-server) | The match in progress on each game server. | On | | `l4d2_gameinfo_sync` |
| [Site chat](#site-chat) | Writing in the game chat from the site. | Off | Live server | `l4d2_gameinfo_sync` |
| [Remote commands](#remote-commands) | Sending commands from the site to a game server. | Off | Live server | `l4d2_gameinfo_sync` |
| [Weapon skin shop](#weapon-skin-shop) | Spending the stars of the MVPs on weapon skins. | Off | Ranking and statistics | `l4d2_weapon_skins` |
| [Community ranking](#community-ranking) | A second ranking, voted by the players. | Off | | None |
| [Championships](#championships) | Championships with teams, draft, stages and results. | Off | | None besides the statistics |
| [Bans](#bans) | The players banned from the game servers. | Off | | `l4d2_bans` |
| [Anti-Cheat](#anti-cheat) | The Anti-Cheat page and who must use it. | Off | | The anti-cheat plugin, a separate project |

Every game server also needs `l4d2_playstats_sync`, which sends the statistics files. The six plugins of this repository are described in [the plugins guide](../plugins/README.md).

## Turning a module on or off

An administrator chooses the modules in the third step of the [first-run setup](manual/first-run.md#step-3-the-modules), and later in **Administration > Modules** (`/admin/modules`): tick the module and click **Save**. The **?** next to each name explains what changes.

![The modules in the administration area](images/en/admin-modules.webp)

- A module that is off disappears from the menus, and its pages and its API answer as not found.
- Turning a module off deletes nothing. Its data comes back when it is turned on again.
- A module whose requirement is off behaves as off, even when its own box is ticked.

See [Administration > Modules](manual/administration.md#modules).

## Ranking and statistics

| | |
|---|---|
| What it is for | Turns every match played on your servers into a ranking, a history and a page for each player. |
| Default | On |
| Needs | `l4d2_playstats_sync` on the game servers. Optional: `l4d2_ranking` for `!ranking` inside the game, and `l4d2_show_patent_icon` for the patent icons. |
| Manual | [Ranking](manual/ranking.md), [Seasons and history](manual/seasons-and-history.md), [Patents](manual/patents.md), [Matches](manual/matches.md), [Players](manual/players.md), [Game modes and campaigns](manual/game-modes-and-campaigns.md), [Pages inside the game](manual/in-game-pages.md), [Moderation > Punishments](manual/moderation.md#punishments) |

The ranking of the current season opens the site: position, patent, experience, wins and losses, MVPs, tank wipes, win rate and rage quits. A selector opens the other seasons, a whole year or all time.

![The ranking of the current season](images/en/ranking.webp)

- **Visitors and players**
  - **Matches**: every match, with the score of each map, the players and the statistics of each team and map.
  - **Players**: the list of players, the page of each one (results, campaigns, skins, who they play with and against) and the comparison of two players.
  - **Patents**: 15 levels, from the experience of the season.
  - **Inside the game**: `!ranking` opens the ranking in the game's own browser.
- **Administrators**
  - Decide which game modes count, which maps form a campaign, how many points each result gives and how long a season lasts.
  - Punish a player in a season, edit the score of a map, replace a player in a match and rebuild every match from the stored files.

![A match, with the teams and the result of each map](images/en/match.webp)

![The page of a player](images/en/player.webp)

With the module off, the first page shows a short welcome text and the matches are still built, out of sight.

## Live server

| | |
|---|---|
| What it is for | Follows the match in progress on each game server, from the browser. |
| Default | On |
| Needs | `l4d2_gameinfo_sync` on the game servers. |
| Manual | [Live server](manual/live-server.md), [Moderation > Player connections](manual/moderation.md#player-connections) |

**Server** in the menu opens the list of game servers, or the live page when there is only one.

![The live page of a server during a match](images/en/server-live.webp)

- **Visitors and players**
  - The scoreboard, the bonus, where the Tank and the Witch spawn, the survivors with health and weapons, the infected with class and damage, the spectators.
  - The events of the match (skeets, crowns, Tank, pauses) and the chat of the game.
  - The address of the server with a copy button, and the number of players online next to the site name on every page.
- **Administrators**
  - Set the delay of the live page (15 seconds by default) and how often it refreshes.
  - **Player connections**: the accounts that connected from the same address as a player. The address itself is never shown.

## Site chat

| | |
|---|---|
| What it is for | Lets signed-in players write in the chat of a game server from the live page. |
| Default | Off |
| Needs | The [Live server](#live-server) module and `l4d2_gameinfo_sync`. |
| Manual | [Live server > Writing in the chat from the site](manual/live-server.md#writing-in-the-chat-from-the-site), [Moderation > Chat bans](manual/moderation.md#chat-bans) |

- **Players** signed in with Steam write in the box under the chat of the live page. The message appears in the game.
- **Administrators** ban a player from the site chat and set the pauses between messages and their size. A chat ban does not mute the player inside the game, and is not a ban from the game server: that is the [Bans](#bans) module.

![The chat bans](images/en/admin-chat-bans.webp)

## Remote commands

| | |
|---|---|
| What it is for | Sends SourceMod commands from the site to a game server, without RCON. |
| Default | Off |
| Needs | The [Live server](#live-server) module and `l4d2_gameinfo_sync`. |
| Manual | [Moderation > Remote commands](manual/moderation.md#remote-commands) |

Only **administrators** use it:

- Type a command, such as `sm_forcestart`, or build it step by step with the terminal button. The game server picks it up on its next check.
- Send a command from the chat box of the live page.
- Read the audit log: every command, who sent it, when, and whether it was delivered.

![The remote commands and the audit log](images/en/admin-remote-commands.webp)

## Weapon skin shop

| | |
|---|---|
| What it is for | Rewards the players: each MVP of a ranked match gives a star, and the stars buy weapon skins. |
| Default | Off |
| Needs | The [Ranking and statistics](#ranking-and-statistics) module, `l4d2_weapon_skins`, and the skin models and materials on a FastDL (see [Connecting a game server](game-server.md#optional-weapon-skins-and-patent-icons)). |
| Manual | [Weapon skin shop](manual/skin-shop.md) |

![The weapon skin shop](images/en/skins.webp)

- **Visitors** see the catalog, by weapon and rarity, with photos of each skin.
- **Players** buy skins with their stars, equip them on the site or with `!skin` in the game, and see their balance in the menu bar. Skins bought in a season are refunded when it ends.
- **Administrators** give skins as gifts, refund purchases, change prices, import the catalog and turn the refund of the season off. The catalog comes with the site and is imported the first time the module is turned on.

![The skins a player owns](images/en/skins-mine.webp)

## Community ranking

| | |
|---|---|
| What it is for | A second ranking, built from what the players think of each other. |
| Default | Off |
| Needs | Nothing else. The votes use the players who played on your servers. |
| Manual | [Community ranking](manual/community-ranking.md) |

![The community ranking](images/en/community.webp)

- **Visitors** see the ranking and the latest votes.
- **Players** with enough matches choose the better of two players: left, right, same level or "I don't know". There is a limit of votes per day, and each pair is asked only once.
- **Administrators** set the limits and the minimums, block a voter and delete votes.
- **Game servers** can ask for two balanced teams made from this ranking, for a mix (see [Connecting a game server](game-server.md#the-community-ranking-mix)).

![Voting on a pair of players](images/en/community-vote.webp)

## Championships

| | |
|---|---|
| What it is for | Organizes championships on your game servers, from the registration to the final. |
| Default | Off |
| Needs | Nothing else. The results come from the statistics files, so the game servers need only `l4d2_playstats_sync`. The [Anti-Cheat](#anti-cheat) and the [Ranking](#ranking-and-statistics) are optional. |
| Manual | [Championships](manual/championships.md) |

**Championships** in the menu lists every championship: the ones in progress at the top, and the finished ones with their champion.

![The championships page](images/en/championships.webp)

Each championship has its own pages, with its banner and logo: an overview with the next game, the teams and the format, and the tabs Rules, Teams, Draft, Standings, Bracket, Games and Statistics.

![The page of a championship](images/en/championship.webp)

- **Visitors** follow everything: the standings, the bracket, the dates (with a calendar link) and the results.
- **Players** ask to join, when the administration allows it, with a message. The administration approves or refuses each request.
- **Captains and vice-captains** have the **captain area**: they pick players in the draft, propose a name, tag and flag for the team, propose and accept dates, veto campaigns and choose the campaigns of their games.
- **Administrators** create the championship from a format, choose the participants and how the teams are formed, build the stages and decide everything the captains can do too. They also choose which campaigns can be played, whether the Anti-Cheat is mandatory, and a suggested day and time for the games.

| Part | Options |
|---|---|
| Formats | Round robin, knockout, round robin followed by knockout, groups followed by knockout. Every stage can be changed until it is built. |
| Team formation | By hand, roulette, pots, snake draft or linear draft. |
| Campaigns | Drawn for each round, chosen by the teams, or chosen by the administration, with vetoes and a list of allowed campaigns. |
| Results | Linked by themselves to the match of the server, linked by hand, typed, or walkover, with penalties. |

![The bracket of the knockout stage](images/en/championship-bracket.webp)

![The captain area, with what the team has to do](images/en/championship-captain.webp)

![The games of a championship in the administration area](images/en/admin-championship-games.webp)

The games are normal matches of the server: they count for the ranking like any other.

## Bans

| | |
|---|---|
| What it is for | Keeps the bans of every game server in one list, public, with the reason and the end of each one. |
| Default | Off |
| Needs | `l4d2_bans` on the game servers. With the [Live server](#live-server) module, a ban made on the site reaches the servers at once; without it, the next time the player connects. |
| Manual | [Bans](manual/bans.md) |

![The public list of bans](images/en/bans.webp)

- **Visitors** see in **More > Bans** who is banned now, why, until when and by whom. The page of a banned player starts with a warning. IP addresses and related accounts are never public.
- **Players** who are banned cannot join any of the game servers, and are told in the bell.
- **Administrators** ban with a duration and a reason, edit, unban, import a `banned_user.cfg` file and read the history of the bans that ended. Bans given in the game with `sm_ban`, `sm_addban`, a vote or another plugin arrive by themselves, and `sm_unban` lifts them.
- **Alerts** warn the administrators when an account connects from the same IP address as a banned account.

![The warning on the page of a banned player](images/en/player-ban.webp)

![The bans in the administration area](images/en/admin-bans.webp)

![The alerts of related accounts](images/en/admin-bans-alerts.webp)

## Anti-Cheat

| | |
|---|---|
| What it is for | Offers the Anti-Cheat program to the players and decides who must use it on the game servers. |
| Default | Off |
| Needs | The Anti-Cheat program and the anti-cheat plugin of the game server, which are separate projects and not part of this repository (see [Connecting a game server](game-server.md#optional-the-anti-cheat-plugin)). |
| Manual | [Anti-Cheat](manual/anti-cheat.md) |

![The Anti-Cheat page](images/en/anti-cheat.webp)

- **Visitors** find the download, how to check the file, how to use it and the answers to common problems.
- **Players** signed in read whether their account must have the Anti-Cheat running.
- **Administrators** choose the mode (everyone except a whitelist, or only a list of players) and keep both lists.

![The Anti-Cheat settings](images/en/admin-anti-cheat.webp)

## Always on

These parts have no switch: they work whatever modules are on.

### Game servers and statistics upload

Each game server is registered in **Administration > Game servers** and receives a token. The plugins send the statistics files with it, and the site shows whether each server is connected and what is missing. Several servers feed the same site, and the results of a player add up across all of them.

![The list of game servers](images/en/admin-servers.webp)

Manual: [Administration > Game servers](manual/administration.md#game-servers), [Connecting a game server](game-server.md).

### Notifications

A bell in the top bar tells each signed-in player what concerns them: a championship they can join, a request that was answered, their turn in the draft, a series that was scheduled, a punishment, a ban, a gift. The administrators are told what waits for them, such as a request to join, a new team flag or a ban alert. The unread ones are counted on the bell, and `/notifications` lists them all. They are kept for 90 days. Which ones arrive depends on the modules that are on.

![The bell, with the latest notifications](images/en/notifications-bell.webp)

![The notifications page](images/en/notifications.webp)

Manual: [Notifications](manual/notifications.md).

### Commands page

**More > Commands** teaches every command of the game servers, with a search and examples. A command that needs a module that is off is hidden.

![The commands page](images/en/commands.webp)

Manual: [Game server commands](manual/commands.md).

### Forced names

Administrators give a player a fixed name, used on every page of the site and, with the `l4d2_forced_names` plugin, in the game.

![The forced names](images/en/admin-forced-names.webp)

Manual: [Moderation > Forced names](manual/moderation.md#forced-names).

### Administration

**Admin** in the menu opens the administration area: the overview, the settings, the modules, the administrators and one page for each feature. Sign-in is with Steam, and every change asks for confirmation.

![The overview of the administration area](images/en/admin-overview.webp)

Manual: [Administration](manual/administration.md), [First-run setup](manual/first-run.md).

### Backups

The site uploads a zip of its data to an Azure Storage container every few hours, checks that it restores and tells the administrators when a backup fails. It is off until a destination is set.

![The backup page](images/en/admin-backup.webp)

Manual: [Backups and updates](manual/backups-and-updates.md).

### Languages, themes and phones

Every page is in English, Portuguese and Spanish, in a dark and a light theme, and fits a phone.

![The ranking on a phone](images/en/phone-ranking.webp)

Manual: [Languages, themes and phones](manual/languages-and-themes.md).
