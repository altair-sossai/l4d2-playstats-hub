# Championships

🌐 **English** | [Português](championships.pt.md) | [Español](championships.es.md)

The Championships module organizes championships played on the game servers of the community: the participants, the teams and how they are formed, the stages (round robin, with groups, and knockout), the campaigns of each game, the dates, the results and the statistics. It is off by default; turn it on in [Administration > Modules](administration.md#modules).

[Back to the manual](README.md)

## Contents

- [For players](#for-players)
  - [The championships page](#the-championships-page)
  - [The page of a championship](#the-page-of-a-championship)
  - [Asking to join](#asking-to-join)
  - [The page of a series](#the-page-of-a-series)
  - [The captain area](#the-captain-area)
- [For administrators](#for-administrators)
  - [Creating a championship](#creating-a-championship)
  - [Settings](#settings)
  - [Rules](#rules)
  - [Participants](#participants)
  - [Teams](#teams)
  - [Draft](#draft)
  - [Stages](#stages)
  - [Games and results](#games-and-results)
  - [History](#history)
- [How the rules work](#how-the-rules-work)

## For players

### The championships page

**Championships** in the menu shows each championship as a card with its banner, its logo, its status, its teams, its players and its start date: the ones in progress at the top, in larger cards, and the history of the finished and cancelled ones below, with the champion of each. A championship without a banner gets a colored background, and one without a logo shows its initials. A championship in draft is not listed: only the administrators see it.

### The page of a championship

Each championship has its own address, such as `/championships/cup-2027`. Every page of the championship starts with its banner, its logo, its name, its status and its dates, followed by its tabs.

The first tab, **Overview**, is the page of the championship itself, read from top to bottom:

1. **About**: the description and the key facts: the dates, the number of teams and of participants (with the places, when there is a limit), the size of each game, such as 4 vs 4, the team formation and the game mode.
2. **Registration**, when the championship takes requests to join: see [Asking to join](#asking-to-join).
3. **Where to watch**: buttons to the live stream and the videos, when the administration gave the links.
4. **Games**: the next game, with a countdown to its date and the buttons **Series page** and **Add to calendar**, the other scheduled games and the latest results. **All games** opens the Games tab.
5. **Teams**: each team with its captain and vice-captain. **Rosters** opens the Teams tab.
6. **Participants**: the photo and the name of each player of the championship, each one linked to the page of the player.
7. **Format**: each stage in order, with its type, groups, best of and campaign rule, whether it is finished, in progress, up next or later, and how many series were played.
8. **Podium**: the champion, the runner-up and the third place, when the championship is finished.
9. **Gallery**: the photos and art of the championship; a click opens each one in full size.

A section without anything to show is left out. The other tabs go deeper:

| Tab | What it shows |
|---|---|
| Rules | The rules written by the administration, in the language of the page when there is a translation. |
| Teams | Each team with its flag, captain, vice-captain and players, with the hours of Left 4 Dead 2 of each one. |
| Draft | The order of the captains and every pick, when the teams were formed by a draft, a roulette or pots. |
| Standings | The table of each round robin stage and group, with the teams that move on to the next stage marked in green. |
| Bracket | The bracket of each knockout stage, round by round, with the third place series when there is one. |
| Games | Every series by stage and round, with its date, its deadline and its result. |
| Statistics | The series and games won by each team, and the numbers of each player in the games linked to matches of the server: MVPs, SI damage, commons, skeets, levels, crowns, tank damage and deaths. |

A tab only appears when it has something to show.

### Asking to join

When the administration lets the players ask to join, the page of the championship has a **Registration** section, and the top of the page a button **Ask to join**, while the status is Participants and there are places left. Sign in with Steam, write a message to the administration if you want (such as your experience or when you can play) and press **Ask to join**. The section then says that your request is waiting; **Cancel the request** withdraws it, and you can ask again later.

The administration approves or refuses each request. Once it is approved you are a participant and the section says so; a refused request is shown as refused, and you cannot ask again in that championship. The section also shows how many places are left, and when there are none it says so.

### The page of a series

A series is the meeting of two teams: one game (best of 1) or more (best of 3, best of 5). Its page shows the score in games, the status, the date with the buttons **Add to Google Calendar** and **Download .ics**, the deadline, the vetoes and each game with its campaign, who chose it, the score and the link to the match of the server.

A game decided by a penalty shows the points taken off. In a knockout, a game that is not needed any more, such as the third game after a 2–0, appears as **Not needed**. A game being played on the server appears as **In progress**, with the score of the maps played so far.

### The captain area

The pages of a championship only show information. What a captain or a vice-captain does for the team is in a separate page, the **captain area**, at `/championships/<address>/captain`. Sign in with Steam: when you lead a team of a championship that is not a draft nor over, a tower icon appears next to your photo at the top of every page, with a red number when your team has something to do, and the pages of the championship show **Open the captain area**.

The captain area starts with the list of what your team has to do, each one a link to its tab, and has these tabs:

| Tab | What you do there |
|---|---|
| Team | **Change name, tag or flag** sends a new name, tag or flag (a square image, ideally 1024 × 1024 pixels, PNG, JPG, WEBP or GIF up to 2 MB) for approval. Until the administration approves it, the page says it is waiting and the other players keep seeing the current ones; **Withdraw the change** takes it back. When the administration refuses it, the reason appears here. **Remove the flag** takes the flag off at once. The table lists the players of the team, and the captain makes one of them the vice-captain or removes the vice-captain. |
| Draft | In a snake or linear draft, the order of the captains, the picks made and the players without a team. When it is the turn of your team, **Choose** puts the player in your team and the next team chooses. There is no time limit and nothing happens live: each captain chooses when they can, on the same day or the next. |
| Series | Each series of your team, with its date and status. **Dates** lists the proposals of both teams: propose a date (your proposal is already accepted by your team), and accept or refuse the ones of the other team. When both teams accept the same date, the series is scheduled. A confirmed date cannot be deleted by the teams. **Veto** (when the stage has vetoes) vetoes one campaign before the first game: it is not chosen nor drawn in the series, and cannot be changed. **Campaigns** (when the teams choose them) chooses the campaign of the games of your turn: the best seed the first game, the other team the second, and so on; the decider is drawn only when it is needed. |

The vice-captain can do everything the captain does, except choosing the vice-captain. Times are in the time zone of the site, shown on the Series tab. The results always come from the matches of the server: the captains do not type nor change scores.

## For administrators

**Administration > Championships** lists every championship, including the drafts, in a table with its status, teams, participants and champion. **Add** opens a window to create one; **Edit** opens the page of the championship and **Delete** deletes it with its teams, stages, games and history (the matches of the server are not touched).

The page of a championship shows its numbers at the top (participants, teams, stages built and series finished) and the tabs Settings, Rules, Participants, Teams, Draft (when the team formation is not manual), Stages, Games and History. Each tab starts with what it is for and the buttons of its actions; every change is made in a window, and the address keeps the tab, so a page reloaded after saving opens on the same tab.

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
- **Teams and players**: players per game, players per team (the most players a team can have, counting the substitutes), places (the most participants), the team formation (manual, roulette, pots, snake draft or linear draft) whether the teams can have substitutes and whether **players can ask to join** (see [Requests to join](#participants)).
- **Images**: the **banner**, a wide image such as 1920 × 600 shown at the top of every page of the championship and on its card; the **logo**, a square image such as 512 × 512; and the **gallery**, up to 12 photos and art shown at the end of the page. Each one is a PNG, JPG, WEBP or GIF file of up to 2 MB. Choose a file only to change it: saving without a file keeps the current image. **Remove the banner**, **Remove the logo** and **Remove** under each image of the gallery delete it when you save. A file that is not one of these images, a file over the limit or more than 12 images in the gallery is refused, and then nothing of the form is saved. The files are kept in the data folder of the hub, in `branding/championships`, so they go with the backups, and each change is written in the history.
- **Results**: whether the matches of the server are linked to the games by themselves, the **hours of tolerance** (how many hours before or after the date of the series the match can start, 6 by default) and **End after (minutes)** (how long a game in progress waits for new statistics before it ends, 60 by default). See [Games and results](#games-and-results).

### Rules

The rules in English, Portuguese and Spanish, one under the other, saved with the **Save** button at the bottom of the tab. They are written in Markdown: `#` titles, `-` lists, `**bold**`, `*italic*` and `[links](https://...)`. The page shows the text of the language of the visitor, or the English one.

### Participants

The administration adds the participants, or approves the requests of the players. With **Players can ask to join** on in the Settings tab, the panel **Requests to join** at the top of the Participants tab lists each request waiting for an answer, with the player, the message, the date and a badge with how many are waiting. **Approve** makes the player a participant, unless the championship is full, and **Refuse** answers no; a refused request stays in the list, so it can still be approved later. Each request, approval and refusal is written in the history.

**Add** opens a window where you paste Steam IDs, one per line, or **Choose from the list** in the player search window. The table, 20 players per page, shows the hours of Left 4 Dead 2 of each player, the team and the pot; **Edit** changes the pot, used by the pots draw, and **Remove** takes the player out of the championship and of the team.

### Teams

**Add** creates one team, and **Add several** creates Team A, Team B, Team C and so on. The table shows the seed (1 is the best; it orders the teams when a stage is built by seeds), the captain, the vice-captain and the players of each team. On each row:

- **Edit**: name, tag, seed, captain, vice-captain and flag. A captain or vice-captain without a team joins this team, which is how the captains of a draft are chosen. A name, tag or flag changed here is shown at once.
- **Review**, when the captain sent a new name, tag or flag: the window shows what the team has now next to what was proposed. **Approve** puts it on every page; **Refuse**, with an optional reason, discards it and shows the reason to the captain. Approve only names and images that are not offensive nor sensitive: nothing sent by a captain appears before it is approved, and the image waiting for approval is seen only by the administrators and the leaders of the team.
- **Players**: the players of the team, with **Remove**, and **Add a player** with a Steam ID or the player search window.
- **Delete**, while the team is in no stage.

The tab warns when teams are waiting for approval. A player can only be in one team, and a team cannot have more players than the limit.

### Draft

The buttons at the top follow the method chosen in the settings, and **Undo the last pick** and **Reset the draft** take the picks back. Every pick appears on the public Draft page.

| Method | How it works |
|---|---|
| Manual | No Draft tab: the administration puts each player in a team on the Teams tab. |
| Roulette | **Spin the roulette** draws a team among the ones with fewest players and a player without a team. |
| Pots | Set the pot of each participant on the Participants tab. **Draw the pots** gives each team one player of each pot; the player of the first pot becomes the captain of a team without one. |
| Snake draft | Give each team a captain on the Teams tab and **Draw the order of the captains**. Each captain then chooses in the [captain area](#the-captain-area) when it is the turn of the team, with no time limit. The team on the clock is highlighted here, and **Pick** chooses for it, for example when a captain asks in the chat. The order goes A, B, C, D and then D, C, B, A. |
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

**Matches linked by themselves.** The games are played on the servers of the community, so the statistics files arrive by themselves. When the option is on in the settings, each match that arrives is linked to the game that has:

- the same campaign as the match;
- at least half of the players of each team in the match, on opposite sides;
- a series with a date, and the match started within the hours of tolerance before or after it (a game can start late or early).

While the match is played the game is **In progress**: its button is yellow, the public pages show the score of the maps played, and the standings and the bracket do not change, so no team moves on before the game ends. The game ends, and the winner moves on, when:

- every map of the campaign is played;
- no statistics of the match arrive for the minutes set in the settings, for a game that ends early, such as on the third map;
- or an administrator clicks **End the game** in the window of the game.

A statistics file of the match that arrives later still updates the score. To undo a wrong link, choose **Not played yet** in the window of the game: that match is not linked to that game again. The administration can always link a match by hand or type a result.

The winner of a knockout series moves on to the next series by itself. A result that would change a winner who already started the next series is refused: clear the results of the next series first.

### History

Every change of the championship, with who made it and when, 20 per page.

## How the rules work

- **A series** ends when a team wins most of its games, such as two of three. In a round robin it also ends after all its games, and an equal number of wins is a draw.
- **A game** is won by the team with more points after its penalty. In a round robin a tie is a draw. In a knockout a tied game counts for nobody, and a new game is added to the series with a drawn campaign.
- **The standings** give the points of a win, a draw and a loss, minus the penalty points. The point difference is the points scored minus the points conceded minus the penalties of the games. When the points are equal, the tiebreakers are used in order, and then the seed.
- **The draws of campaigns** avoid the campaigns already chosen in the series and the vetoes, and only use campaigns with the minimum number of maps.
- **The games of a championship** are normal matches of the server: they count for the ranking like any other. The page of the match shows the championship and links to its series.
- **A game in progress** counts for nobody until it ends: the standings, the bracket and the next series wait for it.
