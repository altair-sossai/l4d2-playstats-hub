# Site parity: administration, setup and community ranking

🌐 **English** | [Português](admin.pt.md) | [Español](admin.es.md)

The pages of this area exist only in the hub: the old site has no setup wizard, no administration area for servers, modules, settings, game modes, campaigns or administrators, and no community ranking. There is nothing to port from the old site, so this checklist is an inventory of what each page had in its first version (Bootstrap 5, commit `f59518f`) and what became of each item when the page was rewritten for the old site's look (Bootstrap 3, `Views/Shared/_Layout.cshtml`, `_AdminStyles`).

`Where today` is the file and line of the item in the Bootstrap 5 version (paths relative to `src/L4D2PlayStats.Hub.Web`), or `new` for something this work adds.

Status: `done` = present with the same content and behaviour, in the old site's look; `adapted` = present but changed, for the reason given; `not ported` = gone, for the reason given.

Totals: 183 done, 11 adapted, 1 not ported.

How it was checked: every page was opened in a browser on a copy of real data (6,636 statistics files) and on an empty data folder (the whole wizard, from the setup code to "Finish"), in the dark and in the light theme, at 1280 and at 400 pixels wide, in English and in Portuguese; no page scrolls sideways at 400 pixels. Each form was posted at least once. `AdminPagesTests`, `SiteSettingsTests` and `FooterLinksTests` cover the layout, the menu, the new settings and the logo.

## Setup wizard (`/setup`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Route `GET /setup`; redirects to `/` once the setup is finished | `Controllers/SetupController.cs:34` | `done` |  |
| Page title "Setup" | `Views/Setup/Index.cshtml:3` | `done` | Set through `PageMetadata`, with `noindex`. |
| Heading "Welcome" | `Views/Setup/Index.cshtml:19` | `done` |  |
| Lead text "A few steps and the site is ready." | `Views/Setup/Index.cshtml:20` | `done` |  |
| Steps bar: Administrator, Site, Modules, Game server, Finish, numbered, with the current step and the finished steps marked | `Views/Setup/Index.cshtml:22` | `adapted` | Bootstrap 3 justified pills; finished steps get a check mark. On phones only the current step is shown, so the bar takes one line instead of five. |
| Step 1: heading "Who is the administrator?" and the explanation of the setup code | `Views/Setup/Index.cshtml:33` | `done` |  |
| Step 1: error message when the setup code is not valid | `Views/Setup/Index.cshtml:37` | `done` |  |
| Step 1: form `POST /setup/start` with the antiforgery token | `Views/Setup/Index.cshtml:39` | `done` |  |
| Step 1: field "Setup code" (required, no autocomplete, keeps the typed value, filled from `?token=`) | `Views/Setup/Index.cshtml:42` | `done` |  |
| Step 1: field "Development only: sign in as this SteamID64" (only with the development sign-in on) | `Views/Setup/Index.cshtml:47` | `done` |  |
| Step 1: button "Continue with Steam" with the Steam icon | `Views/Setup/Index.cshtml:51` | `done` |  |
| Step 1: the code is kept in a cookie, the visitor signs in with Steam and `GET /setup/claim` makes that account the first administrator | `Controllers/SetupController.cs:67` | `done` |  |
| Sign-in step: heading "Setup is in progress", explanation and button "Sign in with Steam" (`/login?returnUrl=/setup`) | `Views/Setup/Index.cshtml:60` | `done` |  |
| Step 2: form `POST /setup/site` with the antiforgery token, heading "About the site" | `Views/Setup/Index.cshtml:68` | `done` |  |
| Step 2: field "Site name" (required) | `Views/Setup/Index.cshtml:73` | `done` |  |
| Step 2: field "Default language" (English, Português, Español) | `Views/Setup/Index.cshtml:78` | `done` |  |
| Step 2: field "Time zone" with the list of time zones and the help text "Decides when a season starts and ends." | `Views/Setup/Index.cshtml:87` | `done` |  |
| Step 2: field "Season length, in months" (1 to 24) with its help text | `Views/Setup/Index.cshtml:99` | `done` |  |
| Step 2: field "Steam Web API key (optional)" with its help text and the link to steamcommunity.com/dev/apikey | `Views/Setup/Index.cshtml:104` | `done` |  |
| Step 2: site logo upload | new | `done` | Added. Same rules as on the Settings page; a refused file keeps the wizard on this step with the reason. |
| Step 3: form `POST /setup/modules`, heading "What should this site have?", note "You can change this later in the administration area.", the module switches and "Continue" | `Views/Setup/Index.cshtml:113` | `done` |  |
| Step 4: heading "Connect a game server" | `Views/Setup/Index.cshtml:126` | `done` |  |
| Step 4, no server yet: explanation, form `POST /setup/server` with "Server name" (required, 100) and "Address players connect to (optional)" (placeholder, 200), button "Create" | `Views/Setup/Index.cshtml:130` | `done` |  |
| Step 4: link "Skip for now" | `Views/Setup/Index.cshtml:140` | `done` |  |
| Step 4, server created: server name, connection instructions with the token shown once, status box that refreshes by itself, "Continue" | `Views/Setup/Index.cshtml:145` | `done` | The two configuration lines also get the old site's copy button. htmx is loaded only on this step. |
| Step 5: form `POST /setup/finish`, heading "All set", explanation, button "Finish" (goes to `/admin`) | `Views/Setup/Index.cshtml:157` | `done` |  |
| The logo and the theme switch answer while the setup is pending | new | `done` | Added: `/site/logo` and `/theme/...` are no longer sent to `/setup`, so the uploaded logo shows in the menu of the wizard and the theme can be changed there. |

