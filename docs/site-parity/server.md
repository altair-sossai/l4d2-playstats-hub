# Site parity: server

🌐 **English** | [Português](server.pt.md) | [Español](server.es.md)

Checklist of everything the old site showed in the **server** area, and where it stands in the hub.

Old files are under `l4d2-playstats-web/L4D2PlayStats.Web`. `Web/` means the project `L4D2PlayStats.Web`, `Core/` means `L4D2PlayStats.Core`.

Status: `done` (same content and look), `adapted` (present, changed for the stated reason), `not ported` (cannot exist in the hub, with the reason).

## Contents

- [Routes](#routes)
- [Server page](#server-page)
- [Scoreboard header](#scoreboard-header)
- [Players](#players)
- [Feed and chat](#feed-and-chat)
- [Game events](#game-events)
- [Terminal](#terminal)
- [Server info modal](#server-info-modal)
- [Live state rules](#live-state-rules)
- [Game server and public API](#game-server-and-public-api)
- [Layout hooks](#layout-hooks)
- [Chat bans page](#chat-bans-page)
- [Chat bans API and rules](#chat-bans-api-and-rules)
- [Hub-only pages](#hub-only-pages)

## Routes

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| `/server` opens the live page | `Web/Controllers/ServerController.cs:16` | adapted | The hub has several game servers, so the page lives at `/servers`: with one enabled server it is the live page itself, with more it is a picker and each server is at `/servers/{publicId}`. `/server` redirects there permanently. |
| `/servers` redirects to `/server` | `Web/Controllers/ServerController.cs:26` | adapted | Reversed: `/servers` is the hub route and `/server` redirects to it. |
| `/server/header` partial | `Web/Controllers/ServerController.cs:32` | adapted | Now `/servers/{publicId}/header`. The old route redirects (302, the target depends on the server) to the server with most players. |
| `/server/players` partial | `Web/Controllers/ServerController.cs:40` | adapted | Now `/servers/{publicId}/players`, with the same redirect. |
| `/server/feed?after=` partial, 204 when nothing is new | `Web/Controllers/ServerController.cs:48` | adapted | Now `/servers/{publicId}/feed?after=`, still 204 when nothing is new, with the same redirect. `after` is the feed sequence instead of the item ticks (see "New items appended"). |
| `/connect` redirects to `steam://connect/<ip>` | `Web/Controllers/ConnectController.cs:8` | done | Already in `SiteController`: goes to the server with most players, or to `/servers` when no server has an address. |
| `/chat-bans` administrator page | `Web/Controllers/ChatBansController.cs:17` | adapted | Now `/admin/chat-bans`, as every administrator page of the hub. `/chat-bans` redirects there permanently, keeping the query string. |
| Menu item "Server" active (`ViewBag.Server`) | `Web/Controllers/ServerController.cs:19` | done |  |
| Menu items "Admin" and "Chat Bans" active | `Web/Controllers/ChatBansController.cs:20` | done |  |

## Server page

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Page metadata: title "Server" | `Web/PageMetadata/ServerPageMetadata.cs:20` | done |  |
| Page metadata: description with map and connected players | `Web/PageMetadata/ServerPageMetadata.cs:21` | adapted | The text takes the site name instead of "L4D2 Competitive" (new text in the `server` fragments). The picker has its own description. |
| Page metadata: image is the campaign thumbnail, alt is the campaign or map | `Web/PageMetadata/ServerPageMetadata.cs:22` | done |  |
| Title `<h4 class="text-center">Server</h4>` | `Web/Views/Server/Index.cshtml:835` | done |  |
| `#game-info-header` container with the header partial | `Web/Views/Server/Index.cshtml:837` | done |  |
| `#game-info-players` container with the players partial | `Web/Views/Server/Index.cshtml:841` | done |  |
| `#game-info-chat` row, hidden while nobody is connected | `Web/Views/Server/Index.cshtml:845` | adapted | Same row. It has this id only when one server is enabled, because the layout shows and hides it by the server with most players; with several servers the page toggles it itself from the players partial. |
| "connect <ip>" button, centred, hidden on phones (`hide-mobile`) | `Web/Views/Server/Index.cshtml:915` | adapted | Same button. It links straight to `steam://connect/<address>` of the server on screen (the old `/connect` only did that redirect) and is left out when the server has no address. |
| Page styles (tables, progress bars, latency, feed, event colours, terminal) | `Web/Views/Server/Index.cshtml:21` | done |  |
| Header refreshed every 2.6 s and patched with diffDOM | `Web/Views/Server/Index.cshtml:195` | adapted | Still the same requests and diffDOM. When the SignalR link at `/hubs/live` is open, the page asks only when the site says the data changed (never more often than this, never before the delay) and every 30 s as a safety; without it, as before. |
| Players refreshed every 2.6 s and patched with diffDOM | `Web/Views/Server/Index.cshtml:211` | adapted | Still the same requests and diffDOM. When the SignalR link at `/hubs/live` is open, the page asks only when the site says the data changed (never more often than this, never before the delay) and every 30 s as a safety; without it, as before. |
| Feed refreshed every 1.3 s, only one request at a time | `Web/Views/Server/Index.cshtml:244` | adapted | Still the same requests and diffDOM. When the SignalR link at `/hubs/live` is open, the page asks only when the site says the data changed (never more often than this, never before the delay) and every 30 s as a safety; without it, as before. |
| Requests time out after 2 s and the next one is scheduled on completion | `Web/Views/Server/Index.cshtml:199` | done |  |

## Scoreboard header

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Panel heading: server name, configuration name and team size ("4v4"), joined by " - " | `Web/Views/Server/_Header.cshtml:22` | adapted | The name is the host name the game server sends in its heartbeat (the old site asked Steam); before the first heartbeat it is the name the server was registered with. |
| Anti-cheat label (on, off, API unavailable) with lock icon, colour and tooltip | `Web/Views/Server/_Header.cshtml:37` | done |  |
| Anti-cheat label links to the anti-cheat page | `Web/Views/Server/_Header.cshtml:46` | adapted | The label is the same. The link is written only while the anti-cheat module is on, since the Anti-Cheat page belongs to that module. |
| "Server IP" label with the question mark button that opens the server info modal | `Web/Views/Server/_Header.cshtml:58` | done |  |
| Server IP value with the copy icon (copies `connect <ip>`, "Text copied!" popover, icon per theme) | `Web/Views/Server/_Header.cshtml:65` | done | The address is the one of the server on screen. The copy icon is left out when the server has no address. |
| Players: connected / maximum | `Web/Views/Server/_Header.cshtml:73` | adapted | Connected is the number of players in the live state (survivors, infected and spectators) instead of the Steam query. Maximum is the slots the game server sends in its heartbeat (`maxPlayers`, added to the plugin); with an older plugin it is twice the team size. |
| Map: campaign, map name and round ("1st round" / "2nd round") | `Web/Views/Server/_Header.cshtml:78` | adapted | The map comes from the heartbeat of the game server (every 30 s) instead of the Steam query. Campaign names come from the hub catalog. |
| Map: label Paused, Starting... or Started with tooltip | `Web/Views/Server/_Header.cshtml:96` | done |  |
| Scoreboard: survivors and infected labels, green for the leader, red for the other, grey on a draw, with tooltips | `Web/Views/Server/_Header.cshtml:117` | done |  |
| Scoreboard: difference label when not zero | `Web/Views/Server/_Header.cshtml:131` | done |  |
| Progress points: current / maximum and percentage label | `Web/Views/Server/_Header.cshtml:139` | done |  |
| Bonus: current / maximum and percentage label | `Web/Views/Server/_Header.cshtml:154` | done |  |
| Progress percentage | `Web/Views/Server/_Header.cshtml:165` | done |  |
| Tank: percentage, "Disabled", and the labels Alive / Dead | `Web/Views/Server/_Header.cshtml:170` | done |  |
| Witch: percentage or "Disabled" | `Web/Views/Server/_Header.cshtml:196` | done |  |
| Phone layout: `table-responsive` with `data-label` on every cell | `Web/Views/Server/_Header.cshtml:55` | done |  |

## Players

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Whole block hidden while nobody is connected | `Web/Views/Server/_Players.cshtml:41` | done |  |
| JSON blocks `#survivors`, `#infecteds`, `#spectators` used by the terminal | `Web/Views/Server/_Players.cshtml:43` | done | Same blocks with the same player fields, minus `avatarUrl`, which the hub does not keep (avatars are served by `/avatar/{id}`). |
| Score bar: two segments sized by score, green and red, "+difference" on the leader, dashed middle marker, tooltips | `Web/Views/Server/_Players.cshtml:55` | done |  |
| Panel headings "Survivors" and "Infecteds" with the score label while a team has no points | `Web/Views/Server/_Players.cshtml:83` | done |  |
| Survivor: character picture with tooltip, grey when dead | `Web/Views/Server/_Players.cshtml:101` | done |  |
| Survivor: avatar linked to the player page, grey when dead | `Web/Views/Server/_Players.cshtml:106` | done | Avatar from `/avatar/{id}` with the empty avatar as fallback. The link always goes to the player page, which shows the basic page with the Steam profile when the ranking module is off. |
| Survivor: name linked to the player page | `Web/Views/Server/_Players.cshtml:114` | done |  |
| Survivor: labels Admin, Dead, Incapacitated, Black and white | `Web/Views/Server/_Players.cshtml:116` | done |  |
| Survivor: latency icon (low, medium, high) with tooltip | `Web/Views/Server/_Players.cshtml:145` | done |  |
| Survivor: weapon icons for the five slots, per theme, with the melee weapon icon in place of the generic one | `Web/Views/Server/_Players.cshtml:157` | done | Image paths are absolute, since the page is no longer at the root. |
| Which weapons have an icon | `Web/Extensions/WeaponExtensions.cs:7`, `Web/Extensions/MeleeWeaponExtensions.cs:7` | done |  |
| Weapon icon file names and descriptions | `Core/GameInfo/Enums/Weapon.cs`, `Core/GameInfo/Enums/MeleeWeapon.cs` | done | Added to the hub enums (`WeaponName` attribute). |
| Survivor: health bar (permanent and temporary, colour by health, faded when incapacitated) and number | `Web/Views/Server/_Players.cshtml:191` | done |  |
| Survivor: progress bar and percentage | `Web/Views/Server/_Players.cshtml:231` | done |  |
| Survivors sorted by character, separated by a rule | `Core/GameInfo/GameInfo.cs:181`, `Web/Views/Server/_Players.cshtml:248` | done |  |
| Infected: class picture with tooltip ("none" while not spawned) | `Web/Views/Server/_Players.cshtml:282` | done |  |
| Infected: avatar and name linked to the player page | `Web/Views/Server/_Players.cshtml:290` | done | Same remarks as the survivor avatar. |
| Infected: labels Admin, Spawning, Dead | `Web/Views/Server/_Players.cshtml:302` | done |  |
| Infected: damage done, with tooltip | `Web/Views/Server/_Players.cshtml:325` | done |  |
| Infected: latency icon | `Web/Views/Server/_Players.cshtml:330` | done |  |
| Infected: health bar and "health / maximum" | `Web/Views/Server/_Players.cshtml:342` | done |  |
| Infected sorted by damage, separated by a rule | `Core/GameInfo/GameInfo.cs:188`, `Web/Views/Server/_Players.cshtml:366` | done |  |
| Spectators: count and names as labels linked to the player page, sorted by name | `Web/Views/Server/_Players.cshtml:383`, `Core/GameInfo/GameInfo.cs:195` | done |  |
| Stable row ids (`tr0-survivor-0`...) so diffDOM patches in place | `Web/Views/Server/_Players.cshtml:100` | done |  |
| Two columns on desktop, stacked on phones (`col-xs-12 col-sm-6`) | `Web/Views/Server/_Players.cshtml:80` | done |  |

## Feed and chat

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Panel "Chat messages" with the filter dropdown All / Events / Messages | `Web/Views/Server/Index.cshtml:848` | done |  |
| Filter remembered in `localStorage` (`feedFilter`) and applied to new items | `Web/Views/Server/Index.cshtml:226` | done |  |
| Feed box 400 px high, scrolled to the end on load and kept at the end when new items arrive | `Web/Views/Server/Index.cshtml:100` | done |  |
| One `<p>` per item with `data-feed-when` and `data-feed-kind` | `Web/Views/Server/_Feed.cshtml:6` | done | Also carries `data-feed-sequence` (see below) and the time of the item as a hover title. |
| New items appended, duplicates removed, at most 300 kept | `Web/Views/Server/Index.cshtml:264` | adapted | Same behaviour. The cursor sent in `after` is the feed sequence of the last answer instead of the ticks of the last item on screen: game messages appear 15 s late, so with ticks a site message could hide a game message that came just before it. |
| Message: team in brackets for team chat | `Web/Views/Server/_Message.cshtml:17` | done |  |
| Message: player name coloured by team (survivor, infected, site, spectator) and linked to the player page | `Web/Views/Server/_Message.cshtml:21` | done | The link always goes to the player page, which shows the basic page with the Steam profile when the ranking module is off. |
| Message: "(Admin)" after the name | `Web/Views/Server/_Message.cshtml:24` | done |  |
| Message text | `Web/Views/Server/_Message.cshtml:29` | done |  |
| Banned visitor: "You have been banned from the chat." and the reason, no form | `Web/Views/Server/Index.cshtml:872` | done |  |
| Signed in: message box, Send button disabled while empty, Enter sends | `Web/Views/Server/Index.cshtml:884` | done | The box also limits the text to 200 characters. |
| Administrator: terminal button with icon and tooltip | `Web/Views/Server/Index.cshtml:887` | adapted | Shown to an administrator only while the remote commands module is on, since the commands would not reach the game server otherwise. |
| Signed out: "Sign in with Steam to use the chat." button with the Steam icon | `Web/Views/Server/Index.cshtml:905` | done | After signing in the visitor comes back to this page. |
| Sending: POST JSON, feed refreshed and box cleared on success, alert with the error otherwise | `Web/Views/Server/Index.cshtml:293` | adapted | Posts a form with the antiforgery token to `/servers/{publicId}/chat` and gets the same `{ success, errorMessage }` answer. The old API had no protection against forged requests. Error texts are translated. |
| Sending while banned: alert and page reload | `Web/Views/Server/Index.cshtml:313` | done | HTTP 403 with the reason, as before. |

## Game events

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Player name in an event links to the Steam profile, or is plain text without one | `Web/Extensions/GameEventHtmlExtensions.cs:20` | adapted | The name links to the player page of the site, which offers the Steam profile. |
| Coloured parts: stars and highlight, player, action, danger | `Web/Views/Server/Index.cshtml:121` | done |  |
| Stars before the events that have them | `Core/GameInfo/Extensions/GameEventTypeExtensions.cs:7` | done |  |
| Boomer vomit landed | `Web/Views/Server/GameEvents/_BoomerVomitLandedEvent.cshtml` | done |  |
| Car alarm triggered, with reason and assisting infected | `Web/Views/Server/GameEvents/_CarAlarmTriggeredEvent.cshtml` | done |  |
| Charger level | `Web/Views/Server/GameEvents/_ChargerLevelEvent.cshtml` | done |  |
| Charger level with chip damage | `Web/Views/Server/GameEvents/_ChargerLevelHurtEvent.cshtml` | done |  |
| Death charge, with bowling note and height | `Web/Views/Server/GameEvents/_DeathChargeEvent.cshtml` | done |  |
| Unknown event | `Web/Views/Server/GameEvents/_GameEvent.cshtml` | done |  |
| Hunter high pounce, with damage and height | `Web/Views/Server/GameEvents/_HunterHighPounceEvent.cshtml` | done |  |
| Mix started | `Web/Views/Server/GameEvents/_MixStartedEvent.cshtml` | done |  |
| Pause and unpause | `Web/Views/Server/GameEvents/_PauseEvent.cshtml` | done |  |
| Player death | `Web/Views/Server/GameEvents/_PlayerDeathEvent.cshtml` | done |  |
| Round ended, with score and difference | `Web/Views/Server/GameEvents/_RoundEndedEvent.cshtml` | done |  |
| Round live, first or second | `Web/Views/Server/GameEvents/_RoundLiveEvent.cshtml` | done |  |
| Skeet (melee, sniper, grenade, plain) and team skeet | `Web/Views/Server/GameEvents/_SkeetEvent.cshtml` | done |  |
| Skeet with chip damage and overkill note | `Web/Views/Server/GameEvents/_SkeetHurtEvent.cshtml` | done |  |
| Tank became a bot | `Web/Views/Server/GameEvents/_TankBecameBotEvent.cshtml` | done |  |
| Tank died | `Web/Views/Server/GameEvents/_TankDiedEvent.cshtml` | done |  |
| Tank rock hit | `Web/Views/Server/GameEvents/_TankRockEatenEvent.cshtml` | done |  |
| Tank rock skeeted | `Web/Views/Server/GameEvents/_TankRockSkeetedEvent.cshtml` | done |  |
| Tank spawned | `Web/Views/Server/GameEvents/_TankSpawnedEvent.cshtml` | done |  |
| Tongue cut | `Web/Views/Server/GameEvents/_TongueCutEvent.cshtml` | done |  |
| Witch crown | `Web/Views/Server/GameEvents/_WitchCrownEvent.cshtml` | done |  |
| Witch draw crown | `Web/Views/Server/GameEvents/_WitchCrownHurtEvent.cshtml` | done |  |

## Terminal

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Modal with the command preview and the option list | `Web/Views/Server/Index.cshtml:923` | done |  |
| The 24 commands with their descriptions and arguments | `Web/Views/Server/Index.cshtml:324` | done |  |
| Argument lists: wildcards, survivors, infected, spectators, players, team, minutes, reason, match mode, anti-cheat state, campaign | `Web/Views/Server/Index.cshtml:422` | done |  |
| Campaign list loaded from `/api/campaigns` | `Web/Views/Server/Index.cshtml:180` | done | New endpoint in the hub, same JSON (`name`, `maps`). |
| Step by step selection with "Back", values with spaces quoted | `Web/Views/Server/Index.cshtml:617` | done |  |
| Confirmation before running, then sent through the chat form | `Web/Views/Server/Index.cshtml:761` | done |  |
| A command typed in the chat by an administrator runs on the game server | `Web/Controllers/Api/ExternalChatController.cs:43` | done | Needs the remote commands module. Every command is also written to the audit log. |

## Server info modal

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Modal "Server info" opened from the header | `Web/Views/Server/Index.cshtml:942` | done |  |
| IP row with the "Recommended" label, `connect <ip>` and copy icon | `Web/Views/Server/Index.cshtml:954` | done | Shown when the server has an address. |
| DNS row with the "Alternative" label, `connect <dns>` and copy icon | `Web/Views/Server/Index.cshtml:966` | adapted | New field "alternative address" on the game server (set in the administration). The row and the explanation appear when it is filled in. |
| Explanation of when to use the DNS | `Web/Views/Server/Index.cshtml:980` | done | Shown together with the DNS row. |

## Live state rules

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Everything from the game is shown 15 s late | `Core/GameInfo/GameInfo.cs:28` | done |  |
| Configuration, round and players expire after 2 h, the scoreboard after 10 min | `Core/GameInfo/GameInfo.cs:33` | done |  |
| Feed keeps items for 1 h, 250 at most | `Core/GameInfo/GameInfo.cs:27`, `Core/GameInfo/Structures/TimedFeed.cs:5` | done |  |
| Chat binds are dropped ("gogogo", "tank tank", "r"...) | `Core/GameInfo/Filters/ChatBindBlacklist.cs` | done |  |
| A player's message equal to the previous one is dropped | `Core/GameInfo/GameInfo.cs:122` | done |  |
| Site message: 200 characters at most | `Core/GameInfo/GameInfo.cs:154` | done |  |
| Site message: 15 s between any two, 30 s per visitor; administrators are exempt | `Core/GameInfo/GameInfo.cs:161` | done |  |
| Site messages kept 1 h for the game server to read | `Core/GameInfo/GameInfo.cs:96` | done |  |
| Site message appears in the feed at once, as team "External" | `Core/GameInfo/GameInfo.cs:175`, `Core/GameInfo/Models/ExternalChatMessage.cs:17` | done |  |
| Latency: low under 100 ms, medium under 200 ms, high otherwise | `Core/GameInfo/Models/Player.cs:25` | done |  |
| Health colour: red when incapacitated or under 25, orange under 40, green otherwise; maximum 100, or 300 incapacitated | `Core/GameInfo/Models/Survivor.cs:11` | done |  |
| Scoreboard values (percentages, difference, leader) | `Core/GameInfo/Models/Scoreboard.cs:13` | done |  |
| Tank and witch disabled at 0 % | `Core/GameInfo/Models/Round.cs:14` | done |  |
| Event stars per type; a team skeet or a bot hunter gives one | `Core/GameInfo/Extensions/GameEventTypeExtensions.cs`, `Core/GameInfo/Models/Events/SkeetEvent.cs:11` | done |  |
| Server command format (`!`, `/` or `sm_`, 256 characters at most) | `Core/GameInfo/Models/ServerCommand.cs:7` | adapted | Same format; the hub also refuses a semicolon or a line break in the arguments, so one message cannot carry two commands. |
| Player connections: saved per address, related accounts returned, cleaned up | `Core/GameInfo/Services/PlayerConnectionInfoService.cs` | done | Connections are kept for three months. |
| Server name, map, players and slots read from Steam every 10 s | `Core/Steam/ServerInfo/Services/Cache/ServerInfoServiceCached.cs` | adapted | The hub does not ask Steam (that needs an API key and one fixed address). The same four values come from the game server itself: host name, map and slots in the heartbeat, players from the live state. |

## Game server and public API

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| `PUT /api/game-info/configuration`, `round`, `scoreboard`, `players`, `messages` (game server) | `Web/Controllers/Api/GameInfoController.cs:33` | done |  |
| `POST /api/game-info/events` (game server) | `Web/Controllers/Api/GameInfoController.cs:123` | done |  |
| `POST /api/game-info/player-connection-info` (game server) | `Web/Controllers/Api/GameInfoController.cs:132` | done |  |
| `GET /api/game-info/server-command/dequeue` (game server) | `Web/Controllers/Api/GameInfoController.cs:144` | done |  |
| `GET /api/game-info` and `/configuration`, `/round`, `/scoreboard`, `/players`, `/messages`, `/events` (public) | `Web/Controllers/Api/GameInfoController.cs:19` | adapted | Now per server: `/api/servers/{publicId}/game-info` and the same sub-paths. The old paths redirect (302) to the server with most players. |
| `GET /api/external-chat?after=` | `Web/Controllers/Api/ExternalChatController.cs:20` | adapted | Same path and JSON, read by the game server with its token; each server gets only the messages written on its own page. |
| `POST /api/external-chat` (signed-in visitor) | `Web/Controllers/Api/ExternalChatController.cs:28` | adapted | Replaced by `POST /servers/{publicId}/chat` (see "Sending"). A POST cannot be redirected, and the old path is not used by any page. |
| `GET /api/server` | `Web/Controllers/Api/ServerController.cs:9` | adapted | Same path, used by the layout. The JSON has `publicId`, `name`, `address`, `map`, `players` and `max_players` instead of the whole Steam answer. The layout asks for it only after the visitor first moves the mouse, scrolls, taps or types, and stops while the tab is in the background. |
| `GET /api/servers` redirects to `/api/server` | `Web/Controllers/Api/ServerController.cs:17` | not ported | In the hub `/api/servers` is the public list of game servers, so it cannot redirect to `/api/server`. |
| `GET /api/campaigns` | `Web/Controllers/Api/CampaignController.cs` | done |  |

## Layout hooks

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Navbar badge "● players/slots", pulsing, linked to the server page, hidden with nobody connected | `Web/Views/Shared/_Layout.cshtml:322` | done | Already in the layout. The numbers are the ones of the header (see "Players"). |
| Browser title prefixed with "🟢 players/slots" | `Web/Views/Shared/_Layout.cshtml:602` | done |  |
| Chat row shown or hidden by the same check, every 4 s | `Web/Views/Shared/_Layout.cshtml:594` | adapted | The same check, starting only after the visitor first moves the mouse, scrolls, taps or types, and paused while the tab is in the background. |
| Floating connect icon, hidden on phones | `Web/Views/Shared/_Layout.cshtml:526` | done | Already in the layout. It is hidden while no server has an address; the old one was always there. |

## Chat bans page

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Administrators only | `Web/Controllers/ChatBansController.cs:10` | done | Also needs the site chat module. |
| Header "Chat Bans" with the "Add" button | `Web/Views/ChatBans/Index.cshtml:35` | done |  |
| Lead text | `Web/Views/ChatBans/Index.cshtml:42` | done |  |
| Counter of banned players | `Web/Views/ChatBans/Index.cshtml:44` | done |  |
| Search by name, Steam ID or reason, with "Clear filters" | `Web/Views/ChatBans/Index.cshtml:51`, `Web/Controllers/ChatBansController.cs:25` | done |  |
| List item: avatar, name, Steam ID and SteamID64, reason in two lines ("No reason given") | `Web/Views/ChatBans/Index.cshtml:67` | done | The Steam ID is written `STEAM_1:...`, as everywhere in the hub. |
| List item: ban date and "by <administrator>" | `Web/Views/ChatBans/Index.cshtml:83` | done | Dates are in the time zone of the site settings instead of the time zone of the machine. |
| Empty list text | `Web/Views/ChatBans/Index.cshtml:63` | done |  |
| Pagination, 20 per page, keeping the search | `Web/Views/ChatBans/Index.cshtml:94`, `Web/Controllers/ChatBansController.cs:14` | done |  |
| Details modal: avatar, name linked to the player page, IDs, Steam profile link | `Web/Views/ChatBans/Index.cshtml:96` | done | The name always links to the player page, also when the ranking module is off. |
| Details modal: ban date, updated at, banned by | `Web/Views/ChatBans/Index.cshtml:115` | done |  |
| Details modal: edit and save the reason | `Web/Views/ChatBans/Index.cshtml:123`, `:245` | done |  |
| Details modal: remove with a second confirmation | `Web/Views/ChatBans/Index.cshtml:127`, `:260` | done |  |
| Add modal: Steam ID or profile URL field | `Web/Views/ChatBans/Index.cshtml:153` | done |  |
| Add modal: a typed Steam ID shows the photo and the name of the player | `Web/Views/ChatBans/Index.cshtml:348-371` | done | Same lookup as the Anti-Cheat settings, `/api/admin/players/lookup`. |
| Add modal: "Choose from the list" opens the player picker | `Web/Views/ChatBans/Index.cshtml:158`, `:334` | done |  |
| Add modal: reason | `Web/Views/ChatBans/Index.cshtml:165` | done |  |
| Add modal: recent senders with avatar, name, last message and "Already banned" | `Web/Views/ChatBans/Index.cshtml:169`, `:284` | done |  |
| Success and error messages, then reload | `Web/Views/ChatBans/Index.cshtml:201` | done |  |
| Styles (`_AdminStyles`, reason clamp, round avatar) | `Web/Views/ChatBans/Index.cshtml:16` | done |  |

## Chat bans API and rules

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| `GET /api/chat-bans` (administrator) | `Web/Controllers/Api/ChatBansController.cs:16` | done |  |
| `GET /api/chat-bans/recent-senders` (administrator) | `Web/Controllers/Api/ChatBansController.cs:25` | done |  |
| `POST /api/chat-bans` creates or updates a ban (administrator) | `Web/Controllers/Api/ChatBansController.cs:34` | adapted | Same path and JSON. It needs the antiforgery token of the page in the `RequestVerificationToken` header; the old API had no protection against forged requests. Error texts are translated. |
| `DELETE /api/chat-bans/{communityId}` (administrator) | `Web/Controllers/Api/ChatBansController.cs:51` | adapted | Same path, with the same token header. |
| The player is given as SteamID64, Steam ID, Steam3 or profile URL | `Core/ChatBans/Services/ChatBanService.cs:82` | done |  |
| A new ban keeps who created it and when; an update keeps them and sets "updated at" | `Core/ChatBans/Services/ChatBanService.cs:89` | done |  |
| The name is replaced only when a new one is given | `Core/ChatBans/Services/ChatBanService.cs:109` | done |  |
| Bans listed newest first | `Core/ChatBans/Services/ChatBanService.cs:39` | done |  |
| Recent senders: the last message of each visitor, newest first, 30 at most, flagged when already banned | `Core/ChatBans/Services/ChatBanService.cs:52` | done |  |
| A banned visitor cannot send: HTTP 403 with the reason | `Web/Controllers/Api/ExternalChatController.cs:32` | done |  |

## Hub-only pages

These did not exist in the old site. They are listed so that nothing the hub had is lost in the port.

| Item | Where in the hub | Status | Notes |
|---|---|---|---|
| Server picker at `/servers` when more than one server is enabled | `Views/Servers/Index.cshtml` | done | Panels in the look of the site, refreshed every 10 s with diffDOM. |
| Server switch on the live page when more than one server is enabled | `Views/Servers/Live.cshtml` | done | Pills under the title. |
| Player connections page (administrator) | `Views/PlayerConnections/Index.cshtml` | done | Restyled with the administrator styles; the search accepts any way of writing a Steam ID. |
| Remote commands page with the audit log (administrator) | `Views/RemoteCommands/Index.cshtml` | done | Restyled with the administrator styles. |
| Public JSON of one server (`/api/servers/{publicId}/game-info...`) | `Controllers/Api/LiveApiController.cs` | done | Unchanged. |

## Notes

- Totals: 151 `done`, 28 `adapted`, 1 `not ported`.
- The old players tables show no patent icon: neither `Views/Server/_Players.cshtml` nor the page in production has one, so there is nothing to port there.
- The statuses were checked against the rendered pages: side by side with `https://l4d2.com.br/server` (desktop, phone width and light theme), with a simulated match that sends every game event, and by driving the pages in a headless browser (feed filter, sending a message, the terminal, the server info modal, every modal of the chat bans page).
- The game server plugin sends the server slots in its heartbeat (`maxPlayers`) from this change on. A game server still running the previous plugin shows twice the team size as the maximum.
