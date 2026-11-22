# Site parity: matches

🌐 **English** | [Português](matches.pt.md) | [Español](matches.es.md)

Port of the old site's matches area (`Views/LastMatches/*`, `LastMatchesController`, `Api/StatisticsController`, `Api/CampaignController`) to the hub.

Paths in the second column are relative to `L4D2PlayStats.Web/L4D2PlayStats.Web` in the old repository unless they start with `Sdk/`, `Core/` or `api/` (the old API repository).

Status: `done` (same content and look), `adapted` (present, changed for the stated reason), `not ported` (cannot exist in the hub).

Texts are quoted in English, as the old site showed them with the English language selected.

How it was checked: the details page and the statistics page of one real match (The Parish, 12 Nov 2026) were rendered by the hub from a copy of the production statistics files and compared with the same pages of https://l4d2.com.br. Every element (tag, CSS classes and the presence of `title`, `alt`, `style`, `data-toggle`, `data-label`, `data-placement`, `colspan`, `role`, `tabindex`, `aria-label`) and every visible text was counted on both sides: 803 elements and 549 texts on the details page, 1,420 elements and 865 texts on the statistics page, with no difference. The list was compared by screenshot, at desktop and phone widths.

## Routes and API

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| `GET /last-matches` (list) | Controllers/LastMatchesController.cs:21 | adapted | The hub route is `/matches`. `/last-matches` answers with a permanent redirect to it. |
| `GET /last-matches/details/{start}/{end}` (match) | Controllers/LastMatchesController.cs:32 | adapted | The hub route is `/matches/{id}`. The old address is still answered: the two keys are turned back into file dates, the match is found and the visitor is redirected permanently. |
| `GET /last-matches/details/{start}/{end}/statistics/{statisticId}` (map) | Controllers/LastMatchesController.cs:47 | adapted | The hub route is `/matches/{id}/statistics/{fileId}`. The old address redirects permanently to it. |
| A match address keeps working over time | Sdk/Matches/Results/MatchResult.cs:16-17 | done | The old address was made of file keys, which never change. In the hub a match used to get a new id every time the matches of its night were rebuilt (a new map, a score correction, a full rebuild); the rebuild now keeps the id of a match that is still there. |
| 404 when the match does not exist | Controllers/LastMatchesController.cs:36, :52 | done | |
| 404 when the statistics file is not part of the match | Controllers/LastMatchesController.cs:57 | done | |
| Menu item "Last matches" active on the list only | Controllers/LastMatchesController.cs:23 | done | `ViewBag.LastMatches = "active"` on the list, not on the details pages, as before. |
| List limited to the last 50 matches, any game mode | Core/Matches/MatchesServiceCached.cs:19, api FunctionApp/Functions/MatchesFunction.cs:21 | adapted | 50 matches per page, newest first, any game mode, with pagination and every season; the old page stopped at 50 and only showed the current season. |
| List cached for one hour | Core/Matches/MatchesServiceCached.cs:15 | not ported | The hub reads the matches from its own database, so there is no remote API call to cache; the list is always up to date. |
| Avatars loaded before rendering | Controllers/LastMatchesController.cs:68-87 | adapted | Avatars are served by `/avatar/{communityId}` and downloaded on demand; no preload call is needed. |
| `PUT /api/statistics/{statisticId}/update-score` (administrator, body `{ team, score }`) | Controllers/Api/StatisticsController.cs:18-45, Commands/UpdateTeamScoreCommand.cs | adapted | Same address, verb and body. The id is the hub's file id. The request must also carry the antiforgery token in the `RequestVerificationToken` header, and the answer carries the file id, the match id and both scores instead of the whole statistics file. |
| update-score: 404 for an unknown file | Controllers/Api/StatisticsController.cs:23-24 | done | |
| update-score: 400 when the file has no scoring | Controllers/Api/StatisticsController.cs:26-30 | done | |
| update-score: only the given team changes, the other score is kept | Controllers/Api/StatisticsController.cs:32-33 | adapted | Same rule. A team other than A or B is answered with 400; the old endpoint saved the file again without changing it. |
| update-score: score between 0 and 10,000 | api Core/Modules/Statistics/Validations/UpdateScoreCommandValidator.cs | done | Answered with 400. |
| update-score: the caches are cleared so match and ranking show the new score | Controllers/Api/StatisticsController.cs:42 | done | The stored file is rewritten, the matches around it are rebuilt and the cached rankings are invalidated. |
| update-score: administrators only | Controllers/Api/StatisticsController.cs:14, :19 | done | A visitor is sent to sign in and a signed-in player gets 403. |
| `GET /api/campaigns` (name and maps of every campaign) | Controllers/Api/CampaignController.cs:10-13, Core/Campaign/CampaignName.cs:164-180 | done | Reads the campaigns registered in the hub (the same list by default, editable by the administrator). |