## Shared partials

### `_AdminNav`

| Item | Where today | Status | Notes |
|---|---|---|---|
| Links: Overview, Game servers, Modules, Settings, Game modes, Campaigns, Administrators | `Views/Shared/_AdminNav.cshtml:8` | `adapted` | The row of pills is now a side menu (`Views/Shared/_AdminMenu.cshtml`, in `_AdminLayout`) on every administrator page, grouped in Site, Game servers, Ranking, Skin shop and Anti-Cheat. The row was missing on the chat bans, Anti-Cheat and skin pages, so the administrator lost the way back; the menu is now always there. On phones it collapses into a control above the content that names the current page. |
| Link "Skins", only with the skin shop on | `Views/Shared/_AdminNav.cshtml:18` | `adapted` | Replaced by the four skin pages (Purchases, Prices, Catalog, Shop settings) in the Skin shop group of the side menu, which also replaces their own row of tabs. |
| Link "Community ranking", only with the community ranking on | `Views/Shared/_AdminNav.cshtml:21` | `done` |  |
| Link "Chat bans", only with the site chat on | `Views/Shared/_AdminNav.cshtml:24` | `done` |  |
| Link "Remote commands", only with the remote commands on | `Views/Shared/_AdminNav.cshtml:27` | `done` |  |
| Link "Player connections", only with the live server on | `Views/Shared/_AdminNav.cshtml:30` | `done` |  |
| Link "Anti-Cheat" (`/admin/anti-cheat`), only with the Anti-Cheat module on | new | `done` | Added. Named "Anti-Cheat settings" since the page got the usage modes and the lists. |
| The link of the current page is highlighted | `Views/Shared/_AdminNav.cshtml:35` | `done` | Sub-pages too: the page of one game server highlights Game servers. |

### `_ServerStatus`

