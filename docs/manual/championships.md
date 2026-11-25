# Championships

🌐 **English** | [Português](championships.pt.md) | [Español](championships.es.md)

The Championships module organizes championships played on the game servers of the community: the participants, the teams and how they are formed, the stages (round robin, with groups, and knockout), the campaigns of each game, the dates, the results and the statistics. It is off by default; turn it on in [Administration > Modules](administration.md#modules).

[Back to the manual](README.md)

## Contents

- [For players](#for-players)
  - [The championships page](#the-championships-page)
  - [The page of a championship](#the-page-of-a-championship)
  - [The page of a series](#the-page-of-a-series)
  - [For captains and vice-captains](#for-captains-and-vice-captains)
- [For administrators](#for-administrators)
  - [Creating a championship](#creating-a-championship)
  - [Settings](#settings)
  - [Participants](#participants)
  - [Teams](#teams)
  - [Draft](#draft)
  - [Stages](#stages)
  - [Games and results](#games-and-results)
  - [History](#history)
- [How the rules work](#how-the-rules-work)

## For players

### The championships page

**Championships** in the menu lists the championships in progress at the top, as cards, and the history of the finished and cancelled ones below, with the champion and the runner-up of each. A championship in draft is not listed: only the administrators see it.

### The page of a championship

Each championship has its own address, such as `/championships/cup-2027`, and these tabs:

| Tab | What it shows |
|---|---|
| Overview | The description, the podium when it is finished, the next games, the latest results, the series of your team when you are its captain or vice-captain, and the format of each stage. |
| Rules | The rules written by the administration, in the language of the page when there is a translation. |
| Teams | Each team with its captain, vice-captain and players, with the hours of Left 4 Dead 2 of each one. |
| Draft | The order of the captains and every pick, when the teams were formed by a draft, a roulette or pots. |
| Standings | The table of each round robin stage and group, with the teams that move on to the next stage marked in green. |
| Bracket | The bracket of each knockout stage, round by round, with the third place series when there is one. |
| Games | Every series by stage and round, with its date, its deadline and its result. |
| Statistics | The series and games won by each team, and the numbers of each player in the games linked to matches of the server: MVPs, SI damage, commons, skeets, levels, crowns, tank damage and deaths. |

A tab only appears when it has something to show.

### The page of a series

A series is the meeting of two teams: one game (best of 1) or more (best of 3, best of 5). Its page shows the score in games, the status, the date with the buttons **Add to Google Calendar** and **Download .ics**, the deadline, the vetoes and each game with its campaign, who chose it, the score and the link to the match of the server.

A game decided by a penalty shows the points taken off. In a knockout, a game that is not needed any more, such as the third game after a 2–0, appears as **Not needed**.

### For captains and vice-captains

Sign in with Steam. When you are the captain or the vice-captain of one of the teams of a series in progress, its page shows the panel **Your team**:

- **Veto a campaign**: when the stage has vetoes, each team vetoes one campaign before the first game. A vetoed campaign is not chosen nor drawn in the series.
- **Campaign of game N**: when the teams choose the campaigns, your team chooses the games of its turn. The best seed chooses the first game, the other team the second, and so on. The last game of a best of 3 or 5 is the decider, drawn only when it is needed. Only the campaigns allowed by the rules are offered.
- **Dates**: propose a date to the other team. Your proposal is already accepted by your team; the captain or the vice-captain of the other team accepts or refuses it. When both teams accept the same date, the series is scheduled. A confirmed date cannot be deleted by the teams; to change it, propose another one and both accept it. You can delete your own proposals that are not confirmed.

Times are in the time zone of the site, shown next to the form.

## For administrators

**Administration > Championships** lists every championship, including the drafts, in a table with its status, teams, participants and champion. **Add** opens a window to create one; **Edit** opens the page of the championship and **Delete** deletes it with its teams, stages, games and history (the matches of the server are not touched).

The page of a championship shows its numbers at the top (participants, teams, stages built and series finished) and the tabs Settings, Participants, Teams, Draft (when the team formation is not manual), Stages, Games and History. Each tab starts with what it is for and the buttons of its actions; every change is made in a window, and the address keeps the tab, so a page reloaded after saving opens on the same tab.

### Creating a championship

In the **Add** window, type the name and choose the format. The format only creates the first stages; everything can be changed on the Stages tab until each stage is built.

| Format | Stages |
|---|---|
| Round robin | A league where every team plays every other team once, in best of 1, with one campaign drawn for each round. |
| Round robin + knockout | A league, and the four best play a knockout in best of 3, the teams choosing the campaigns. |
| Knockout | Only a knockout in best of 3, with the bracket drawn. |
| Groups + knockout | Two groups, and the two best of each group play a knockout in best of 3. |

A new championship is a draft, seen only by the administrators, and its page opens on the Settings tab.

### Settings

One form, saved with the **Save** button at the bottom:

- **Championship**: name, address, short description, start and end dates, game mode (such as ZoneMod v2.9.1b), and the links of the live stream and the videos.
- **Status**: Draft, Participants, Team formation, In progress, Finished or Cancelled, with the reason of the cancellation. Building the first stage puts the championship in progress. Mark it as finished after the final: the history then shows the champion.
- **Teams and players**: players per game, players per team (the most players a team can have, counting the substitutes), places (the most participants), the team formation (manual, roulette, pots, snake draft or linear draft) and whether the teams can have substitutes.
- **Rules**, in English, Portuguese and Spanish, in Markdown: `#` titles, `-` lists, `**bold**`, `*italic*` and `[links](https://...)`. The page shows the text of the language of the visitor, or the English one.

### Participants

Only the administration adds participants. **Add** opens a window where you paste Steam IDs, one per line, or **Choose from the list** in the player search window. The table, 20 players per page, shows the hours of Left 4 Dead 2 of each player, the team and the pot; **Edit** changes the pot, used by the pots draw, and **Remove** takes the player out of the championship and of the team.

### Teams

**Add** creates one team, and **Add several** creates Team A, Team B, Team C and so on. The table shows the seed (1 is the best; it orders the teams when a stage is built by seeds), the captain, the vice-captain and the players of each team. On each row:

- **Edit**: name, tag, seed, captain and vice-captain. A captain or vice-captain without a team joins this team, which is how the captains of a draft are chosen.
- **Players**: the players of the team, with **Remove**, and **Add a player** with a Steam ID or the player search window.
- **Delete**, while the team is in no stage.

A player can only be in one team, and a team cannot have more players than the limit.

### Draft

The buttons at the top follow the method chosen in the settings, and **Undo the last pick** and **Reset the draft** take the picks back. Every pick appears on the public Draft page.

| Method | How it works |
|---|---|
| Manual | No Draft tab: the administration puts each player in a team on the Teams tab. |
| Roulette | **Spin the roulette** draws a team among the ones with fewest players and a player without a team. |
| Pots | Set the pot of each participant on the Participants tab. **Draw the pots** gives each team one player of each pot; the player of the first pot becomes the captain of a team without one. |
| Snake draft | Give each team a captain on the Teams tab and **Draw the order of the captains**. The team on the clock is highlighted; click **Pick** on the player it chose among the ones without a team. The order goes A, B, C, D and then D, C, B, A. |
| Linear draft | The same, always in the order A, B, C, D. |

### Stages

The stages are played in order. **Add** adds one at the end, with a name and a type. The table shows the format, the campaign rule and whether each stage is built, and its rows have:

- **Edit**: the window with every field of the stage, below.
- **Build**: a window with the teams that enter, in their order, and the button that creates the groups and rounds, or the bracket. The first stage takes every team; the next ones take the teams that move on from the previous stage, which must be built and finished.
- **Penalties** (built round robin stages): points taken off the standings, with a reason.
- **Reset** (built stages): deletes the series and results of the stage to start again, as long as the next stage was not built.
- **Delete** (stages not built).

| Field | Meaning |
|---|---|
| Type | Round robin or knockout. A knockout stage must be the last one. |
| Order of the teams | By the seeds of the teams, by draw, or by the standings of the previous stage. |
| Best of | The number of games of each series. A knockout has its own **Final best of**. |
| Groups and Legs | Round robin: how many groups, and whether every team plays every other once or twice. |
| Move on per group | How many teams of each group enter the next stage. 0 for all. Only when there is a next stage. |
| Win, Draw, Loss | The points of a series in the standings. |
| Tiebreakers | Up to three, in order: point difference, points scored, head-to-head, wins. |
| Third place series | Knockout: the losers of the semifinals play for the third place. |
| Campaigns | One campaign drawn for each round, each team chooses one with the decider drawn, or the administration chooses. |
| Minimum maps | Only campaigns with at least this many maps can be chosen or drawn. |
| Vetoes and once per team | Each team vetoes one campaign per series, and a team chooses each campaign only once in the championship. |

In a knockout with a number of teams that is not a power of two, the best seeds get a bye to the next round. With six teams, for example, the first and the second wait in the semifinals, the third plays the sixth and the fourth plays the fifth. After a stage is built, its type, games, groups and campaign rule cannot change; the points, tiebreakers, minimum maps, vetoes and the once-per-team rule still can.

### Games and results

The Games tab lists every series of the built stages, 20 per page, and can be filtered by stage. At the top:

- **Deadlines**: the date by which every series of a round should be played, shown on the pages of the games. Leave it empty to remove it.
- **Campaigns**: draw again, or clear, the campaigns of the games not played yet of a stage.

Each series shows its round, the teams, the result, the date and one numbered button per game: green when played, faded when not needed. A game button opens the game window:

- **Campaign**, chosen by the administration at any time. In a stage where the teams choose, a choice of the administration counts as the choice of the team of that turn.
- **Result**: not played yet; **Match of the server**, among the matches of the last 60 days with players of both teams, the newest first, which gives the score and finds which side each team played on (a match can only be linked to one game, and the statistics of the championship come from the linked matches); or **Type the result**, for a game that has no match on the server.
- **Penalties** of each team and notes: points taken off the score of a team in the game. They can change the winner.

**Edit** on a series opens its window: the date, the deadline, the notes, the vetoes of each team (when the stage has vetoes), the walkover (the team that wins without playing; the games not played are cancelled) and the date proposals of the teams, which can be deleted. **See** opens the public page of the series.

The winner of a knockout series moves on to the next series by itself. A result that would change a winner who already started the next series is refused: clear the results of the next series first.

### History

Every change of the championship, with who made it and when, 20 per page.

## How the rules work

- **A series** ends when a team wins most of its games, such as two of three. In a round robin it also ends after all its games, and an equal number of wins is a draw.
- **A game** is won by the team with more points after its penalty. In a round robin a tie is a draw. In a knockout a tied game counts for nobody, and a new game is added to the series with a drawn campaign.
- **The standings** give the points of a win, a draw and a loss, minus the penalty points. The point difference is the points scored minus the points conceded minus the penalties of the games. When the points are equal, the tiebreakers are used in order, and then the seed.
- **The draws of campaigns** avoid the campaigns already chosen in the series and the vetoes, and only use campaigns with the minimum number of maps.
- **The games of a championship** are normal matches of the server: they count for the ranking like any other. The page of the match shows the championship and links to its series.
