# Site parity

This project replaces an existing website, [l4d2-playstats-web](https://github.com/altair-sossai/l4d2-playstats-web) (in production at [l4d2.com.br](https://l4d2.com.br/)). The rule for the pages is strict: everything a visitor sees on the old site must be on the new one, with the same look. Each page was therefore ported from the old view, not redesigned, and checked item by item.

## Contents

- [The checklists](#the-checklists)
- [How to read them](#how-to-read-them)
- [What is not ported](#what-is-not-ported)
- [What changed on purpose](#what-changed-on-purpose)
- [When you change a page](#when-you-change-a-page)

## The checklists

Each checklist lists every item of the old screens of one area: columns, fields, labels, tooltips, icons, buttons, links, charts, modals, filters, scripts, phone rules, page metadata, addresses and API endpoints.

| Checklist | Screens | Done | Adapted | Not ported |
|---|---|---|---|---|
| [site.md](site.md) | Layout, ranking, history, patents, Anti-Cheat | 79 | 24 | 1 |
| [players.md](players.md) | Players list, player page, comparison, player not ranked | 150 | 23 | 0 |
| [matches.md](matches.md) | Last matches, match details, map statistics | 128 | 18 | 1 |
| [server.md](server.md) | Live server, chat, game events, chat bans | 153 | 25 | 1 |
| [skins.md](skins.md) | Skin shop, gifts, prices, purchases | 144 | 13 | 0 |
| [admin.md](admin.md) | Pages that exist only here: wizard, administration, community ranking | 185 | 9 | 1 |

The first five compare the hub with the old site. The last one compares the pages that are new here with their first version, which used another look, so that nothing was lost when they were converted.

## How to read them

Every line has a status.

| Status | Meaning |
|---|---|
| `done` | Present here with the same content and look. |
| `adapted` | Present, but changed for the reason in the notes. |
| `not ported` | Cannot exist here, for the reason in the notes. |

The column that says where the item is in the old site gives a file and line of the old repository.

## What is not ported

Three items of the old site, each replaced by how this application works:

- **`POST /api/cache/clear`.** The old site cached what it read from a separate API. Here the site and the API are one application, and an upload refreshes the ranking by itself.
- **The one-hour cache of the matches list.** The list is read from the application's own database.
- **`GET /api/servers` redirecting to `/api/server`.** Here `/api/servers` is the public list of game servers.

## What changed on purpose

Most `adapted` lines fall into a few groups:

- **Fixed values that became settings.** The old site belonged to one community, so its name, logo, footer links, analytics ID, keywords, Anti-Cheat addresses and contact links were written in the code. Here the administrator sets them.
- **Seasons of any length.** Texts and counters that assumed two-month periods follow the configured season, and past seasons can be browsed.
- **Several game servers.** With one server the pages look as before. With more than one, a server picker and the server name appear.
- **Modules.** A block that belongs to a module (skin shop, live server, site chat, remote commands, Anti-Cheat) only shows while that module is on.
- **One application.** Addresses that pointed to a separate API point to this one. Old addresses redirect to the new ones.
- **Forms and scripts.** Requests that change data carry an antiforgery token.

## When you change a page

- Keep the item in the checklist true: if you change something a line describes, update the line.
- A new page follows the same look: the layout, the themes and the patterns of the pages around it.
- Do not remove a field, a column or a piece of information from a page that came from the old site.