| Item | Where today | Status | Notes |
|---|---|---|---|
| Refreshes by itself every 3 seconds (`GET /admin/servers/{id}/status`) | `Views/Shared/_ServerStatus.cshtml:30` | `done` | Still htmx, now loaded by the pages that show the status (server page, wizard) instead of by the layout. Checked in a browser: the block is replaced without reloading the page. |
| Coloured dot and title for the state: Connected, Offline, Invalid key, Waiting for the first connection | `Views/Shared/_ServerStatus.cshtml:32` | `done` |  |
| "Last contact" with the date and time | `Views/Shared/_ServerStatus.cshtml:36` | `done` |  |
| Alert when the identifier arrived with the wrong key | `Views/Shared/_ServerStatus.cshtml:41` | `done` |  |
| Check "Connection with this site" | `Views/Shared/_ServerStatus.cshtml:44` | `done` |  |
| Check "Statistics upload plugin loaded" | `Views/Shared/_ServerStatus.cshtml:45` | `done` |  |
| Check "Statistics writing enabled (sm_stats_writestats 1)" | `Views/Shared/_ServerStatus.cshtml:46` | `done` |  |
| Check "First statistics file received" with the number of files | `Views/Shared/_ServerStatus.cshtml:47` | `done` |  |
| Check "Live match data arriving", only with the live server on | `Views/Shared/_ServerStatus.cshtml:50` | `done` |  |
| Check "Weapon skins plugin loaded", only with the skin shop on | `Views/Shared/_ServerStatus.cshtml:54` | `done` |  |
| Each check shows passed, failed or not known yet, with its colour | `Views/Shared/_ServerStatus.cshtml:15` | `done` | Glyphicons (`ok`, `remove`, `option-horizontal`) with `text-success`, `text-danger`, `text-muted` in place of the text symbols. |
| Line with what the server reported: host name, map, configuration, SourceMod version | `Views/Shared/_ServerStatus.cshtml:60` | `done` |  |

### `_ServerInstructions`

| Item | Where today | Status | Notes |
|---|---|---|---|
| Step "Download the PlayStats plugins" with the links "Download" and "Step-by-step guide" and the note about the plugins folder | `Views/Shared/_ServerInstructions.cshtml:8` | `done` |  |
| Step with what the server needs (SourceMod, REST in Pawn, l4d2_playstats) | `Views/Shared/_ServerInstructions.cshtml:15` | `done` |  |
| Step with the two lines for `cfg/server.cfg` (`playstats_url`, `playstats_token`) | `Views/Shared/_ServerInstructions.cshtml:19` | `done` | With the old site's copy button (`_CopyButton`). |
| Warning "Copy the token now..." when the key has just been generated | `Views/Shared/_ServerInstructions.cshtml:23` | `done` |  |
| Note "The key is not stored in readable form..." otherwise, with `YOUR-KEY` in place of the key | `Views/Shared/_ServerInstructions.cshtml:27` | `done` |  |
| Step "Restart the game server or change the map..." with the `sm_playstats_test` tip | `Views/Shared/_ServerInstructions.cshtml:30` | `done` |  |

### `_ModuleSwitches`

| Item | Where today | Status | Notes |
|---|---|---|---|
| Switch "Ranking and statistics" with its description | `Views/Shared/_ModuleSwitches.cshtml:5` | `done` | Bootstrap 3 has no switch: all seven are checkboxes with the title in bold and the description under it. |
| Switch "Live server" with its description | `Views/Shared/_ModuleSwitches.cshtml:6` | `done` |  |
| Switch "Site chat" with its description | `Views/Shared/_ModuleSwitches.cshtml:7` | `done` |  |
| Switch "Remote commands" with its description | `Views/Shared/_ModuleSwitches.cshtml:8` | `done` |  |
| Switch "Weapon skin shop" with its description | `Views/Shared/_ModuleSwitches.cshtml:9` | `done` |  |
| Switch "Community ranking" with its description | `Views/Shared/_ModuleSwitches.cshtml:10` | `done` |  |
| Switch "Anti-cheat", disabled ("Reserved. Not available yet.") | `Views/Shared/_ModuleSwitches.cshtml:11` | `adapted` | No longer reserved: the switch is enabled like the others and its description is "Shows the Anti-Cheat page with the download and instructions. The Anti-Cheat itself is a separate program." (the page itself is ported in the base branch). |
| Hidden `false` value after each switch, so an unchecked switch is saved as off | `Views/Shared/_ModuleSwitches.cshtml:18` | `done` |  |

