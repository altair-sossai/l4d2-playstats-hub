# Site parity: server

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
| `/server` opens the live page | `Web/Controllers/ServerController.cs:16` | pending | |
| `/servers` redirects to `/server` | `Web/Controllers/ServerController.cs:26` | pending | |
| `/server/header` partial | `Web/Controllers/ServerController.cs:32` | pending | |
| `/server/players` partial | `Web/Controllers/ServerController.cs:40` | pending | |
| `/server/feed?after=` partial, 204 when nothing is new | `Web/Controllers/ServerController.cs:48` | pending | |
| `/connect` redirects to `steam://connect/<ip>` | `Web/Controllers/ConnectController.cs:8` | pending | |
| `/chat-bans` administrator page | `Web/Controllers/ChatBansController.cs:17` | pending | |
| Menu item "Server" active (`ViewBag.Server`) | `Web/Controllers/ServerController.cs:19` | pending | |
| Menu items "Admin" and "Chat Bans" active | `Web/Controllers/ChatBansController.cs:20` | pending | |

## Server page

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Page metadata: title "Server" | `Web/PageMetadata/ServerPageMetadata.cs:20` | pending | |
| Page metadata: description with map and connected players | `Web/PageMetadata/ServerPageMetadata.cs:21` | pending | |
| Page metadata: image is the campaign thumbnail, alt is the campaign or map | `Web/PageMetadata/ServerPageMetadata.cs:22` | pending | |
| Title `<h4 class="text-center">Server</h4>` | `Web/Views/Server/Index.cshtml:835` | pending | |
| `#game-info-header` container with the header partial | `Web/Views/Server/Index.cshtml:837` | pending | |
| `#game-info-players` container with the players partial | `Web/Views/Server/Index.cshtml:841` | pending | |
| `#game-info-chat` row, hidden while nobody is connected | `Web/Views/Server/Index.cshtml:845` | pending | |
| "connect <ip>" button, centred, hidden on phones (`hide-mobile`) | `Web/Views/Server/Index.cshtml:915` | pending | |
| Page styles (tables, progress bars, latency, feed, event colours, terminal) | `Web/Views/Server/Index.cshtml:21` | pending | |
| Header refreshed every 2.6 s and patched with diffDOM | `Web/Views/Server/Index.cshtml:195` | pending | |
| Players refreshed every 2.6 s and patched with diffDOM | `Web/Views/Server/Index.cshtml:211` | pending | |
| Feed refreshed every 1.3 s, only one request at a time | `Web/Views/Server/Index.cshtml:244` | pending | |
| Requests time out after 2 s and the next one is scheduled on completion | `Web/Views/Server/Index.cshtml:199` | pending | |

## Scoreboard header

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Panel heading: server name, configuration name and team size ("4v4"), joined by " - " | `Web/Views/Server/_Header.cshtml:22` | pending | |
| Anti-cheat label (on, off, API unavailable) with lock icon, colour and tooltip | `Web/Views/Server/_Header.cshtml:37` | pending | |
| Anti-cheat label links to the anti-cheat page | `Web/Views/Server/_Header.cshtml:46` | pending | |
| "Server IP" label with the question mark button that opens the server info modal | `Web/Views/Server/_Header.cshtml:58` | pending | |
| Server IP value with the copy icon (copies `connect <ip>`, "Text copied!" popover, icon per theme) | `Web/Views/Server/_Header.cshtml:65` | pending | |
| Players: connected / maximum | `Web/Views/Server/_Header.cshtml:73` | pending | |
| Map: campaign, map name and round ("1st round" / "2nd round") | `Web/Views/Server/_Header.cshtml:78` | pending | |
| Map: label Paused, Starting... or Started with tooltip | `Web/Views/Server/_Header.cshtml:96` | pending | |
| Scoreboard: survivors and infected labels, green for the leader, red for the other, grey on a draw, with tooltips | `Web/Views/Server/_Header.cshtml:117` | pending | |
| Scoreboard: difference label when not zero | `Web/Views/Server/_Header.cshtml:131` | pending | |
| Progress points: current / maximum and percentage label | `Web/Views/Server/_Header.cshtml:139` | pending | |
| Bonus: current / maximum and percentage label | `Web/Views/Server/_Header.cshtml:154` | pending | |
| Progress percentage | `Web/Views/Server/_Header.cshtml:165` | pending | |
| Tank: percentage, "Disabled", and the labels Alive / Dead | `Web/Views/Server/_Header.cshtml:170` | pending | |
| Witch: percentage or "Disabled" | `Web/Views/Server/_Header.cshtml:196` | pending | |
| Phone layout: `table-responsive` with `data-label` on every cell | `Web/Views/Server/_Header.cshtml:55` | pending | |