## Match numbers (old API)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Teams A and B: the players of each half of the last map, in the order of the file's player list | api Core/Modules/Statistics/Extensions/StatisticsExtensions.cs:53-54 | done | |
| Player name taken from the last map | api Core/Modules/Matches/Match.cs:163 | done | |
| Team score: the score of the last map | api Core/Modules/Matches/Match.cs:65 | done | |
| Per-player totals over every map and both halves (died, incaps, damage taken, commons, SI killed, SI damage, tank damage, rock eats, witch damage, skeets incl. hurt and melee, melee skeets, levels incl. hurt, crowns incl. hurt, FF given, infected damage, tank damage dealt, spit damage, high pounce damage) | api Core/Modules/Matches/Match.cs:104-146 | done | |
| MVP (SI), MVP (CI) and LVP (FF) counted once per half | api Core/Modules/Matches/Match.cs:110-134 | done | |
| Team totals: the sum of the team's players | api Core/Modules/Matches/Match.cs:69-94 | done | |
| Percentages: player value divided by the team total, zero when the total is zero | api Core/Modules/Matches/Match.cs:166-218 | done | |
| Winner: the team with the highest score; difference; draw | Sdk/Matches/Results/MatchResult.cs:19-29 | done | |
| Match start (start of the first map), end (end of the last map) and elapsed time | api Core/Modules/Statistics/Extensions/StatisticsExtensions.cs:58-65, Match.cs:11-13 | adapted | The start is the first round start of the first map, in UTC, as on the old site. The hub only accepts files with round times, so every match has it (all 1,832 matches of the production copy do). The old API fell back to the round date written in the file, a game server clock time, and the old site then shifted it as if it were UTC; for a match without a UTC start the hub shows the file date of its first map unchanged, the game server clock, and leaves the elapsed time out. |
| Times shown in the site's local time | Sdk/Converters/DateTimeConverter.cs:10 | adapted | The old site used the time zone of the machine; the hub uses the time zone chosen in the site settings. |
| Maps of the match, oldest first, with the score after each one | api Core/Modules/Matches/Match.cs:42-47, :218-223 | done | |
| "Competitive": two maps or more and teams of four | api Core/Modules/Matches/Match.cs:31 | adapted | The hub marks as competitive the matches that count for the ranking, following the game modes the administrator registered (by default the same rule). |
| Configuration name of the match | api Core/Modules/Matches/Match.cs:9 | done | |
| Statistics files of the match: every file of the server between the first and the last map, oldest first | api FunctionApp/Functions/StatisticsFunction.cs:64-84 | done | A repeated map that the match ignores is still listed, as before. |

## Last matches list (`Views/LastMatches/Index.cshtml`)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Page title "Last matches" | PageMetadata/LastMatchesPageMetadata.cs:17 | done | |
| Meta description "See the latest {site} matches, including scores, campaigns, and players." | PageMetadata/LastMatchesPageMetadata.cs:18 | adapted | The text named "L4D2 Competitive"; it now takes the site name (new text in `pt-BR.matches.json` and `es-ES.matches.json`). |
| Social image: thumbnail of the latest match's campaign, alt text with its name | PageMetadata/LastMatchesPageMetadata.cs:19-20 | done | |
| Heading `h4.text-center` "Last matches" | Views/LastMatches/Index.cshtml:13 | done | |
| `div.match` with one card per match | Views/LastMatches/Index.cshtml:15-20 | done | |
| Season filter | (new) | adapted | Hub addition, in the old look: a small select at the right, above the list ("All seasons" by default, then the seasons of the current year and one entry per year), so past seasons stay browsable. Seasons are named as in the period selectors of the old site, "From MM/YYYY to MM/YYYY". Hidden while there is a single season and a single server. |
| Server filter | (new) | adapted | Hub addition: a second select, shown only when there is more than one game server. |
| Pagination | (new) | adapted | Hub addition, with the shared `_Pagination` partial, shown when there is more than one page. |
| Empty list | Views/LastMatches/Index.cshtml:15-20 | adapted | The old page showed nothing; the hub shows "No match found" in the same heading style. |