## Overview (`/admin`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Every `/admin` page is only for administrators (401 anonymous, 403 other players) | `Controllers/Admin/AdminController.cs:23` | `done` |  |
| Page title and heading "Administration" | `Views/Admin/Index.cshtml:3` | `done` |  |
| Administration menu | `Views/Admin/Index.cshtml:6` | `done` |  |
| Message "Reprocessing finished. Matches rebuilt: N" | `Views/Admin/Index.cshtml:8` | `done` |  |
| Tile "Game servers" | `Views/Admin/Index.cshtml:14` | `done` | `admin-stats` tiles, as on the old Chat Bans page. |
| Tile "Statistics files" | `Views/Admin/Index.cshtml:15` | `done` |  |
| Tile "Matches" | `Views/Admin/Index.cshtml:16` | `done` |  |
| Tile "Ranked matches" | `Views/Admin/Index.cshtml:17` | `done` |  |
| Tile "Players" | `Views/Admin/Index.cshtml:18` | `done` |  |
| Tile "Seasons" | new | `done` | Added: the controller already counted the seasons and the page did not show the number. |
| Warning about configurations that match no rule, with the link "Review the game modes" | `Views/Admin/Index.cshtml:21` | `done` |  |
| Section "Game servers": message "No game server registered yet." with the link "Add one" | `Views/Admin/Index.cshtml:32` | `done` | `admin-empty` inside an `admin-list`. |
| Section "Game servers": one line per server with the status dot, the name (link to the server page) and the number of files | `Views/Admin/Index.cshtml:39` | `done` | `admin-list` / `admin-item`, as on the old Chat Bans page. The line also shows the state in words and the identifier. |
| Panel "Maintenance": explanation and button "Reprocess all matches" (`POST /admin/reprocess`, asks "Rebuild every match now?") | `Views/Admin/Index.cshtml:51` | `done` |  |

## Game servers (`/admin/servers`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Page title and heading "Game servers", administration menu | `Views/Admin/Servers.cshtml:3` | `done` |  |
| Table column "Server": name (link to the server page) and the badge "Disabled" | `Views/Admin/Servers.cshtml:14` | `done` | Table with the old site's classes; on phones each cell is a line with its `data-label`. |
| Table column "Identifier" | `Views/Admin/Servers.cshtml:15` | `done` |  |
| Table column "Status": dot and state | `Views/Admin/Servers.cshtml:16` | `done` |  |
| Table column "Statistics files" | `Views/Admin/Servers.cshtml:17` | `done` |  |
| Panel "Add a game server": "Server name" (required, 100), "Address players connect to (optional)" (placeholder, 200), button "Create" (`POST /admin/servers`) | `Views/Admin/Servers.cshtml:44` | `done` |  |
| Error "A name is required."; after creating, the server page opens with the token shown once | `Controllers/Admin/AdminController.cs:80` | `done` |  |

## Game server (`/admin/servers/{id}`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Page title and heading with the server name, administration menu | `Views/Admin/Server.cshtml:4` | `done` | Added a button back to the list of game servers in the header. |
| Panel "How to connect this server" with the instructions | `Views/Admin/Server.cshtml:12` | `done` |  |
| Panel "Status" that refreshes by itself | `Views/Admin/Server.cshtml:18` | `done` |  |
| Panel "Details": form `POST /admin/servers/{id}` with "Server name" (required, 100) and "Address players connect to (optional)" (200) | `Views/Admin/Server.cshtml:28` | `done` |  |
| Panel "Details": switch "Enabled", with the hidden `false` value | `Views/Admin/Server.cshtml:38` | `done` | Checkbox. |
| Panel "Details": the identifier of the server and the button "Save" | `Views/Admin/Server.cshtml:42` | `done` |  |
| Panel "Key": explanation and button "Generate a new key" (`POST .../key`, asks "Generate a new key?") | `Views/Admin/Server.cshtml:51` | `done` | `panel-warning`. |
| Panel "Key": button "Delete" (`POST .../delete`, asks "Delete this server?") | `Views/Admin/Server.cshtml:54` | `done` |  |
| Error "A server that already sent statistics cannot be deleted. Disable it instead." | `Controllers/Admin/AdminController.cs:144` | `done` |  |

