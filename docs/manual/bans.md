# Bans

🌐 **English** | [Português](bans.pt.md) | [Español](bans.es.md)

The Bans module keeps the players banned from the game servers on the site. The bans given in the game with the usual SourceMod commands arrive by themselves, the administrators can also ban on the site, and the game servers ask the site about each player who connects. Everyone can see who is banned, why, and when the ban ends.

[Back to the manual](README.md)

## Contents

- [Turning it on](#turning-it-on)
- [For players](#for-players)
- [For administrators](#for-administrators)
- [Bans given in the game](#bans-given-in-the-game)
- [How the game servers apply the bans](#how-the-game-servers-apply-the-bans)
- [Related accounts and alerts](#related-accounts-and-alerts)
- [Importing banned_user.cfg](#importing-banned_usercfg)

## Turning it on

The module is off on a new site. Turn on **Bans** in **Administration → Modules**, and install the `l4d2_bans` plugin on each game server (see [Connecting a game server](../game-server.md)). Without the plugin the list exists on the site, but the servers neither send their bans nor check the players.

## For players

**More → Bans** opens `/bans`, the list of the players banned now: the player, the reason, the day of the ban, when it ends (or **Permanent**) and who banned. The search box finds a ban by name, Steam ID or reason. Bans that ended are not on this list.

The page of a banned player starts with a red warning: "*Name* is banned from the game servers until *date*", or "permanently", with the reason and a link to the list.

A banned player who tries to join a game server is kicked with the reason and the end of the ban, in the language of the game. There is no way to contest a ban on the site.

A signed-in player is told in the bell when banned and when the ban is lifted (see [Notifications](notifications.md)).

The IP addresses, the related accounts and the alerts are never shown to the public: only administrators see them.

## For administrators

**Administration → Bans** (`/admin/bans`) has three tabs:

| Tab | What it shows |
|---|---|
| Active | The bans in force, with the player, the reason, the day, the end, who banned (an administrator, **Server** for a ban given from the console or by a plugin, **Vote**, or **Imported**, with the command and the server below) and the **Attempts** of the player to join after the ban. |
| Ended | The bans that expired or were lifted, with who lifted them and why. Nothing is deleted: this is the history. |
| Alerts | Accounts that share an IP address with a banned account and were not banned. See [Related accounts and alerts](#related-accounts-and-alerts). |

The search box at the top filters the Active and Ended tabs by name, Steam ID or reason.

| Button | What it does |
|---|---|
| **Add** | Opens the **Ban a player** window: the player (a Steam ID, a profile link, or **Search for a player**), the duration and the reason. The duration is **Permanent**, 1 hour, 1 day, 3 days, 1 week, 2 weeks, 1 month, 3 months, 1 year, all counted from now, or **Until a date**, a day and time in the time zone of the site. A player who is already banned cannot be banned again: edit the ban instead. |
| **Edit** | Changes the end and the reason of an active ban. |
| **Unban** | Lifts the ban at once. The reason you write stays in the history and is shown only to administrators. |
| **Import** | Reads a `banned_user.cfg` file. See [Importing banned_user.cfg](#importing-banned_usercfg). |

Every change asks for confirmation. When the [live server module](live-server.md) is on, a ban, a change or an unban made on the site reaches the game servers within a few seconds: a banned player who is playing is kicked, and an unbanned player is removed from the ban list of the server. Without that module, the change applies the next time the player connects.

## Bans given in the game

The `l4d2_bans` plugin does not add commands. It listens to the bans of SourceMod, so everything that bans a player arrives on the site:

| In the game | On the site |
|---|---|
| `sm_ban <player> <minutes> [reason]`, or the ban of the admin menu | A ban of that duration (0 is permanent), with the administrator who gave it. |
| `sm_addban <minutes> <steamid> [reason]` | The same, for a player who is not on the server. |
| A ban vote (`sm_voteban`) | A ban shown as **Vote**. |
| A plugin that bans, such as `l4d2_mix` when a player abandons the mix | A ban shown as **Server**, with the command of the plugin. It is a normal ban, listed like the others. |
| `sm_unban <steamid>` | Lifts the active ban of the player. |

Banning a player who is already banned replaces the end and the reason of the active ban. Only bans by Steam ID are kept; a ban by IP address (`sm_banip`) is applied by the server but does not reach the site.

The server keeps applying its own bans as before, so a ban given while the site is down still works on that server. The plugin keeps the bans the site did not receive and sends them again every minute.

## How the game servers apply the bans

When a player connects, the plugin asks the site about that one player, once. The site answers from an index, so the size of the list does not matter, and the game server does not wait for the answer: the request runs in the background. When the player is banned, the plugin kicks them with the reason and also adds the ban to the server itself for the time that is left, so a player who reconnects is refused by the server without asking the site again. Each of these attempts is counted on the **Active** tab.

If the site cannot be reached, the player gets in, except for the bans the server holds itself: the ones given on that server, and the ones it learned from the site since it started.

## Related accounts and alerts

The site knows which Steam accounts connected from the same IP address (see [Player connections](moderation.md)).

When you ban a player on the site and that player has related accounts, a window lists them and asks if they should be banned too, with the same duration and reason. Uncheck an account to leave it out; accounts that are already banned are marked and cannot be chosen.

An alert is created, and the administrators are told in the bell, when:

- an account connects from the address of a banned account after the ban, or the banned player connects from a new address where other accounts played;
- a ban arrives from a game server, or is imported, and the player already shares an address with other accounts.

The **Alerts** tab lists the account, the banned account it is related to and the reason of that ban. **Ban** opens the **Ban a player** window filled in with the account, the reason "Related account of *name*" and the end of the original ban. **Ignore** closes the alert, and it is not raised again for that ban. An alert also leaves the list when the account is banned or when the original ban ends.

## Importing banned_user.cfg

**Import** takes the contents of the `cfg/banned_user.cfg` file of a game server. Each `banid` line becomes a ban. Comments written above it are read when they follow this layout:

```text
// Name: Player
// Ban date: 2026-08-24
// Expire in: never
// Ban reason: Cheating
banid 0 STEAM_0:1:12345678
banid 0 STEAM_1:1:12345678
```

The two lines of the same player become one ban. A blank line separates the players. Players who are already banned, and bans whose end is already past, are left as they are; lines that cannot be read are counted and left out. Imported bans are shown as **Imported**.