## Players

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Whole block hidden while nobody is connected | `Web/Views/Server/_Players.cshtml:41` | pending | |
| JSON blocks `#survivors`, `#infecteds`, `#spectators` used by the terminal | `Web/Views/Server/_Players.cshtml:43` | pending | |
| Score bar: two segments sized by score, green and red, "+difference" on the leader, dashed middle marker, tooltips | `Web/Views/Server/_Players.cshtml:55` | pending | |
| Panel headings "Survivors" and "Infecteds" with the score label while a team has no points | `Web/Views/Server/_Players.cshtml:83` | pending | |
| Survivor: character picture with tooltip, grey when dead | `Web/Views/Server/_Players.cshtml:101` | pending | |
| Survivor: avatar linked to the player page, grey when dead | `Web/Views/Server/_Players.cshtml:106` | pending | |
| Survivor: name linked to the player page | `Web/Views/Server/_Players.cshtml:114` | pending | |
| Survivor: labels Admin, Dead, Incapacitated, B&W | `Web/Views/Server/_Players.cshtml:116` | pending | |
| Survivor: latency icon (low, medium, high) with tooltip | `Web/Views/Server/_Players.cshtml:145` | pending | |
| Survivor: weapon icons for the five slots, per theme, with the melee weapon icon in place of the generic one | `Web/Views/Server/_Players.cshtml:157` | pending | |
| Which weapons have an icon | `Web/Extensions/WeaponExtensions.cs:7`, `Web/Extensions/MeleeWeaponExtensions.cs:7` | pending | |
| Weapon icon file names and descriptions | `Core/GameInfo/Enums/Weapon.cs`, `Core/GameInfo/Enums/MeleeWeapon.cs` | pending | |
| Survivor: health bar (permanent and temporary, colour by health, faded when incapacitated) and number | `Web/Views/Server/_Players.cshtml:191` | pending | |
| Survivor: progress bar and percentage | `Web/Views/Server/_Players.cshtml:231` | pending | |
| Survivors sorted by character, separated by a rule | `Core/GameInfo/GameInfo.cs:181`, `Web/Views/Server/_Players.cshtml:248` | pending | |
| Infected: class picture with tooltip ("none" while not spawned) | `Web/Views/Server/_Players.cshtml:282` | pending | |
| Infected: avatar and name linked to the player page | `Web/Views/Server/_Players.cshtml:290` | pending | |
| Infected: labels Admin, Spawning, Dead | `Web/Views/Server/_Players.cshtml:302` | pending | |
| Infected: damage done, with tooltip | `Web/Views/Server/_Players.cshtml:325` | pending | |
| Infected: latency icon | `Web/Views/Server/_Players.cshtml:330` | pending | |
| Infected: health bar and "health / maximum" | `Web/Views/Server/_Players.cshtml:342` | pending | |
| Infected sorted by damage, separated by a rule | `Core/GameInfo/GameInfo.cs:188`, `Web/Views/Server/_Players.cshtml:366` | pending | |
| Spectators: count and names as labels linked to the player page, sorted by name | `Web/Views/Server/_Players.cshtml:383`, `Core/GameInfo/GameInfo.cs:195` | pending | |
| Stable row ids (`tr0-survivor-0`...) so diffDOM patches in place | `Web/Views/Server/_Players.cshtml:100` | pending | |
| Two columns on desktop, stacked on phones (`col-xs-12 col-sm-6`) | `Web/Views/Server/_Players.cshtml:80` | pending | |