## Match card (`Views/LastMatches/_Match.cshtml`)

The card is `Views/Matches/_Match.cshtml` with the model `MatchCardModel`; `IMatchViewService.CardsAsync(filter)` builds the cards for any page (the old player page also rendered this partial).

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| `panel panel-primary` with heading padding `6px 10px` | _Match.cshtml:160-161 | done | |
| Campaign name in bold `h4` | _Match.cshtml:164-166 | adapted | A match on maps of no registered campaign (only possible in the hub) shows "Custom campaign". |
| Configuration name in italic `small` | _Match.cshtml:167 | done | |
| Server name | (new) | adapted | Hub addition: a second italic `small` after the configuration name, only when there is more than one game server. |
| Label "Competitive" (`label-success`) or "Casual" (`label-default`) | _Match.cshtml:168-175 | done | |
| "Team {0} won by {1:N0} points" | _Match.cshtml:176-178 | done | |
| Calendar icon and start: "Today at {t}", "Yesterday at {t}" or "{d} at {t}" | _Match.cshtml:181-197 | done | Today is the current day in the site's time zone. |
| Tooltip "Date and time when the game occurred" | _Match.cshtml:181 | done | |
| Clock icon and duration `hh:mm:ss`, only when the match has an end | _Match.cshtml:199-205 | done | |
| Tooltip "Duration of the game in hours" | _Match.cshtml:201 | done | |
| Campaign image (`img-responsive img-thumbnail img-campaign`, alt and title with the campaign) opening in a lightbox (`data-lity`) | _Match.cshtml:212 | done | |
| Campaign without image falls back to `no-image.jpg` | Core/Campaign/CampaignThumb.cs:7-56 | adapted | The old site had a fixed list of campaigns with an image; the hub checks whether the image file exists (same file names), so a campaign added by the administrator gets its image as soon as the file is there. |
| "Details" button (`btn btn-danger btn-xs btn-block`), hidden on the details pages | _Match.cshtml:213-218, Models/MatchDetailsModel.cs:12 | done | |
| Two team panels side by side (`col-xs-6`), green for the winner and red for the loser | _Match.cshtml:221-225 | done | |
| Team heading "Team A/B" and score (`N0`) on the right | _Match.cshtml:226-235 | done | |
| Players ordered by MVPs, then MVP (SI), then MVP (CI), then fewest LVP | _Match.cshtml:239-244 | done | |
| Row highlighted (`it-is-me`) for the signed-in player | _Match.cshtml:246 | done | |
| Avatar 32px with rounded corners, link to the player, name in alt and title, fallback image on error | _Match.cshtml:247-251 | done | |
| Truncated player name linking to the player page, "-" when unknown | _Match.cshtml:253 | done | |
| Vertical bar separator after the name when the player has any MVP | _Match.cshtml:254-257 | done | Tank wipes were added after it. |
| One gold star per MVP (SI) with tooltip "MVP (SI)" | _Match.cshtml:259-262 | done | |
| One blue star per MVP (CI) with tooltip "MVP (CI)" | _Match.cshtml:264-267 | done | |
| Horizontal rule between teams and charts | _Match.cshtml:281 | done | |
| "Scoreboard" line chart: one series per team ("Team A", "Team B"), score after each map, green for the winner and red for the loser, map names cut at 16 characters, legend at the bottom, axis starting at zero | _Match.cshtml:26-79, :284-294 | done | |
| "Points earned per map" bar chart: one series per team with the points of each map (difference from the previous map) | _Match.cshtml:81-157, :296-306 | done | Tank wipes were added after it. |
| Chart text colour follows the theme (`#222222` light, `#c8c8c8` dark) | _Match.cshtml:18-24 | done | The theme cookie is the hub's (`hub_theme`). |
| Charts drawn only when they come into view (`canvas.app-chart` with `data-chart`), 200px high | _Match.cshtml:285-304 | done | |
| Mobile: image column and teams column take the full width (`left-side`, `right-side`); charts stacked (`col-sm-6 col-xs-12`) | _Match.cshtml:211, :220, :284, :296 | done | |

