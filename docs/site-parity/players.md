# Site parity: players

🌐 **English** | [Português](players.pt.md) | [Español](players.es.md)

Port of the old site's players area (`Views/Players/*`, `PlayersController`, `Api/PlayersController`, `Api/PunishmentsController`, the player models and page metadata) to the hub.

Paths in the second column are relative to `L4D2PlayStats.Web/L4D2PlayStats.Web` in the old repository unless they start with `Core/` or `Sdk/` (the old site's other projects) or `api/` (the old API repository).

Status: `done` (same content and look), `adapted` (present, changed for the stated reason), `not ported` (cannot exist in the hub).

Checked against the rendered pages of a copy of the production data and against https://l4d2.com.br (`/players`, `/player/76561199157667941`, `/player/76561199157667941/76561199429077716`, `/player/76561190000000001`), at 1280 px and at phone width, in English, in the dark theme and (HTML only) in the light theme. The count of rows, cells, tooltips, copy buttons, charts, modals, relation cards and campaign cards of the old and the new HTML is the same for the same data.

## Routes and API

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| `GET /players` (list) | Controllers/PlayersController.cs:26-39 | done | Same address. |
| Query `orderBy` (one of 25 statistics, default `Experience`) | Controllers/PlayersController.cs:27, Core/Players/Enums/PlayerResultProperty.cs | done | Same names and order (`Models/Players/PlayerResultProperty.cs`). An unknown value falls back to `Experience`. Without `orderBy` the page now shows the Overview, the former history table, ordered with `sort`. |
| Query `asc` (default `false`) | Controllers/PlayersController.cs:27 | done | |
| Menu item "Players" (under "More") active on the list | Controllers/PlayersController.cs:29 | done | `ViewBag.Players = "active"`; not set on the player page, as before. |
| `GET /player/{communityId}/{compareWith?}` (player and comparison) | Controllers/PlayersController.cs:41-97 | done | Same address. |
| `communityId <= 0` shows the "not ranked" page | Controllers/PlayersController.cs:48-49 | adapted | An address that is not a valid SteamID64 (`/player/0`, `/player/null`, `/player/5`) answers 404. Every valid SteamID64 gets the "not ranked" page, also when the ranking module is off. |
| A player outside the ranking shows the "not ranked" page with the connection information (HTTP 200) | Controllers/PlayersController.cs:55-64 | done | The hub's first version answered 404 here. |
| `compareWith` that is not in the ranking is ignored | Controllers/PlayersController.cs:53 | done | |
| Ranking of the current season as the source of both pages | Controllers/PlayersController.cs:31, :51, Core/Ranking/Services/RankingServiceCached.cs:11-19 | adapted | The old API call returned only the first 100 players of the ranking, so the list stopped at 100 and player 101 got the "not ranked" page. The hub uses the whole ranking of the season. |
| Query `season` (new) | - | adapted | Hub addition: both pages accept `?season={id}`, `?year={year}` or `?period=all-time` to show a past period. An unknown season answers 404. Without it the current season is shown, as before. |
| Avatars loaded before rendering | Controllers/PlayersController.cs:33, :60-61, :66-69, :85 | adapted | Avatars are served by `/avatar/{communityId}` and downloaded on demand; no preload call is needed. |
| Last matches: the 50 most recent matches of the season, any game mode, filtered to the ones where either player is on a team | Controllers/PlayersController.cs:77, Core/Matches/MatchesServiceCached.cs:19, Models/PlayerDetailsModel.cs:22-32 | done | Same rule, over the displayed season and every game server (`IPlayerMatchService`, `AppConsts.PlayerRecentMatches`). |
| Related accounts from the connection information | Controllers/PlayersController.cs:78, Core/GameInfo/Services/PlayerConnectionInfoService.cs:75-108 | adapted | `IPlayerConnectionService.RelatedAsync`. Empty when the live server module is off, and for visitors who are not administrators when Settings > Privacy limits related accounts to administrators (the default is everyone, as before). |
| Owned skins and wallet of each player | Controllers/PlayersController.cs:87-93, :99-106 | done | Only read when the skin shop module is on. |
| Patent progress of each player | Controllers/PlayersController.cs:71-75 | done | `RankingModel` / `PatentProgressModel` of the base. |
| `POST /api/punishments` (administrator, body `{ communityId, lostExperiencePoints }`) | Controllers/Api/PunishmentsController.cs:15-24, api Core/Modules/Punishments/Services/PunishmentsService.cs:14-31 | adapted | Same address, verb and body; points are added to what the player already lost, as before. The body also takes `seasonId` (the displayed season; the current one when missing) and the call asks for the antiforgery token in the `RequestVerificationToken` header. The answer carries the player's total for the season. |
| `DELETE /api/punishments/{communityId}` (administrator) | Controllers/Api/PunishmentsController.cs:26-35 | adapted | Same address and verb, 204 answer. Takes `?season={id}` and the antiforgery header. |
| Punishment API: administrators only, signed in | Controllers/Api/PunishmentsController.cs:11, :16, :27 | done | 401 for a visitor, 403 for a player. |
| Punishment API: the caches are cleared so the ranking shows the new experience | Controllers/Api/PunishmentsController.cs:21, :32 | done | `HubState.DataChanged()` invalidates the cached rankings. |
| Punishment API: validation of the command | api Core/Modules/Punishments/Validations/PunishmentCommandValidator.cs | done | 400 for an invalid player or for points that are not positive. |
| `GET /api/players/patents` | Controllers/Api/PlayersController.cs:15-31 | done | Already in the hub (`RankingApiController`); not called by these pages. |
| `GET /api/players/skins`, `GET /api/players/{communityId}/wallet`, `POST /api/players/{communityId}/skins/equip` | Controllers/Api/PlayersController.cs:33-69 | done | Already in the hub (`PlayerSkinsApiController`); plugin and layout endpoints, not called by these pages. |
| Ranking cached for one hour | Core/Ranking/Services/RankingServiceCached.cs:13-18 | adapted | The hub caches the computed ranking in memory and drops it whenever the data changes, so the pages are never an hour behind. |

## Players list (`/players`)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Heading "Players" | Views/Players/Index.cshtml:63 | done | |
| Page styles (table, paragraph and header margins) | Views/Players/Index.cshtml:47-61 | done | |
| Dropdown "Order by: **statistic**" with the caret, now "Statistic: **statistic**", with an Overview entry | Views/Players/Index.cshtml:67-71 | done | |
| Dropdown items: the 25 statistics, each linking to `orderBy={statistic}&asc=False` | Views/Players/Index.cshtml:72-81 | done | The link keeps the displayed season. |
| Statistic names: Experience, Games, Wins, Loss, Rage quit, SI MVP awards, CI MVP awards, FF LVP awards, Deaths, Times incapacitated, SI damage taken, Commons killed, SI killed, Damage to SI, Damage to Tank, Rock hits taken, Total skeets, Melee skeets, Total levels, Total crowns, FF dealt (non-incap.), SI damage (excl. Tank), Damage as Tank (upright), Spit damage (upright), Total pounce damage | Views/Players/Index.cshtml:15-27, Resources/SharedResource*.resx | done | `Helpers/PlayerProperties.cs`. Tank wipes were added after it. |
| Period selector (new) | - | adapted | Hub addition, next to the "Statistic" dropdown and in the style of the history page: "Displayed period: **From MM/YYYY to MM/YYYY**". It lists the seasons of the current year then one entry per year ("Year of YYYY"), which shows the ranking and the matches of that whole year, and "All time". Only shown when there is more than one choice. |
| Table `table-striped table-hover table-bordered table-condensed` | Views/Players/Index.cshtml:86 | done | |
| Column `#` (slim, centered) | Views/Players/Index.cshtml:89, :113 | done | |
| Column "Player" over two cells (avatar and name) | Views/Players/Index.cshtml:90 | done | |
| Column with the chosen statistic, linking to the inverted order, with the arrow (↑ ascending, ↓ descending) | Views/Players/Index.cshtml:91-97 | done | |
| Column "Per game", hidden when the statistic is "Games" | Views/Players/Index.cshtml:44, :98-101, :131-139 | done | |
| Position: 1, 2, 3... descending; counted down from the number of players when ascending | Views/Players/Index.cshtml:105-107, :142 | done | |
| Pages of 100 players (new) | - | adapted | Hub addition: the list shows 100 players per page with the page links under the table, which keep `orderBy`, `asc` and the period. The position goes on from the previous page, in both orders. With up to 100 players there are no page links and the page is as before. |
| Row highlighted (`it-is-me`) for the signed-in player | Views/Players/Index.cshtml:112 | done | |
| Avatar 40 px, rounded, with link, title and the fallback image on error | Views/Players/Index.cshtml:114-118 | done | |
| Name truncated (`TruncatePlayerName`) with link to the player page | Views/Players/Index.cshtml:119-121 | done | The link keeps the displayed season. |
| Value of the statistic (`N0`) | Views/Players/Index.cshtml:124 | done | |
| Help icon with tooltip on the value for "SI damage taken", "SI damage (excl. Tank)" and "Total pounce damage" | Views/Players/Index.cshtml:29-38, :125-128 | done | Same three texts; `aria-label` "More information". |
| Average per game (`N2`, `-` without games) | Views/Players/Index.cshtml:135, Core/Players/Player.cs:26 | done | |
| Info icon beside the average with the tooltip "Games: n" | Views/Players/Index.cshtml:136 | done | |
| Sort: by value, ties by community id, both directions | Core/Players/Extensions/PlayerResultExtensions.cs:13-15 | done | `PlayerProperties.Sort`; covered by a test. |
| Page metadata: title "Players" | PageMetadata/PlayersPageMetadata.cs:14 | done | |
| Page metadata: description "Browse statistics for {n} players from the ... server." | PageMetadata/PlayersPageMetadata.cs:15 | adapted | The text named "L4D2 Competitive"; it now takes the site name (new key in the `players` resource fragments). |
| Page metadata: image and alt text of the first player of the list | PageMetadata/PlayersPageMetadata.cs:12, :16-17 | done | |

## Player page (`/player/{communityId}/{compareWith?}`): frame

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Heading "Player details" | Views/Players/Details.cshtml:584 | done | |
| Period selector (new) | - | adapted | Hub addition under the heading, same dropdown as on the list. It keeps the compared player and the open tab. |
| Left column (`col-xs-2 left-side`): dropdown with the player's name (10 characters) and caret | Views/Players/Details.cshtml:587-592 | done | |
| Left dropdown list: every other ranked player as "#position - name", switching the first player and keeping the comparison | Views/Players/Details.cshtml:593-600 | done | Scrollable list of 175 px (`players-dropdown-menu`). |
| First player's avatar (`img-thumbnail`, opens enlarged with lity) | Views/Players/Details.cshtml:603 | done | |
| Separator line | Views/Players/Details.cshtml:605 | done | |
| Second dropdown in yellow: the compared player's name or "Compare..." | Views/Players/Details.cshtml:607-611 | done | |
| Second dropdown: "Remove comparison" with the remove icon and a divider, when comparing | Views/Players/Details.cshtml:613-621 | done | |
| Second dropdown list: every other ranked player in yellow, choosing the player to compare with | Views/Players/Details.cshtml:622-629 | done | |
| Second player's avatar with the yellow frame, opens enlarged | Views/Players/Details.cshtml:632-635 | done | |
| Tabs: General, Campaigns (badge with the count), Skins (badge with both players' skins), Relations (badge, only when there are relations) | Views/Players/Details.cshtml:639-661 | adapted | The Skins tab is only there while the skin shop module is on. The rest is the same. |
| Styles of the page (skins, badges, headers, progress, dropdown height, yellow `second`, separator, previous names, streak tiles, tabs, campaign cards and bars) | Views/Players/Details.cshtml:187-500 | done | Copied as they were; one rule added for a skin without a thumbnail. |
| Mobile (max 700 px): smaller tabs, both columns at full width, `mobile-row-label` rows of the head-to-head shown as block labels | Views/Players/Details.cshtml:476-499 | done | |
| Tables as `table-responsive` with `data-label` on every cell (label before the value on a phone) | Views/Players/Details.cshtml:671-1233 | done | |
| Script: opening a tab writes its hash in the address and triggers a resize (so the lazy charts load) | Views/Players/Details.cshtml:549-554 | done | |
| Script: the tab named in the address hash opens on load | Views/Players/Details.cshtml:556-557 | done | |
| Script: links of the player dropdowns and of the relation cards keep the open tab | Views/Players/Details.cshtml:559-562 | done | The period selector does the same. |
| Page metadata: title "{name} - Player details", or "{first} vs {second}" when comparing | PageMetadata/PlayerDetailsPageMetadata.cs:16-18 | done | |
| Page metadata: description "See {name}'s statistics: rank #{n}, {n} matches, and {n} wins." | PageMetadata/PlayerDetailsPageMetadata.cs:20-21 | done | |
| Page metadata: description of a comparison "Compare {first} and {second}'s performance and statistics on ..." | PageMetadata/PlayerDetailsPageMetadata.cs:22 | adapted | The text named "L4D2 Competitive"; it now takes the site name. |
| Page metadata: first player's avatar as image, name as alt text, Open Graph type `profile` | PageMetadata/PlayerDetailsPageMetadata.cs:24-29 | done | |

## Player page: General tab, "General statistics"

Every value is followed by ` | ` and the second player's value in yellow when comparing, as in the old page.

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Panel "General statistics" | Views/Players/Details.cshtml:666-669 | done | |
| Ranking (`#position`) | Views/Players/Details.cshtml:674-682 | done | |
| Steam ID with the copy icon and the "Text copied!" popover | Views/Players/Details.cshtml:684-700 | done | Same field, icon (dark or light by theme) and behaviour, written as `STEAM_0:x:y` like the old site. |
| Name (second player on a new line) | Views/Players/Details.cshtml:703-711 | adapted | The display name of the player registry (`MainName`): the forced name, else the most used name, else the Steam name. |
| Profile URL as a link in a new tab, with the copy button | Views/Players/Details.cshtml:713-733 | done | |
| Previous names: up to 5 labels with tooltip; the second player's in yellow labels after the separator; the row only exists when the first player has names | Views/Players/Details.cshtml:735-754 | done | |
| Patent: image of 32 px with the tooltip "{name} - Level nn - patent" (row hidden on a phone) | Views/Players/Details.cshtml:755-764 | done | |
| XP: progress bar in two parts (progress and last change) with the HTML tooltip (level up or drop, last game, next level, XP, progress); a second bar for the compared player | Views/Players/Details.cshtml:766-784, Models/RankingModel.cs | done | |
| Games | Views/Players/Details.cshtml:787-795 | done | |
| Wins | Views/Players/Details.cshtml:796-804 | done | |
| Rage quit | Views/Players/Details.cshtml:805-813 | done | |
| Win rate (`P0`) | Views/Players/Details.cshtml:816-824 | done | |
| Loss | Views/Players/Details.cshtml:825-833 | done | |
| Punishment ("0" or "-n XP") | Views/Players/Details.cshtml:35-38, :834-836, :848 | done | |
| Administrator: "Edit" button beside each punishment (tooltip below) | Views/Players/Details.cshtml:838-843, :850-855 | done | |
| Administrator: the button asks "How many experience points should the user lose?" with 150 suggested | Views/Players/Details.cshtml:530-531, :840 | done | |
| Administrator: a positive number adds the punishment, 0 removes it, anything else does nothing; the page reloads afterwards | Views/Players/Details.cshtml:505-546 | adapted | Same behaviour. The calls also send the displayed season and the antiforgery token. |
| Longest win streak | Views/Players/Details.cshtml:860-868 | done | |
| Longest loss streak | Views/Players/Details.cshtml:869-877 | done | |
| Match history: one tile per result (green win, red loss, grey draw) with tooltip; a dash when empty; with the names above each row when comparing | Views/Players/Details.cshtml:40-75, :880-887 | done | |
| Comparison: "Playing together" row (games, wins with win rate, losses with loss rate), only when they played together | Views/Players/Details.cshtml:889-909 | done | |
| Comparison: "Playing against" row (games, each player's wins and losses with rates, second player in yellow), only when they faced each other | Views/Players/Details.cshtml:911-937 | done | |

## Player page: General tab, "Related accounts"

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Panel (`panel-warning`) only when another account connected from the same address | Views/Players/Details.cshtml:943-948 | adapted | Same panel. It is shown to every visitor by default, as before; the setting Settings > Privacy > "Who can see related accounts" can limit it to administrators. It needs the live server module, which is where the connections are recorded. |
| Columns: Name, Steam ID, Last login, Connections, Profile URL | Views/Players/Details.cshtml:951-959 | done | |
| Avatar of 32 px and name linking to that account's page | Views/Players/Details.cshtml:964-967 | done | |
| Steam ID with the copy button | Views/Players/Details.cshtml:968-974 | done | `STEAM_0:x:y` form, as above. |
| Last login (`g` format) | Views/Players/Details.cshtml:975 | adapted | The hub stores the moment in UTC and shows it in the site's time zone; the old site showed its server's local time. |
| Number of connections | Views/Players/Details.cshtml:976 | done | |
| Profile URL as a link (new tab, `noopener noreferrer`) with the copy button | Views/Players/Details.cshtml:977-985 | done | |

## Player page: General tab, "Statistics as survivor" and "Statistics as infected"

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Panel "Statistics as survivor" | Views/Players/Details.cshtml:993-996 | done | |
| SI MVP awards, CI MVP awards, FF LVP awards | Views/Players/Details.cshtml:999-1027 | done | |
| SI killed, Commons killed, Damage to SI | Views/Players/Details.cshtml:1029-1057 | done | |
| Deaths, Times incapacitated, SI damage taken (with the help icon and tooltip) | Views/Players/Details.cshtml:1059-1090 | done | |
| Damage to Tank, Rock hits taken, FF dealt (non-incap.) | Views/Players/Details.cshtml:1092-1121 | done | |
| Total skeets, Melee skeets, Total levels | Views/Players/Details.cshtml:1123-1153 | done | |
| Total crowns, row only when either player has crowns | Views/Players/Details.cshtml:1155-1172 | done | |
| Panel "Statistics as infected" | Views/Players/Details.cshtml:1177-1180 | done | |
| SI damage (excl. Tank) with the help icon and tooltip; Damage as Tank (upright) | Views/Players/Details.cshtml:1183-1206 | done | |
| Spit damage (upright); Total pounce damage with the help icon and tooltip | Views/Players/Details.cshtml:1208-1231 | done | |

## Player page: General tab, "Last matches"

The card is the old `Views/LastMatches/_Match.cshtml`. The player page renders the shared matches card, `Views/Matches/_Match.cshtml`, the same one the match list uses; it no longer has its own copy.

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Heading "Last matches" and the list, only when there are matches | Views/Players/Details.cshtml:1236-1246 | done | |
| Card header: campaign, game mode in italics, label "Competitive" (green) or "Casual" (grey) | Views/LastMatches/_Match.cshtml:157-171 | adapted | "Competitive" is a match that counts for the ranking by the hub's game mode rules; the old rule was fixed at two or more maps and teams of four. |
| "Team X won by n points" | Views/LastMatches/_Match.cshtml:172-174 | done | |
| Calendar icon with "Today at", "Yesterday at" or date and time, and its tooltip | Views/LastMatches/_Match.cshtml:176-193 | done | Shown in the site's time zone. |
| Clock icon with the duration (`hh:mm:ss`) and its tooltip | Views/LastMatches/_Match.cshtml:195-201 | done | |
| Campaign cover (opens enlarged) | Views/LastMatches/_Match.cshtml:208, Core/Campaign/CampaignThumb.cs | done | Same images and file names; a campaign without a cover falls back to `no-image.jpg`. |
| "Details" button | Views/LastMatches/_Match.cshtml:209-214 | adapted | Links to the hub's match page, `/matches/{id}`. |
| Two team panels (green winner, red loser) with "Team X" and the score | Views/LastMatches/_Match.cshtml:217-232 | done | |
| Players of each team ordered by MVP awards, with avatar, name with link and the row highlighted for the signed-in player | Views/LastMatches/_Match.cshtml:233-250 | done | The links keep the displayed season. |
| Yellow star per SI MVP and blue star per CI MVP, with tooltips, after a separator | Views/LastMatches/_Match.cshtml:251-264 | done | |
| Chart "Scoreboard" (line): one series per team with the accumulated score per map, green for the winner and red for the loser, legend below | Views/LastMatches/_Match.cshtml:24-78, :278-289 | done | Same Chart.js configuration, loaded lazily through `.app-chart` + `data-chart`; font colour by theme. |
| Chart "Points earned per map" (bars): one series per team with the points of each map | Views/LastMatches/_Match.cshtml:80-153, :291-302 | done | |
| Map names on the axis truncated to 16 characters | Views/LastMatches/_Match.cshtml:31, :85 | done | |
| Mobile: cover and teams stack at full width | Views/Shared/_Layout.cshtml (`.match .left-side`, `.match .right-side`) | done | Rules of the ported layout. |

## Player page: Campaigns tab

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| "No campaign statistics for this player yet." when empty | Views/Players/Details.cshtml:1250-1253 | done | |
| Help line with the info icon | Views/Players/Details.cshtml:1256-1258 | done | |
| Grid of campaign cards, most played first (then by the first player's games, then by name); campaigns without a name left out | Views/Players/Details.cshtml:1259-1283, Models/PlayerDetailsModel.cs:104-120 | done | |
| Card: cover, campaign name, "n Games" on the right (single player only) | Views/Players/Details.cshtml:1262-1271 | done | |
| Per player: bar in three parts (wins, draws, losses) with tooltips | Views/Players/Details.cshtml:77-80, :101-105 | done | |
| Per player: three boxes with wins, draws and losses, each with its percentage | Views/Players/Details.cshtml:106-119 | done | |
| Per player: "Rage quit: n" when there is any | Views/Players/Details.cshtml:120-123 | done | |
| Comparison: each player's name with "· n Games" above the bar; "Has not played it yet" for the one who has no games | Views/Players/Details.cshtml:85-98, :1273-1279 | done | |
| Games, win, loss and draw rates of a campaign | Sdk/Ranking/Results/PlayerCampaignResult.cs:9-18 | done | Added to the hub's `PlayerCampaign`. |

## Player page: Skins tab (skin shop module)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Panel heading "Skins" with a badge per player (yellow for the second) | Views/Players/Details.cshtml:1288-1295 | done | |
| Link "See the skin shop" | Views/Players/Details.cshtml:1296 | done | |
| Per player: name with the number of skins (when comparing) | Views/Players/Details.cshtml:148-156 | done | |
| Per player: wallet pill "★ n star(s) available · n earned, n spent" | Views/Players/Details.cshtml:128-141, :157 | done | |
| "This player does not own any skins yet." | Views/Players/Details.cshtml:160-163 | done | |
| Owned skins, rarest and most expensive first: thumbnail, localized title, link to the skin in the shop (`/skins#skin-{id}`) | Views/Players/Details.cshtml:166-181, Models/PlayerSkinsModel.cs:8-14 | adapted | Thumbnails come from the address configured for the skin assets (they are not bundled with the hub); without one a grey box of the same size is shown. |
| Green frame and label "Equipped" on the equipped skins | Views/Players/Details.cshtml:170-178 | done | |

## Player page: Relations tab

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Heading "Player relationships" with the player names | Views/Players/Details.cshtml:1383-1394 | done | |
| One card per player met, the compared player first, then by games and by wins | Views/Players/Details.cshtml:1400-1403, Models/PlayerDetailsModel.cs:55-69, :71-96 | done | |
| Card in yellow (`panel-warning`) for the compared player | Views/Players/Details.cshtml:1403 | done | |
| Card heading: name (link that switches the page to that player, keeping the comparison, when ranked), "n Games" | Views/Players/Details.cshtml:1408-1416 | adapted | A player who is not in the ranking was shown by the community id; the hub shows the last known name when it has one. |
| Help button (question mark, tooltip "I don't get it, explain it to me") | Views/Players/Details.cshtml:1405-1407 | done | |
| Avatar | Views/Players/Details.cshtml:1420-1422 | done | |
| Table: "Playing together" and "Playing against" with Games, Wins and Loss, counts and rates with tooltips, a column per player when comparing (second in yellow) | Views/Players/Details.cshtml:1313-1335, :1424-1455 | done | |
| Help modal per card: "Understanding the numbers", the three sentences with the names in colour and the numbers in bold, separated by a line for the second player, "Close" | Views/Players/Details.cshtml:1337-1373, :1460-1493 | done | |
| Only 12 cards at first; button "Show more players" / "Hide" toggles the rest | Views/Players/Details.cshtml:564-579, :1402, :1498-1507, Core/AppConsts.cs:7 | done | `AppConsts.PlayerRelationsVisible`. |

## Not ranked page (`Views/Players/PlayerNotFound.cshtml`)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Heading "Player details" | Views/Players/PlayerNotFound.cshtml:36 | done | |
| Warning box: "Player not ranked" and the two explanation lines | Views/Players/PlayerNotFound.cshtml:38-42 | done | |
| Period selector (new) | - | adapted | Hub addition: a player who is not ranked in the displayed season may be ranked in another one. |
| Avatar (184 px at most, opens enlarged, fallback image) | Views/Players/PlayerNotFound.cshtml:45-47 | done | |
| Panel "Available information" | Views/Players/PlayerNotFound.cshtml:49-52 | done | |
| Name, when known | Views/Players/PlayerNotFound.cshtml:56-62 | adapted | Taken from the connection information, as before, or from the name the hub recorded in a past match. |
| Community ID with the copy button | Views/Players/PlayerNotFound.cshtml:63-69 | done | |
| Steam ID with the copy button | Views/Players/PlayerNotFound.cshtml:70-79 | done | `STEAM_0:x:y` form, as on the player page. |
| Last login, when known | Views/Players/PlayerNotFound.cshtml:80-86 | adapted | Site time zone, as above. |
| Connections, when known | Views/Players/PlayerNotFound.cshtml:87-93 | done | |
| Profile URL (new tab) with the copy button | Views/Players/PlayerNotFound.cshtml:94-105 | done | |
| Panel "Related accounts" with the same five columns as on the player page | Views/Players/PlayerNotFound.cshtml:112-160 | adapted | With the live server module, and following the same privacy setting as the player page. |
| Button "Return to ranking page" | Views/Players/PlayerNotFound.cshtml:162-164 | done | "Back to the home page" when the ranking module is off. |
| Button "Open Steam profile" under the avatar, Steam ID in the `STEAM_1` form, Steam3 ID (new) | - | adapted | Hub addition: every player link of the site opens this page instead of the Steam profile, so the page offers the way to Steam. |
| Warning "No statistics yet" when the ranking module is off (new) | - | adapted | Hub addition. |
| Styles (avatar size, header cells, phone margin) | Views/Players/PlayerNotFound.cshtml:15-34 | done | |
| Page metadata: title "{name or id} - Player details" | PageMetadata/PlayerNotFoundPageMetadata.cs:14 | done | |
| Page metadata: description "See the available information about player {name} on ..." | PageMetadata/PlayerNotFoundPageMetadata.cs:15 | adapted | The text named "L4D2 Competitive"; it now takes the site name. |
| Page metadata: avatar as image, Open Graph type `profile`, not indexed | PageMetadata/PlayerNotFoundPageMetadata.cs:16-19 | done | |

## Models and data

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| `PlayersModel` (players, order, direction) | Models/PlayersModel.cs | done | `Models/Players/PlayersModel.cs`, plus the displayed period. |
| `Player` row of the list (value by statistic, games, average) | Core/Players/Player.cs | done | `Models/Players/PlayerListRow.cs` (the name `Player` is taken by the statistics package). |
| `PlayerDetailsModel` (rankings, players, last matches, head-to-head, relations, campaigns, skins, related accounts) | Models/PlayerDetailsModel.cs | done | `Models/Players/PlayerDetailsModel.cs`. |
| `PlayerRelationModel`, `PlayerRelationRowModel` | Models/PlayerRelationModel.cs, Models/PlayerRelationRowModel.cs | done | |
| `PlayerCampaignRowModel` | Models/PlayerCampaignRowModel.cs | done | |
| `PlayerSkinsModel` | Models/PlayerSkinsModel.cs | done | |
| `PlayerNotFoundModel` | Models/PlayerNotFoundModel.cs | done | |
| `PlayerPatentModel` | Models/PlayerPatentModel.cs | done | Shape of `GET /api/players/patents`, already in the hub. |
| `PlayerResult` and its statistics, names, relations, campaigns, results and streaks | Sdk/Ranking/Results/*.cs | done | `RankingPlayer` of the base. |
| `PunishmentCommand`, `PunishmentResult` | Sdk/Punishments/* | adapted | `Models/Players/PunishmentCommand.cs`, with the optional `seasonId`. The answer keeps `communityId` and `lostExperiencePoints`; `server` gave way to `seasonId`, because in the hub a punishment belongs to a season and not to a game server. |
| `PlayerStarsResult` (stars of a player) | Sdk/Stars/* | done | The wallet shown in the Skins tab (`SkinWallet`). |