## Feed and chat

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Panel "Chat messages" with the filter dropdown All / Events / Messages | `Web/Views/Server/Index.cshtml:848` | pending | |
| Filter remembered in `localStorage` (`feedFilter`) and applied to new items | `Web/Views/Server/Index.cshtml:226` | pending | |
| Feed box 400 px high, scrolled to the end on load and kept at the end when new items arrive | `Web/Views/Server/Index.cshtml:100` | pending | |
| One `<p>` per item with `data-feed-when` and `data-feed-kind` | `Web/Views/Server/_Feed.cshtml:6` | pending | |
| New items appended, duplicates removed, at most 300 kept | `Web/Views/Server/Index.cshtml:264` | pending | |
| Message: team in brackets for team chat | `Web/Views/Server/_Message.cshtml:17` | pending | |
| Message: player name coloured by team (survivor, infected, site, spectator) and linked to the player page | `Web/Views/Server/_Message.cshtml:21` | pending | |
| Message: "(Admin)" after the name | `Web/Views/Server/_Message.cshtml:24` | pending | |
| Message text | `Web/Views/Server/_Message.cshtml:29` | pending | |
| Banned visitor: "You have been banned from the chat." and the reason, no form | `Web/Views/Server/Index.cshtml:872` | pending | |
| Signed in: message box, Send button disabled while empty, Enter sends | `Web/Views/Server/Index.cshtml:884` | pending | |
| Administrator: terminal button with icon and tooltip | `Web/Views/Server/Index.cshtml:887` | pending | |
| Signed out: "Sign in with Steam to use the chat." button with the Steam icon | `Web/Views/Server/Index.cshtml:905` | pending | |
| Sending: POST JSON, feed refreshed and box cleared on success, alert with the error otherwise | `Web/Views/Server/Index.cshtml:293` | pending | |
| Sending while banned: alert and page reload | `Web/Views/Server/Index.cshtml:313` | pending | |

## Game events

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Player name in an event links to the Steam profile, or is plain text without one | `Web/Extensions/GameEventHtmlExtensions.cs:20` | pending | |
| Coloured parts: stars and highlight, player, action, danger | `Web/Views/Server/Index.cshtml:121` | pending | |
| Stars before the events that have them | `Core/GameInfo/Extensions/GameEventTypeExtensions.cs:7` | pending | |
| Boomer vomit landed | `Web/Views/Server/GameEvents/_BoomerVomitLandedEvent.cshtml` | pending | |
| Car alarm triggered, with reason and assisting infected | `Web/Views/Server/GameEvents/_CarAlarmTriggeredEvent.cshtml` | pending | |
| Charger level | `Web/Views/Server/GameEvents/_ChargerLevelEvent.cshtml` | pending | |
| Charger level with chip damage | `Web/Views/Server/GameEvents/_ChargerLevelHurtEvent.cshtml` | pending | |
| Death charge, with bowling note and height | `Web/Views/Server/GameEvents/_DeathChargeEvent.cshtml` | pending | |
| Unknown event | `Web/Views/Server/GameEvents/_GameEvent.cshtml` | pending | |
| Hunter high pounce, with damage and height | `Web/Views/Server/GameEvents/_HunterHighPounceEvent.cshtml` | pending | |
| Mix started | `Web/Views/Server/GameEvents/_MixStartedEvent.cshtml` | pending | |
| Pause and unpause | `Web/Views/Server/GameEvents/_PauseEvent.cshtml` | pending | |
| Player death | `Web/Views/Server/GameEvents/_PlayerDeathEvent.cshtml` | pending | |
| Round ended, with score and difference | `Web/Views/Server/GameEvents/_RoundEndedEvent.cshtml` | pending | |
| Round live, first or second | `Web/Views/Server/GameEvents/_RoundLiveEvent.cshtml` | pending | |
| Skeet (melee, sniper, grenade, plain) and team skeet | `Web/Views/Server/GameEvents/_SkeetEvent.cshtml` | pending | |
| Skeet with chip damage and overkill note | `Web/Views/Server/GameEvents/_SkeetHurtEvent.cshtml` | pending | |
| Tank became a bot | `Web/Views/Server/GameEvents/_TankBecameBotEvent.cshtml` | pending | |
| Tank died | `Web/Views/Server/GameEvents/_TankDiedEvent.cshtml` | pending | |
| Tank rock hit | `Web/Views/Server/GameEvents/_TankRockEatenEvent.cshtml` | pending | |
| Tank rock skeeted | `Web/Views/Server/GameEvents/_TankRockSkeetedEvent.cshtml` | pending | |
| Tank spawned | `Web/Views/Server/GameEvents/_TankSpawnedEvent.cshtml` | pending | |
| Tongue cut | `Web/Views/Server/GameEvents/_TongueCutEvent.cshtml` | pending | |
| Witch crown | `Web/Views/Server/GameEvents/_WitchCrownEvent.cshtml` | pending | |
| Witch draw crown | `Web/Views/Server/GameEvents/_WitchCrownHurtEvent.cshtml` | pending | |

