# Players

🌐 **English** | [Português](players.pt.md) | [Español](players.es.md)

Every player who played on your game servers has a page with their results, their statistics as survivor and as infected, their matches, the campaigns they play, their skins, and who they play with and against.

[Back to the manual](README.md)

## Contents

- [The players list](#the-players-list)
- [The player page](#the-player-page)
- [General tab](#general-tab)
- [Campaigns tab](#campaigns-tab)
- [Skins tab](#skins-tab)
- [Relations tab](#relations-tab)
- [Comparing two players](#comparing-two-players)
- [Related accounts](#related-accounts)
- [Punishments](#punishments)

## The players list

**More > Players** lists every player of the chosen period, ordered by one statistic.

![The players list](../images/en/players.webp)

- **Order by** chooses the statistic: experience, games, wins, losses, rage quits, MVP awards, deaths, damage, skeets, levels, crowns, friendly fire and the infected numbers.
- The column of the chosen statistic shows each player's total, and **Per game** shows the average per game. Hover it to see the number of games.
- The [period selector](seasons-and-history.md#the-period-selector) chooses a season, a year or all time.

The list has the same players as the ranking of the period, all of them and not only the top 100.

## The player page

Click a player anywhere on the site to open their page. It has a period selector, a **Compare** button, and up to four tabs.

![The player page](../images/en/player.webp)

## General tab

**General statistics**:

| Item | Meaning |
|---|---|
| Ranking | Position in the ranking of the period. |
| Steam ID, Profile URL | The account, with copy buttons and a link to the Steam profile. |
| Name, Previous names | The name used most often, and the other names used. |
| Patent, XP | The patent and experience in the season. See [Patents](patents.md). |
| Games, Wins, Loss, Win rate, Rage quit | As in the [ranking](ranking.md#the-ranking-page). |
| Punishment | Experience taken away by an administrator in this season. |
| Longest win streak, Longest loss streak | The most matches won or lost in a row. |
| Match history | A strip with the result of each match, oldest to newest. |

**Statistics as survivor** and **Statistics as infected** add up the numbers of every ranked match of the period: MVP awards, kills, damage, deaths, skeets, levels, crowns and so on. The ⓘ icons explain the less obvious ones.

**Last matches** lists the player's matches of the period with the same cards as the [match list](matches.md#the-match-list). The player's row is highlighted.

## Campaigns tab

![The campaigns tab](../images/en/player-campaigns.webp)

The games played in each campaign in the period, from the most played, with wins, draws, losses and rage quits. Draws count as games here, and a rage quit counts as a loss.

## Skins tab

![The skins tab](../images/en/player-skins.webp)

The weapon skins the player owns, the ones equipped, and a link to the [skin shop](skin-shop.md). Shown only when the skin shop module is on.

## Relations tab

![The relations tab](../images/en/player-relations.webp)

Two tables built from the ranked matches of the period:

- **Playing together**: for each teammate, the matches won and lost on the same team, and the rates.
- **Playing against**: for each opponent, the matches won and lost against them, and the rates.

**I don't get it, explain it to me** opens a short explanation of one row. The first 12 players are shown; **Show more players** shows the rest.

## Comparing two players

**Compare** chooses a second player. The page then shows both players side by side: the general statistics, the survivor and infected statistics, and the matches they played, with the second player's numbers next to the first one's.

![Two players compared](../images/en/player-compare.webp)

**Remove comparison** goes back to one player.

## Related accounts

When the live server module is on, the game servers report the address each player connects from. If other accounts connected from the same address, the player page shows a **Related accounts** panel with those accounts, their last login and how many connections they have. The address itself is never shown.

Administrators have a search over the same data in [Moderation > Player connections](moderation.md#player-connections). Connections older than three months are deleted.

## Punishments

An administrator sees an **Edit** link next to **Punishment**, when a season is selected. It asks how many experience points the player should lose, with 150 suggested.

- A number above zero adds a punishment of that many points to the season.
- `0` removes every punishment of the player in that season.

See [Ranking > Punishments](ranking.md#punishments) for how they count.