## Settings (`/admin/settings`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Page title and heading "Settings", administration menu | `Views/Admin/Settings.cshtml:3` | `done` |  |
| Form `POST /admin/settings` with the antiforgery token, button "Save", message "Saved." | `Views/Admin/Settings.cshtml:9` | `adapted` | The form is now `multipart/form-data`, because of the logo. Besides "Saved.", problems with the new fields are listed in red (refused logo, footer lines left out, invalid Analytics ID). |
| Panel "Site": "Site name" (required) | `Views/Admin/Settings.cshtml:15` | `done` |  |
| Panel "Site": "Public address of the site" with placeholder and help text | `Views/Admin/Settings.cshtml:19` | `done` |  |
| Panel "Site": "Default language" | `Views/Admin/Settings.cshtml:25` | `done` |  |
| Panel "Site": "Time zone" with the list of time zones | `Views/Admin/Settings.cshtml:34` | `done` |  |
| Panel "Site": "Steam Web API key (optional)" with its help text | `Views/Admin/Settings.cshtml:45` | `done` |  |
| Panel "Appearance": site logo, with preview, upload and removal | new | `done` | Added. Preview on a strip with the menu colour, file field (PNG, JPG, WEBP, GIF, up to 1 MB), and a checkbox to remove the logo. Stored by `BrandingStore`, served at `/site/logo`. |
| Panel "Appearance": footer links | new | `done` | Added. One per line as `Text | https://address`; saved normalised, lines that are not links are left out and reported. Empty means the link to the project. |
| Panel "Appearance": Google Analytics ID | new | `done` | Added. Optional; only letters, digits and hyphens are accepted. |
| Panel "Appearance": keywords for search engines | new | `done` | Added (`SiteSettings.MetaKeywords`): saved trimmed, separated by commas, at most 500 characters. The `<meta name="keywords">` that shows it is in the layout of the base branch. |
| Panel "Seasons": "Season length, in months" (1 to 24) with its help text | `Views/Admin/Settings.cshtml:55` | `done` |  |
| Panel "Ranking points": "Win" | `Views/Admin/Settings.cshtml:64` | `done` |  |
| Panel "Ranking points": "Loss" | `Views/Admin/Settings.cshtml:65` | `done` |  |
| Panel "Ranking points": "Rage quit" | `Views/Admin/Settings.cshtml:66` | `done` |  |
| Panel "Ranking points": "MVP in special infected damage, per round" | `Views/Admin/Settings.cshtml:67` | `done` |  |
| Panel "Ranking points": "MVP in common infected, per round" | `Views/Admin/Settings.cshtml:68` | `done` |  |
| Panel "Ranking points": "Minimum matches to appear in the ranking" | `Views/Admin/Settings.cshtml:69` | `done` |  |
| Panel "Ranking points": switch "Keep matches played on maps that are not in the campaign list", with the hidden `false` value, and the help text | `Views/Admin/Settings.cshtml:72` | `done` | Checkbox. |

## Modules (`/admin/modules`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Page title and heading "Modules", administration menu | `Views/Admin/Modules.cshtml:3` | `done` |  |
| Form `POST /admin/modules`: note "A module that is off disappears from the menu...", the module switches, button "Save", message "Saved." | `Views/Admin/Modules.cshtml:8` | `done` |  |

## Administrators (`/admin/administrators`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Page title and heading "Administrators", administration menu | `Views/Admin/Administrators.cshtml:3` | `done` |  |
| Table column "Name" ("-" when there is none) | `Views/Admin/Administrators.cshtml:12` | `done` | Added the avatar of each administrator (hidden on phones); your own line is highlighted like "it is me" lines elsewhere. |
| Table column "SteamID64", link to the Steam profile | `Views/Admin/Administrators.cshtml:13` | `done` |  |
| Table column "Since" | `Views/Admin/Administrators.cshtml:14` | `done` |  |
| Button "Remove" (`POST /admin/administrators/{id}/delete`, asks "Remove this administrator?") | `Views/Admin/Administrators.cshtml:28` | `done` |  |
| Badge "You" on your own line, in place of the button | `Views/Admin/Administrators.cshtml:34` | `done` | `label label-default`. |
| Panel "Add an administrator": "SteamID64" (number, required, placeholder), "Name" (100), button "Add" | `Views/Admin/Administrators.cshtml:46` | `done` |  |
| Errors "That is not a valid SteamID64." and "You cannot remove yourself or the last administrator." | `Controllers/Admin/AdminController.cs:249`, `Controllers/Admin/AdminController.cs:258` | `done` |  |