## Match details (`Views/LastMatches/Details.cshtml`)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Page title "Match details - {campaign}" | PageMetadata/MatchDetailsPageMetadata.cs:18 | done | |
| Meta description "See details of the match on {campaign}, played on {date}, with a {A}–{B} score." | PageMetadata/MatchDetailsPageMetadata.cs:19 | done | |
| Social image: campaign thumbnail, alt text with the campaign | PageMetadata/MatchDetailsPageMetadata.cs:20-21 | done | |
| Style `.maps .label { margin: 0 3px; }` | Details.cshtml:17-23 | done | |
| "No match found" when the match has no statistics files | Details.cshtml:62-72 | done | |
| Heading "Match details" | Details.cshtml:74 | done | |
| Breadcrumb: "Last matches" (link to the list) / campaign | Details.cshtml:76-83 | done | |
| Match card without the "Details" button | Details.cshtml:85-87 | done | |
| Panel "Maps" with the text "Map results" | Details.cshtml:90-96 | done | |
| Maps table columns: #, Map, Start, End, Elapsed, Scoreboard (three cells) | Details.cshtml:97-105 | done | |
| `#n` and map name link to the statistics page of the map | Details.cshtml:117-126 | done | |
| Start and end as short date plus `HH:mm`; elapsed as `hh:mm:ss` | Details.cshtml:127-129 | done | |
| Team A score label (green winner, red loser, grey draw) with tooltip "Team A score" | Details.cshtml:113, :130-133 | done | |
| Score difference label with tooltip "Score difference" | Details.cshtml:143-147 | done | |
| Team B score label with tooltip "Team B score" | Details.cshtml:114, :148-151 | done | |
| Administrator: "Edit" link button (tooltip "Edit") beside each team score | Details.cshtml:134-141, :152-159 | done | |
| `editScore`: prompt "Team X score" with the current value, ignores empty or non-numeric input, alert "Invalid score range" outside 0–10,000, `PUT /api/statistics/{id}/update-score`, then shows the page again | Details.cshtml:25-60 | adapted | Same flow. The script is only sent to administrators and the request carries the antiforgery token. After saving, the page opens the address of the match named in the answer (the same page, since the match keeps its id; the list when the correction leaves no match). |
| Mobile: table turns into labelled rows (`table-responsive`, `data-label` on every cell) | Details.cshtml:97, :117-148 | done | |
| Per team, panel "Team X - Survivor" with the text "Statistics of team X playing as survivors" | Details.cshtml:168-174 | done | |
| Survivor table header: one column pair per player (truncated name linking to the player), "Total" | Details.cshtml:177-188 | done | |
| Second header row: "Value" and "Team %" under each player | Details.cshtml:189-201 | done | |
| Row "SI MVP awards": value, share, total | Details.cshtml:204-216 | done | |
| Row "CI MVP awards" | Details.cshtml:218-230 | done | |
| Row "FF LVP awards" | Details.cshtml:232-244 | done | |
| Row "Deaths" | Details.cshtml:246-258 | done | |
| Row "Times incapacitated" | Details.cshtml:260-272 | done | |
| Row "SI damage taken" with the info icon and its tooltip | Details.cshtml:274-289 | done | |
| Row "Commons killed" | Details.cshtml:291-303 | done | |
| Row "SI killed" | Details.cshtml:305-317 | done | |
| Row "SI - Damage dealt" | Details.cshtml:319-331 | done | |
| Row "Damage to Tank" | Details.cshtml:333-345 | done | |
| Row "Damage to Witch" | Details.cshtml:347-359 | done | |
| Row "Rock hits taken" | Details.cshtml:361-373 | done | |
| Row "Total skeets" | Details.cshtml:375-387 | done | |
| Row "Melee skeets" | Details.cshtml:389-401 | done | |
| Row "Total levels" | Details.cshtml:403-415 | done | |
| Row "Crowns (Witch one-shot)" | Details.cshtml:417-429 | done | |
| Row "FF dealt (non-incap.)" | Details.cshtml:431-443 | done | |
| Values as `N0`, shares as `P0` | Details.cshtml:209-212 (and every row) | done | |
| Mobile: share cells hidden (`hide-mobile`), value cells labelled with the player name, total labelled "Total" | Details.cshtml:208-215 (and every row) | done | |
| Per team, panel "Team X - Infected" with the text "Statistics of team X playing as infected" | Details.cshtml:449-455 | done | |
| Infected table header rows (players, "Value" / "Team %", "Total") | Details.cshtml:457-483 | done | |
| Row "SI damage (excl. Tank)" with info icon and tooltip | Details.cshtml:485-500 | done | |
| Row "Damage as Tank (upright)" | Details.cshtml:502-514 | done | |
| Row "Spit damage (upright)" | Details.cshtml:516-528 | done | |
| Row "Total pounce damage" with info icon and tooltip | Details.cshtml:530-545 | done | |