## Terminal

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Modal with the command preview and the option list | `Web/Views/Server/Index.cshtml:923` | pending | |
| The 24 commands with their descriptions and arguments | `Web/Views/Server/Index.cshtml:324` | pending | |
| Argument lists: wildcards, survivors, infected, spectators, players, team, minutes, reason, match mode, anti-cheat state, campaign | `Web/Views/Server/Index.cshtml:422` | pending | |
| Campaign list loaded from `/api/campaigns` | `Web/Views/Server/Index.cshtml:180` | pending | |
| Step by step selection with "Back", values with spaces quoted | `Web/Views/Server/Index.cshtml:617` | pending | |
| Confirmation before running, then sent through the chat form | `Web/Views/Server/Index.cshtml:761` | pending | |
| A command typed in the chat by an administrator runs on the game server | `Web/Controllers/Api/ExternalChatController.cs:43` | pending | |

## Server info modal

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Modal "Server info" opened from the header | `Web/Views/Server/Index.cshtml:942` | pending | |
| IP row with the "Recommended" label, `connect <ip>` and copy icon | `Web/Views/Server/Index.cshtml:954` | pending | |
| DNS row with the "Alternative" label, `connect <dns>` and copy icon | `Web/Views/Server/Index.cshtml:966` | pending | |
| Explanation of when to use the DNS | `Web/Views/Server/Index.cshtml:980` | pending | |