## Campaigns (`/admin/campaigns`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Page title and heading "Campaigns", administration menu | `Views/Admin/Campaigns.cshtml:3` | `done` |  |
| Explanation of how the maps form a match | `Views/Admin/Campaigns.cshtml:8` | `done` |  |
| One form per campaign (`POST /admin/campaigns`): name (required) and the maps, one per line, in play order | `Views/Admin/Campaigns.cshtml:16` | `done` | One panel per campaign, in a grid that goes from four columns to one on phones. |
| Button "Save" of each campaign | `Views/Admin/Campaigns.cshtml:21` | `done` |  |
| Button "Delete" of each campaign (`POST /admin/campaigns/{id}/delete`, asks "Delete this campaign?") | `Views/Admin/Campaigns.cshtml:22` | `done` |  |
| Form for a new campaign: name (placeholder "New campaign"), maps (placeholder, required), button "Add" | `Views/Admin/Campaigns.cshtml:30` | `done` |  |

## Game modes (`/admin/game-modes`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Page title and heading "Game modes", administration menu | `Views/Admin/GameModes.cshtml:3` | `done` |  |
| Explanation of how the rules are applied | `Views/Admin/GameModes.cshtml:9` | `done` |  |
| Table "Rules": columns Name, Configuration pattern, Team size, Minimum maps, Priority, Counts for the ranking | `Views/Admin/GameModes.cshtml:17` | `done` |  |
| One form per rule (`POST /admin/game-modes`): Name (required), Configuration pattern (required), Team size (1 to 8, "Any" when empty), Minimum maps (from 1), Priority, Counts for the ranking (Yes/No) | `Views/Admin/GameModes.cshtml:32` | `done` | Each rule is now a real table row: the fields are tied to the form of the row with the `form` attribute, so the columns line up with the header and the phone layout (`data-label`) works. Checked in a browser: saving and deleting a rule post the right fields. |
| Button "Save" of each rule | `Views/Admin/GameModes.cshtml:45` | `done` |  |
| Button "Delete" of each rule (`POST /admin/game-modes/{id}/delete`, asks "Delete this rule?") | `Views/Admin/GameModes.cshtml:46` | `done` |  |
| Line for a new rule: placeholder "New rule", pattern `*`, minimum maps 2, priority 50, button "Add" | `Views/Admin/GameModes.cshtml:55` | `done` |  |
| Section "Configurations seen on your servers" | `Views/Admin/GameModes.cshtml:74` | `done` |  |
| Message "Nothing yet. Configurations appear here when the first statistics files arrive." | `Views/Admin/GameModes.cshtml:77` | `done` | `admin-empty`. |
| Table columns Configuration, Team size, Last seen, Rule | `Views/Admin/GameModes.cshtml:85` | `done` |  |
| Rule badge: "No rule: not ranked", the rule name, or the rule name with "Not ranked" | `Views/Admin/GameModes.cshtml:101` | `done` | `label-warning`, `label-success`, `label-default`. |