## Map statistics (`Views/LastMatches/Statistics.cshtml`)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Page title "{map} - {campaign}" | PageMetadata/MatchStatisticsPageMetadata.cs:19 | done | |
| Meta description "See statistics for map {map} from the match on {campaign}, with a {A}–{B} score." | PageMetadata/MatchStatisticsPageMetadata.cs:20 | done | |
| Social image: campaign thumbnail, alt text with the campaign | PageMetadata/MatchStatisticsPageMetadata.cs:21-22 | done | |
| Styles: `.round-details` side paddings from 992px, `.player-name` wrapping | Statistics.cshtml:21-38 | done | |
| "No match found" when the match has no statistics files | Statistics.cshtml:183-193 | done | |
| Heading "Match details" | Statistics.cshtml:202 | done | |
| Breadcrumb: "Last matches" / campaign (link to the match) / map name | Statistics.cshtml:204-214 | done | |
| Maps table (#, Map, Start, End, Elapsed, Scoreboard) with the current map highlighted (`tr.info`) and links to each map | Statistics.cshtml:216-271 | done | |
| Team A / difference / Team B score labels with tooltips | Statistics.cshtml:251-265 | done | No "Edit" button here, as before. |
| Tabs "1st round" and "2nd round", one per half, first one active | Statistics.cshtml:273-282 | done | |
| Tab pane `fade`, first one `in active` | Statistics.cshtml:309 | done | |
| Panel "Survivors": green when that team won the map, red otherwise | Statistics.cshtml:292, :314-317 | done | |
| Survivors list: avatar, truncated name linking to the player, `it-is-me` highlight | Statistics.cshtml:321-331 | done | |
| Survivors ordered MVP (SI) first, MVP (CI) second, LVP last | Statistics.cshtml:295-303 | done | |
| Gold star for the MVP (SI), blue star for the MVP (CI), LVP icon for the most friendly fire, each with its tooltip | Statistics.cshtml:332-345 | done | |
| Panel "Infecteds": green when that team won the map, red otherwise | Statistics.cshtml:293, :355-358 | done | |
| Infected list ordered by damage: avatar, name, damage (`N0`) with tooltip "SI damage (excl. Tank)", `it-is-me` highlight | Statistics.cshtml:305-307, :362-379 | done | |
| Panel "Details", left table: every non-zero round counter (restarts, pills, kits, defibrillators, commons, SI killed, SI damage, SI spawned, witches, tanks, incaps, deaths, FF) | Statistics.cshtml:388-407 | done | |
| Panel "Details", right table: round start, end and duration; pause start, end and duration; tank start, end and duration (each only when present) | Statistics.cshtml:409-476 | done | |
| Panel "Survivors" (statistics table): header with each player's truncated name linking to the player | Statistics.cshtml:488-507 | done | |
| Rows shown only when some player has a value; blank separator rows between groups | Statistics.cshtml:61-128 | done | |
| Group: deaths, incaps, SI damage taken (with info tooltip), fall damage | Statistics.cshtml:510-517 | done | |
| Per weapon (shotgun, SMG/rifle, sniper, pistol): shots, hits, hits %, SI hits, SI hits %, tank hits, tank hits %, headshots, headshots %, SI headshots, SI headshots % | Statistics.cshtml:519-634 | done | |
| Group from commons killed to SI damage during tank | Statistics.cshtml:637 | done | |
| Group from tank damage to rock hits taken | Statistics.cshtml:638 | done | |
| Group from skeets to self clears, then clears and average clear time (seconds, two decimals) | Statistics.cshtml:89-92, :639-640 | done | |
| Friendly fire given (by kind, self, total) and taken (by kind, total) | Statistics.cshtml:643-671 | done | |
| Panel "Infecteds" (statistics table): header with each player's name; groups damage, tank, scratch/spit/boom (with info tooltips), pounces to ledged, commons, spawns by class | Statistics.cshtml:677-727 | done | |
| Panel "Connected Players": #, Steam ID, Community ID, Name | Statistics.cshtml:735-748 | done | |
| Copy button beside the Steam ID and the Community ID | Statistics.cshtml:755-768 | done | |
| Player name linking to the player page | Statistics.cshtml:769-773 | done | |
| Numbers as `N0`, percentages as `P0` | Statistics.cshtml:95, :557 | done | |
