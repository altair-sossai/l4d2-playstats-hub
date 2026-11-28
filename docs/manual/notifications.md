# Notifications

🌐 **English** | [Português](notifications.pt.md) | [Español](notifications.es.md)

The site tells each signed-in player what happened that concerns them: a championship they can join, a request to join that was answered, their turn in the draft, a series that was scheduled, a punishment, a gift. The administrators are also told what needs their attention, like a new request to join or a team identity waiting for approval.

[Back to the manual](README.md)

## Contents

- [The bell](#the-bell)
- [The notifications page](#the-notifications-page)
- [Who receives what](#who-receives-what)
- [How long they are kept](#how-long-they-are-kept)

## The bell

A signed-in player sees a bell in the top bar, next to the photo. A red number on the bell counts the notifications not read yet. Visitors who are not signed in do not see the bell.

A click on the bell opens the 8 newest notifications. The unread ones are in bold with a red mark on the left. A click on one opens its page, for example the page of the championship or the captain area, and marks it as read.

At the bottom of the list:

| Link | What it does |
|---|---|
| See all | Opens the notifications page. |
| Mark all as read | Asks for confirmation, then marks every notification as read. The number on the bell goes back to zero. |

## The notifications page

**See all** opens `/notifications`, the full list from the newest to the oldest, 20 per page. The unread ones have a red **New** label. **Open** goes to the page of the notification and marks it as read. **Mark all as read** at the top does the same as the link in the bell.

The text of each notification is shown in the language the player chose, and the players named in it are shown by their current name.

## Who receives what

| Event | Who receives it |
|---|---|
| A championship is published, or its registrations open | Everyone. When the championship takes requests to join: "There is a new championship you can join". |
| A player asks for a place, or cancels the request | The administrators. |
| A request to join is approved or refused | The player who asked. |
| A player is added to or removed from the participants, or put in a team | That player. |
| A player is made captain or vice-captain | That player, with a link to the captain area. |
| The order of the draft is drawn, or a pick is made | The captains; the picked player; the captain and vice-captain of the team whose turn it is. |
| A team sends a new name, tag or flag | The administrators. |
| The new name, tag or flag is approved or refused | The captain and the vice-captain of the team. |
| The games of a stage are created | The players of the teams in the stage. |
| A date is proposed or refused, a campaign is vetoed or chosen | The captain and the vice-captain of the other team. |
| A series is scheduled, a game ends, a series is won or decided by walkover | The players of both teams. |
| The championship starts, finishes or is cancelled | The participants and the players of the teams. |
| A player becomes an administrator | That player. |
| A punishment is given or removed | That player. |
| A chat ban is given or removed | That player. |
| A player is banned from the game servers, or the ban is lifted | That player. |
| An account shares an IP address with a banned account (see [Bans](bans.md#related-accounts-and-alerts)) | The administrators. |
| A player is blocked from voting in the community ranking, or unblocked | That player. |
| A forced name is set or removed | That player. |
| A skin is given as a gift or refunded | That player. |
| A new season starts | Everyone. The first season, created when the site is set up, does not send it. |
| The automatic backup fails, or its check fails | The administrators. A backup started by hand shows its result on the screen instead. |

A notification for everyone or for the administrators is read by each person on their own: when one administrator reads it, it stays unread for the others.

## How long they are kept

Notifications older than 90 days are deleted once a day, together with the record of who read them.

[Back to the manual](README.md)