## Community ranking (`/community`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| The pages answer only with the community ranking on | `Controllers/CommunityController.cs:14` | `done` |  |
| Page title and heading "Community ranking" | `Views/Community/Index.cshtml:3` | `done` | `PageMetadata` with the description and the avatar of the first place; heading as on the ranking page (`h4.text-center`). `ViewBag.Community` marks the menu. |
| Text "Built from the votes of the players. It does not reset with the season." | `Views/Community/Index.cshtml:8` | `done` |  |
| Button "Vote" | `Views/Community/Index.cshtml:10` | `done` |  |
| Message "Nobody has enough comparisons to be listed yet." | `Views/Community/Index.cshtml:15` | `done` | Jumbotron, as the empty ranking of the home page. |
| Table column "#" (position; the first place stands out) | `Views/Community/Index.cshtml:23` | `done` | As on the home ranking: medals for the first three places, `#N` for the others. |
| Table column "Player": avatar and name, linked to the player page when the ranking module is on | `Views/Community/Index.cshtml:24` | `adapted` | Same two cells as the home ranking (avatar, name). The name is shortened by `TruncatePlayerName`, as on the home ranking; the full name is in the title of the avatar. |
| Table column "Patent" | new | `done` | Added, as on the home ranking: the patent of the player in the current season. Only with the ranking module on. |
| Table column "Rating" | `Views/Community/Index.cshtml:25` | `done` |  |
| Table column "Comparisons" | `Views/Community/Index.cshtml:26` | `adapted` | Column on desktop. On phones the column is hidden (`hide-mobile`) and the number is a line under the name; the Bootstrap 5 page scrolled sideways instead. |
| Table column "Wins" | `Views/Community/Index.cshtml:27` | `done` |  |
| Table column "Draws" | `Views/Community/Index.cshtml:28` | `adapted` | Same as "Comparisons". |
| Table column "Losses" | `Views/Community/Index.cshtml:29` | `adapted` | Same as "Comparisons". |
| The line of the signed-in player is highlighted | `Views/Community/Index.cshtml:35` | `done` | `it-is-me`, the class the old site uses. |
| Note "Minimum comparisons to be listed: N" | `Views/Community/Index.cshtml:67` | `done` |  |

## Vote (`/community/vote`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Page title and heading "Who is better?" | `Views/Community/Vote.cshtml:3` | `done` | `PageMetadata`; `ViewBag.Community` marks the menu. |
| Text "Your answers move players up and down the community ranking." | `Views/Community/Vote.cshtml:8` | `done` |  |
| Button "Community ranking" (back to the ranking) | `Views/Community/Vote.cshtml:10` | `done` |  |
| Warning when the answer was refused ("That pair is not the one waiting...", "Choose one of the answers.") | `Views/Community/_Ballot.cshtml:6` | `done` |  |
| Not signed in: "Sign in with Steam to vote." with the Steam button, which comes back to this page | `Views/Community/_Ballot.cshtml:14` | `done` | Jumbotron with the Steam button. |
| Too few matches: message with "Matches played: N / minimum" | `Views/Community/_Ballot.cshtml:23` | `done` | Jumbotron. |
| Daily limit: message with "Votes per day: N" | `Views/Community/_Ballot.cshtml:30` | `done` | Jumbotron. |
| Nobody left: message | `Views/Community/_Ballot.cshtml:36` | `done` | Jumbotron. |
| Ballot form (`POST /community/vote`, antiforgery token); with scripts the answer is sent in the background and only the ballot is replaced | `Views/Community/_Ballot.cshtml:43` | `done` | Still htmx, loaded by this page only. Checked in a browser: the answer is recorded and the next pair appears without reloading. |
| Without scripts the form posts normally and the page reloads, with the refusal shown as an error | `Controllers/CommunityController.cs:60` | `done` |  |
| The two players side by side, each with avatar and name | `Views/Community/_Ballot.cshtml:50` | `done` | Two Bootstrap 3 panels, side by side also on phones. |
| Button "This one is better" under each player | `Views/Community/_Ballot.cshtml:52` | `done` |  |
| Button "I don't know this player" under each player | `Views/Community/_Ballot.cshtml:53` | `done` |  |
| Button "They are at the same level" | `Views/Community/_Ballot.cshtml:69` | `done` |  |
| Button "I don't know either of them" | `Views/Community/_Ballot.cshtml:70` | `done` |  |
| Counter "Votes left today: N / per day" | `Views/Community/_Ballot.cshtml:74` | `done` |  |

