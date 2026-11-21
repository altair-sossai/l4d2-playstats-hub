# Community ranking

🌐 **English** | [Português](community-ranking.pt.md) | [Español](community-ranking.es.md)

The community ranking is a second ranking, built from what the players think of each other. The site shows a voter two players and asks which one is better. Each answer moves the two players up or down. It does not reset with the season.

[Back to the manual](README.md)

## Contents

- [The community ranking page](#the-community-ranking-page)
- [Voting](#voting)
- [Who can vote, and how much](#who-can-vote-and-how-much)
- [How the pair is chosen](#how-the-pair-is-chosen)
- [How the rating is calculated](#how-the-rating-is-calculated)
- [For administrators](#for-administrators)

## The community ranking page

**Community** in the menu opens the list.

![The community ranking](../images/en/community.webp)

| Column | Meaning |
|---|---|
| # | Position. The first three have a medal. |
| Patent | The player's patent in the current season of the statistics ranking. |
| Player | Avatar and name. |
| Rating | The rating built from the votes. Everybody starts at 1500. |
| Comparisons | How many times the player was judged against another player. |
| Wins, Draws, Losses | How many comparisons the player won, drew and lost. |

**See the latest votes**, above and below the table, opens **Latest votes**: one card per vote with who voted, the two players side by side, the chosen one in green (or both in yellow for the same level) and when. "I don't know" answers are not listed. Clicking a player, or the arrows icon next to a name in the table, shows only the comparisons of that player. The filters **Votes made by** and **Votes about** take a name, or the search button next to each opens a window with the players of the voting, the most recent first. An administrator can hide who voted.

On a phone the table shows only the position, the player and the rating. The **?** next to **Rating** opens a window that explains every column.

Only players with enough comparisons are listed: 5 by default. A player must also have played the minimum number of matches to be compared, 1 by default. Both minimums are written under the table, and the list stops at the first 100 players by default. When more players are listed, the table shows 100 per page, with page links under it. **How does it work?**, in the box, opens a window that explains in four steps how the votes become the list, with the numbers of the current settings. The voting page has the same link, also when the votes of the day are used up. Clicking a player's row opens a window with their rating, comparisons and a bar of wins, draws and losses, with links to their comparisons and profile. **Vote now**, in the box above the table, opens the voting page. The same box appears above the statistics ranking on the home page; a player can close it there, and it comes back after 7 days.

## Voting

![The voting page](../images/en/community-vote.webp)

The page shows two players with their avatars. Answer with one of:

| Answer | What it does |
|---|---|
| This one is better (left or right), or a click on the photo | The chosen player wins the comparison, the other loses. |
| They are at the same level | A draw. |
| I don't know one of them | Nothing changes, and this pair is not shown to you again. Both players can still come up in other pairs. |

Under each name the card shows up to 4 other names the player has used in matches, the most used first, so you recognize someone who changed names. After each answer the page marks your choice and the next pair slides in. On a computer the ← and → keys pick the left or the right player, ↓ says they are at the same level and ↑ that you don't know one of them. **Votes left today** shows how many votes you still have.

## Who can vote, and how much

| Rule | Default |
|---|---|
| You must be signed in with Steam. | |
| You must have played a minimum number of matches, counted over all seasons, ranked or not. | 10 |
| You have a number of votes per day. A day follows the time zone of the site. Only answers that judge (better or same level) use a vote; "I don't know" does not. | 20 |
| A player is offered for comparison only after a minimum number of matches. | 1 |
| A player can be offered only if their last match was within a number of days. With 0, however long ago it was. | 0 |
| You are never asked about yourself, and never asked about the same pair twice. | |

When you cannot vote, the page says why: you need to sign in, you need more matches (with your count), you used all the votes of the day, there is nobody left for you to compare, or the administrators blocked your votes.

## How the pair is chosen

The site chooses the pair; a voter cannot choose who to judge, and an answer is accepted only for the pair the site offered.

1. It prefers players you have played with or against, since you know them best. Only when there is no such pair left does it offer other players.
2. Among those, it starts from one of the players whose rating is least certain (usually the ones with the fewest comparisons), so that new players get rated.
3. It pairs that player with the opponent whose answer teaches the most: someone with a close rating, preferring a rating that is also uncertain. So the best players are compared with each other, not with much weaker ones.
4. Which one appears on the left is random.

## How the rating is calculated

The rating uses the Bradley–Terry model, the statistical model made for comparisons in pairs.

- After each judging answer, the site calculates every rating again from all the votes together. The order of the votes does not matter, and a vote counts the same whether it came first or last.
- Being chosen over a player with a high rating is worth more than being chosen over one with a low rating, because the model weighs each answer by who the other player is. "They are at the same level" counts as half a win for each.
- A difference of 400 points means a chance of about 10 to 1 of being judged better. The scale is centered on the initial rating, 1500 by default.
- Each rating has a margin of uncertainty (±), shown next to it. It starts large and shrinks as the player is compared with more players. A player with very few votes is held near the middle of the scale until the votes say otherwise.
- The position in the ranking uses the rating minus twice the margin. A player with two lucky votes does not go above a player who won many comparisons.
- "I don't know" answers change nothing.

The rating is permanent. It does not start again with the season.

## For administrators

**Administration > Community ranking** has the totals of votes and rated players on top and four tabs: **Settings**, **Voters**, **Blocked voters** and **Maintenance**, which holds **Rebuild the ratings** and **Reset the community ranking**. The address keeps the open tab and page, so a reload or a link opens the same place.

![The community ranking settings](../images/en/admin-community.webp)

Each field has a **?** next to its name: hovering it shows a short explanation, and the fields that need more show a window with the details when clicked.

| Setting | What it does | Default |
|---|---|---|
| Minimum matches to vote | Matches a player must have played, over all seasons, to vote. | 10 |
| Votes per day | Judging answers each voter can give per day. At least 1. | 20 |
| Minimum matches to be compared and listed | Matches a player must have played, over all seasons, to be offered for comparison and to appear in the community ranking. | 1 |
| Maximum days since the last match to be compared | A player whose last match is older than this is no longer offered for comparison, but stays in the community ranking. 0 means no limit. | 0 |
| Minimum comparisons to be listed | Comparisons a player needs to appear in the community ranking. | 5 |
| Show who voted in the latest votes | Shows the name and the avatar of each voter on the Latest votes page. Off, each vote shows only "A player voted". | On |
| Maximum players in the ranking | The community ranking shows at most this many players, from the highest rating down. 0 shows everyone. | 100 |
| Initial rating | The middle of the scale: the rating of a player who was never compared. | 1500 |

The ratings are calculated again from every vote after each new vote. **Rebuild the ratings** does it right away, for example after changing the initial rating.

**Reset the community ranking**, in a red panel, deletes every vote and every rating, so the community ranking starts again from zero; the settings are kept. A window shows how many votes and players will be deleted and asks for confirmation, since it cannot be undone.

**Voters** lists everybody who voted, with their number of votes and their last vote, 20 per page, the most votes first; **See votes** opens, in a new tab, the latest votes of that voter. **Remove votes** deletes all the votes of one voter and rebuilds the ratings without them, for example when someone voted in bad faith. The field takes the voter in any Steam ID format or as a profile link, and the search button next to it opens the [player search](administration.md#the-player-search).

**Block** stops a voter from voting, from the same field or from the voter's row. It opens a window with two choices: **Only block** keeps the votes already given, and **Block and delete the votes** also deletes them and calculates the ratings again without them. The voting page of a blocked voter then says they can no longer vote because the administrators reviewed the votes and blocked theirs, and any answer they send is refused. **Blocked voters** lists everybody who is blocked, since when, 20 per page, with **Unblock** to let them vote again; the votes deleted when blocking do not come back. **Add**, on that tab, opens the window **Block a player**: it blocks anyone chosen in the field or with the player search, also someone who has not voted yet, and **Also delete the votes they already gave** deletes their votes too. **Save** blocks the player.

To delete a single vote, open **Latest votes** or the **Votes received** and **Votes cast** tabs of a player page while signed in as an administrator. Each vote then has a red button that deletes it, and the ratings are calculated again without it, and a yellow button that blocks the player who gave it. Administrators always see who voted, even when **Show who voted in the latest votes** is off.

Turning off the community ranking module in [Modules](administration.md#modules) removes **Community** from the menu. The votes are kept.