## Live state rules

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Everything from the game is shown 15 s late | `Core/GameInfo/GameInfo.cs:28` | pending | |
| Configuration, round and players expire after 2 h, the scoreboard after 10 min | `Core/GameInfo/GameInfo.cs:33` | pending | |
| Feed keeps items for 1 h, 250 at most | `Core/GameInfo/GameInfo.cs:27`, `Core/GameInfo/Structures/TimedFeed.cs:5` | pending | |
| Chat binds are dropped ("gogogo", "tank tank", "r"...) | `Core/GameInfo/Filters/ChatBindBlacklist.cs` | pending | |
| A player's message equal to the previous one is dropped | `Core/GameInfo/GameInfo.cs:122` | pending | |
| Site message: 200 characters at most | `Core/GameInfo/GameInfo.cs:154` | pending | |
| Site message: 15 s between any two, 30 s per visitor; administrators are exempt | `Core/GameInfo/GameInfo.cs:161` | pending | |
| Site messages kept 1 h for the game server to read | `Core/GameInfo/GameInfo.cs:96` | pending | |
| Site message appears in the feed at once, as team "External" | `Core/GameInfo/GameInfo.cs:175`, `Core/GameInfo/Models/ExternalChatMessage.cs:17` | pending | |
| Latency: low under 100 ms, medium under 200 ms, high otherwise | `Core/GameInfo/Models/Player.cs:25` | pending | |
| Health colour: red when incapacitated or under 25, orange under 40, green otherwise; maximum 100, or 300 incapacitated | `Core/GameInfo/Models/Survivor.cs:11` | pending | |
| Scoreboard values (percentages, difference, leader) | `Core/GameInfo/Models/Scoreboard.cs:13` | pending | |
| Tank and witch disabled at 0 % | `Core/GameInfo/Models/Round.cs:14` | pending | |
| Event stars per type; a team skeet or a bot hunter gives one | `Core/GameInfo/Extensions/GameEventTypeExtensions.cs`, `Core/GameInfo/Models/Events/SkeetEvent.cs:11` | pending | |
| Server command format (`!`, `/` or `sm_`, 256 characters at most) | `Core/GameInfo/Models/ServerCommand.cs:7` | pending | |
| Player connections: saved per address, related accounts returned, cleaned up | `Core/GameInfo/Services/PlayerConnectionInfoService.cs` | pending | |
| Server name, map, players and slots read from Steam every 10 s | `Core/Steam/ServerInfo/Services/Cache/ServerInfoServiceCached.cs` | pending | |

## Game server and public API

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| `PUT /api/game-info/configuration`, `round`, `scoreboard`, `players`, `messages` (game server) | `Web/Controllers/Api/GameInfoController.cs:33` | pending | |
| `POST /api/game-info/events` (game server) | `Web/Controllers/Api/GameInfoController.cs:123` | pending | |
| `POST /api/game-info/player-connection-info` (game server) | `Web/Controllers/Api/GameInfoController.cs:132` | pending | |
| `GET /api/game-info/server-command/dequeue` (game server) | `Web/Controllers/Api/GameInfoController.cs:144` | pending | |
| `GET /api/game-info` and `/configuration`, `/round`, `/scoreboard`, `/players`, `/messages`, `/events` (public) | `Web/Controllers/Api/GameInfoController.cs:19` | pending | |
| `GET /api/external-chat?after=` | `Web/Controllers/Api/ExternalChatController.cs:20` | pending | |
| `POST /api/external-chat` (signed-in visitor) | `Web/Controllers/Api/ExternalChatController.cs:28` | pending | |
| `GET /api/server` | `Web/Controllers/Api/ServerController.cs:9` | pending | |
| `GET /api/servers` redirects to `/api/server` | `Web/Controllers/Api/ServerController.cs:17` | pending | |
| `GET /api/campaigns` | `Web/Controllers/Api/CampaignController.cs` | pending | |

## Layout hooks

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Navbar badge "● players/slots", pulsing, linked to the server page, hidden with nobody connected | `Web/Views/Shared/_Layout.cshtml:322` | pending | |
| Browser title prefixed with "🟢 players/slots" | `Web/Views/Shared/_Layout.cshtml:602` | pending | |
| Chat row shown or hidden by the same check, every 4 s | `Web/Views/Shared/_Layout.cshtml:594` | pending | |
| Floating connect icon, hidden on phones | `Web/Views/Shared/_Layout.cshtml:526` | pending | |