## Community ranking administration (`/admin/community`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Only for administrators, and only with the community ranking on | `Controllers/Admin/CommunityAdminController.cs:16` | `done` |  |
| Page title and heading "Community ranking", administration menu | `Views/CommunityAdmin/Index.cshtml:3` | `done` | Added a button to the public ranking in the header. |
| Message "Ratings rebuilt. Votes replayed: N" | `Views/CommunityAdmin/Index.cshtml:9` | `done` |  |
| Message "Ratings rebuilt without that voter. Votes removed: N" | `Views/CommunityAdmin/Index.cshtml:13` | `done` |  |
| Tile "Votes" | `Views/CommunityAdmin/Index.cshtml:19` | `done` | `admin-stats`. |
| Tile "Rated players" | `Views/CommunityAdmin/Index.cshtml:20` | `done` |  |
| Form `POST /admin/community` with the antiforgery token, button "Save", message "Saved." | `Views/CommunityAdmin/Index.cshtml:23` | `done` |  |
| Panel "Voting": "Minimum matches to vote" | `Views/CommunityAdmin/Index.cshtml:30` | `done` |  |
| Panel "Voting": "Votes per day" (with its minimum) | `Views/CommunityAdmin/Index.cshtml:34` | `done` |  |
| Panel "Voting": "Minimum matches to be compared" and the help text | `Views/CommunityAdmin/Index.cshtml:38` | `done` |  |
| Panel "Rating": "Minimum comparisons to be listed" | `Views/CommunityAdmin/Index.cshtml:51` | `done` |  |
| Panel "Rating": "Elo K factor" (with its limits) | `Views/CommunityAdmin/Index.cshtml:55` | `done` |  |
| Panel "Rating": "Initial rating" and the help text | `Views/CommunityAdmin/Index.cshtml:59` | `done` |  |
| Panel "Maintenance": explanation and button "Rebuild the ratings" (asks "Rebuild every rating now?") | `Views/CommunityAdmin/Index.cshtml:73` | `done` |  |
| Panel "Voters": explanation and the form to remove the votes of a SteamID64 (asks "Remove all votes of this voter?") | `Views/CommunityAdmin/Index.cshtml:85` | `done` |  |
| Panel "Voters": message "Nobody has voted yet." | `Views/CommunityAdmin/Index.cshtml:94` | `done` |  |
| Panel "Voters": table column "Player", avatar and name linked to the Steam profile | `Views/CommunityAdmin/Index.cshtml:102` | `done` | The avatar is hidden on phones, where each cell is a line with its label. |
| Panel "Voters": table column "Votes" | `Views/CommunityAdmin/Index.cshtml:103` | `done` |  |
| Panel "Voters": table column "Last vote" | `Views/CommunityAdmin/Index.cshtml:104` | `done` |  |
| Panel "Voters": button "Remove votes" on each line (asks for confirmation) | `Views/CommunityAdmin/Index.cshtml:122` | `done` |  |
| Error "That is not a valid SteamID64." | `Controllers/Admin/CommunityAdminController.cs:64` | `done` |  |

## Welcome page (`/` with the ranking module off)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Page title: the site name | `Views/Home/Welcome.cshtml:4` | `done` | Already converted in the base branch; left as it was. |
| Box with the site name and "Use the menu to see what is available on this site." | `Views/Home/Welcome.cshtml:6` | `done` |  |

## Layout

| Item | Where today | Status | Notes |
|---|---|---|---|
| Bootstrap 5 styles and `bootstrap.bundle` | `Views/Shared/_Bootstrap5Layout.cshtml:19` | `not ported` | It is the point of this work: the pages now use the Bootstrap 3 themes of `_Layout`. `_Bootstrap5Layout` and `wwwroot/lib/bootstrap` are still in the repository for the folders that other branches convert. |
| htmx loaded on every page | `Views/Shared/_Bootstrap5Layout.cshtml:114` | `adapted` | `_Layout` does not load it. The three places that need it load it in their `Scripts` section: the game server page, the server step of the wizard and the vote page. |
| `site.css` classes used by these pages (status dot, wizard steps, configuration block, avatars) | `Views/Shared/_Bootstrap5Layout.cshtml:20` | `adapted` | What these pages still need (status dot, wizard steps, configuration block, grid of panels) is in the partial `Views/Shared/_AdministrationStyles.cshtml`, next to `_AdminStyles`; the ballot styles are in the vote page. |
| Error and "Saved." messages at the top of the page | `Views/Shared/_Bootstrap5Layout.cshtml:100` | `done` | By `_Layout`. |