## Chat bans page

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Administrators only | `Web/Controllers/ChatBansController.cs:10` | pending | |
| Header "Chat Bans" with the "Add ban" button | `Web/Views/ChatBans/Index.cshtml:35` | pending | |
| Lead text | `Web/Views/ChatBans/Index.cshtml:42` | pending | |
| Counter of banned players | `Web/Views/ChatBans/Index.cshtml:44` | pending | |
| Search by name, Steam ID or reason, with "Clear filters" | `Web/Views/ChatBans/Index.cshtml:51`, `Web/Controllers/ChatBansController.cs:25` | pending | |
| List item: avatar, name, Steam ID and SteamID64, reason in two lines ("No reason given") | `Web/Views/ChatBans/Index.cshtml:67` | pending | |
| List item: ban date and "by <administrator>" | `Web/Views/ChatBans/Index.cshtml:83` | pending | |
| Empty list text | `Web/Views/ChatBans/Index.cshtml:63` | pending | |
| Pagination, 20 per page, keeping the search | `Web/Views/ChatBans/Index.cshtml:94`, `Web/Controllers/ChatBansController.cs:14` | pending | |
| Details modal: avatar, name linked to the player page, IDs, Steam profile link | `Web/Views/ChatBans/Index.cshtml:96` | pending | |
| Details modal: ban date, updated at, banned by | `Web/Views/ChatBans/Index.cshtml:115` | pending | |
| Details modal: edit and save the reason | `Web/Views/ChatBans/Index.cshtml:123`, `:245` | pending | |
| Details modal: remove with a second confirmation | `Web/Views/ChatBans/Index.cshtml:127`, `:260` | pending | |
| Add modal: Steam ID or profile URL field | `Web/Views/ChatBans/Index.cshtml:153` | pending | |
| Add modal: "Choose from the list" opens the player picker | `Web/Views/ChatBans/Index.cshtml:158`, `:334` | pending | |
| Add modal: reason | `Web/Views/ChatBans/Index.cshtml:165` | pending | |
| Add modal: recent senders with avatar, name, last message and "Already banned" | `Web/Views/ChatBans/Index.cshtml:169`, `:284` | pending | |
| Success and error messages, then reload | `Web/Views/ChatBans/Index.cshtml:201` | pending | |
| Styles (`_AdminStyles`, reason clamp, round avatar) | `Web/Views/ChatBans/Index.cshtml:16` | pending | |

## Chat bans API and rules

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| `GET /api/chat-bans` (administrator) | `Web/Controllers/Api/ChatBansController.cs:16` | pending | |
| `GET /api/chat-bans/recent-senders` (administrator) | `Web/Controllers/Api/ChatBansController.cs:25` | pending | |
| `POST /api/chat-bans` creates or updates a ban (administrator) | `Web/Controllers/Api/ChatBansController.cs:34` | pending | |
| `DELETE /api/chat-bans/{communityId}` (administrator) | `Web/Controllers/Api/ChatBansController.cs:51` | pending | |
| The player is given as SteamID64, Steam ID, Steam3 or profile URL | `Core/ChatBans/Services/ChatBanService.cs:82` | pending | |
| A new ban keeps who created it and when; an update keeps them and sets "updated at" | `Core/ChatBans/Services/ChatBanService.cs:89` | pending | |
| The name is replaced only when a new one is given | `Core/ChatBans/Services/ChatBanService.cs:109` | pending | |
| Bans listed newest first | `Core/ChatBans/Services/ChatBanService.cs:39` | pending | |
| Recent senders: the last message of each visitor, newest first, 30 at most, flagged when already banned | `Core/ChatBans/Services/ChatBanService.cs:52` | pending | |
| A banned visitor cannot send: HTTP 403 with the reason | `Web/Controllers/Api/ExternalChatController.cs:32` | pending | |

## Hub-only pages

These did not exist in the old site. They are listed so that nothing the hub had is lost in the port.

| Item | Where in the hub | Status | Notes |
|---|---|---|---|
| Server picker at `/servers` when more than one server is enabled | `Views/Servers/Index.cshtml` | pending | |
| Server switch on the live page when more than one server is enabled | `Views/Servers/Live.cshtml` | pending | |
| Player connections page (administrator) | `Views/PlayerConnections/Index.cshtml` | pending | |
| Remote commands page with the audit log (administrator) | `Views/RemoteCommands/Index.cshtml` | pending | |
| Public JSON of one server (`/api/servers/{publicId}/game-info...`) | `Controllers/Api/LiveApiController.cs` | pending | |
