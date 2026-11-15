# Site parity: administration, setup and community ranking

The pages of this area exist only in the hub: the old site has no setup wizard, no administration area for servers, modules, settings, game modes, campaigns or administrators, and no community ranking. There is nothing to port from the old site, so this checklist is an inventory of what each page had in its first version (Bootstrap 5, commit `f59518f`) and what became of each item when the page was rewritten for the old site's look (Bootstrap 3, `Views/Shared/_Layout.cshtml`, `_AdminStyles`).

`Where today` is the file and line of the item in the Bootstrap 5 version (paths relative to `src/L4D2PlayStats.Hub.Web`), or `new` for something this work adds.

Status: `done` = present with the same content and behaviour, in the old site's look; `adapted` = present but changed, for the reason given; `not ported` = gone, for the reason given.

Totals: 192 pending.

## Setup wizard (`/setup`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Route `GET /setup`; redirects to `/` once the setup is finished | `Controllers/SetupController.cs:34` | `pending` |  |
| Page title "Setup" | `Views/Setup/Index.cshtml:3` | `pending` |  |
| Heading "Welcome" | `Views/Setup/Index.cshtml:19` | `pending` |  |
| Lead text "A few steps and the site is ready." | `Views/Setup/Index.cshtml:20` | `pending` |  |
| Steps bar: Administrator, Site, Modules, Game server, Finish, numbered, with the current step and the finished steps marked | `Views/Setup/Index.cshtml:22` | `pending` |  |
| Step 1: heading "Who is the administrator?" and the explanation of the setup code | `Views/Setup/Index.cshtml:33` | `pending` |  |
| Step 1: error message when the setup code is not valid | `Views/Setup/Index.cshtml:37` | `pending` |  |
| Step 1: form `POST /setup/start` with the antiforgery token | `Views/Setup/Index.cshtml:39` | `pending` |  |
| Step 1: field "Setup code" (required, no autocomplete, keeps the typed value, filled from `?token=`) | `Views/Setup/Index.cshtml:42` | `pending` |  |
| Step 1: field "Development only: sign in as this SteamID64" (only with the development sign-in on) | `Views/Setup/Index.cshtml:47` | `pending` |  |
| Step 1: button "Continue with Steam" with the Steam icon | `Views/Setup/Index.cshtml:51` | `pending` |  |
| Step 1: the code is kept in a cookie, the visitor signs in with Steam and `GET /setup/claim` makes that account the first administrator | `Controllers/SetupController.cs:67` | `pending` |  |
| Sign-in step: heading "Setup is in progress", explanation and button "Sign in with Steam" (`/login?returnUrl=/setup`) | `Views/Setup/Index.cshtml:60` | `pending` |  |
| Step 2: form `POST /setup/site` with the antiforgery token, heading "About the site" | `Views/Setup/Index.cshtml:68` | `pending` |  |
| Step 2: field "Site name" (required) | `Views/Setup/Index.cshtml:73` | `pending` |  |
| Step 2: field "Default language" (English, Português, Español) | `Views/Setup/Index.cshtml:78` | `pending` |  |
| Step 2: field "Time zone" with the list of time zones and the help text "Decides when a season starts and ends." | `Views/Setup/Index.cshtml:87` | `pending` |  |
| Step 2: field "Season length, in months" (1 to 24) with its help text | `Views/Setup/Index.cshtml:99` | `pending` |  |
| Step 2: field "Steam Web API key (optional)" with its help text and the link to steamcommunity.com/dev/apikey | `Views/Setup/Index.cshtml:104` | `pending` |  |
| Step 2: site logo upload | new | `pending` |  |
| Step 3: form `POST /setup/modules`, heading "What should this site have?", note "You can change this later in the administration area.", the module switches and "Continue" | `Views/Setup/Index.cshtml:113` | `pending` |  |
| Step 4: heading "Connect a game server" | `Views/Setup/Index.cshtml:126` | `pending` |  |
| Step 4, no server yet: explanation, form `POST /setup/server` with "Server name" (required, 100) and "Address players connect to (optional)" (placeholder, 200), button "Create" | `Views/Setup/Index.cshtml:130` | `pending` |  |
| Step 4: link "Skip for now" | `Views/Setup/Index.cshtml:140` | `pending` |  |
| Step 4, server created: server name, connection instructions with the token shown once, status box that refreshes by itself, "Continue" | `Views/Setup/Index.cshtml:145` | `pending` |  |
| Step 5: form `POST /setup/finish`, heading "All set", explanation, button "Finish" (goes to `/admin`) | `Views/Setup/Index.cshtml:157` | `pending` |  |

## Shared partials

### `_AdminNav`

| Item | Where today | Status | Notes |
|---|---|---|---|
| Links: Overview, Game servers, Modules, Settings, Game modes, Campaigns, Administrators | `Views/Shared/_AdminNav.cshtml:8` | `pending` |  |
| Link "Skins", only with the skin shop on | `Views/Shared/_AdminNav.cshtml:18` | `pending` |  |
| Link "Community ranking", only with the community ranking on | `Views/Shared/_AdminNav.cshtml:21` | `pending` |  |
| Link "Chat bans", only with the site chat on | `Views/Shared/_AdminNav.cshtml:24` | `pending` |  |
| Link "Remote commands", only with the remote commands on | `Views/Shared/_AdminNav.cshtml:27` | `pending` |  |
| Link "Player connections", only with the live server on | `Views/Shared/_AdminNav.cshtml:30` | `pending` |  |
| The link of the current page is highlighted | `Views/Shared/_AdminNav.cshtml:35` | `pending` |  |

### `_ServerStatus`

| Item | Where today | Status | Notes |
|---|---|---|---|
| Refreshes by itself every 3 seconds (`GET /admin/servers/{id}/status`) | `Views/Shared/_ServerStatus.cshtml:30` | `pending` |  |
| Coloured dot and title for the state: Connected, Offline, Invalid key, Waiting for the first connection | `Views/Shared/_ServerStatus.cshtml:32` | `pending` |  |
| "Last contact" with the date and time | `Views/Shared/_ServerStatus.cshtml:36` | `pending` |  |
| Alert when the identifier arrived with the wrong key | `Views/Shared/_ServerStatus.cshtml:41` | `pending` |  |
| Check "Connection with this site" | `Views/Shared/_ServerStatus.cshtml:44` | `pending` |  |
| Check "Statistics upload plugin loaded" | `Views/Shared/_ServerStatus.cshtml:45` | `pending` |  |
| Check "Statistics writing enabled (sm_stats_writestats 1)" | `Views/Shared/_ServerStatus.cshtml:46` | `pending` |  |
| Check "First statistics file received" with the number of files | `Views/Shared/_ServerStatus.cshtml:47` | `pending` |  |
| Check "Live match data arriving", only with the live server on | `Views/Shared/_ServerStatus.cshtml:50` | `pending` |  |
| Check "Weapon skins plugin loaded", only with the skin shop on | `Views/Shared/_ServerStatus.cshtml:54` | `pending` |  |
| Each check shows passed, failed or not known yet, with its colour | `Views/Shared/_ServerStatus.cshtml:15` | `pending` |  |
| Line with what the server reported: host name, map, configuration, SourceMod version | `Views/Shared/_ServerStatus.cshtml:60` | `pending` |  |

### `_ServerInstructions`

| Item | Where today | Status | Notes |
|---|---|---|---|
| Step "Download the PlayStats plugins" with the links "Download" and "Step-by-step guide" and the note about the plugins folder | `Views/Shared/_ServerInstructions.cshtml:8` | `pending` |  |
| Step with what the server needs (SourceMod, REST in Pawn, l4d2_playstats) | `Views/Shared/_ServerInstructions.cshtml:15` | `pending` |  |
| Step with the two lines for `cfg/server.cfg` (`playstats_url`, `playstats_token`) | `Views/Shared/_ServerInstructions.cshtml:19` | `pending` |  |
| Warning "Copy the token now..." when the key has just been generated | `Views/Shared/_ServerInstructions.cshtml:23` | `pending` |  |
| Note "The key is not stored in readable form..." otherwise, with `YOUR-KEY` in place of the key | `Views/Shared/_ServerInstructions.cshtml:27` | `pending` |  |
| Step "Restart the game server or change the map..." with the `sm_playstats_test` tip | `Views/Shared/_ServerInstructions.cshtml:30` | `pending` |  |

### `_ModuleSwitches`

| Item | Where today | Status | Notes |
|---|---|---|---|
| Switch "Ranking and statistics" with its description | `Views/Shared/_ModuleSwitches.cshtml:5` | `pending` |  |
| Switch "Live server" with its description | `Views/Shared/_ModuleSwitches.cshtml:6` | `pending` |  |
| Switch "Site chat" with its description | `Views/Shared/_ModuleSwitches.cshtml:7` | `pending` |  |
| Switch "Remote commands" with its description | `Views/Shared/_ModuleSwitches.cshtml:8` | `pending` |  |
| Switch "Weapon skin shop" with its description | `Views/Shared/_ModuleSwitches.cshtml:9` | `pending` |  |
| Switch "Community ranking" with its description | `Views/Shared/_ModuleSwitches.cshtml:10` | `pending` |  |
| Switch "Anti-cheat", disabled ("Reserved. Not available yet.") | `Views/Shared/_ModuleSwitches.cshtml:11` | `pending` |  |
| Hidden `false` value after each switch, so an unchecked switch is saved as off | `Views/Shared/_ModuleSwitches.cshtml:18` | `pending` |  |

## Overview (`/admin`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Every `/admin` page is only for administrators (401 anonymous, 403 other players) | `Controllers/Admin/AdminController.cs:23` | `pending` |  |
| Page title and heading "Administration" | `Views/Admin/Index.cshtml:3` | `pending` |  |
| Administration menu | `Views/Admin/Index.cshtml:6` | `pending` |  |
| Message "Reprocessing finished. Matches rebuilt: N" | `Views/Admin/Index.cshtml:8` | `pending` |  |
| Tile "Game servers" | `Views/Admin/Index.cshtml:14` | `pending` |  |
| Tile "Statistics files" | `Views/Admin/Index.cshtml:15` | `pending` |  |
| Tile "Matches" | `Views/Admin/Index.cshtml:16` | `pending` |  |
| Tile "Ranked matches" | `Views/Admin/Index.cshtml:17` | `pending` |  |
| Tile "Players" | `Views/Admin/Index.cshtml:18` | `pending` |  |
| Tile "Seasons" | new | `pending` |  |
| Warning about configurations that match no rule, with the link "Review the game modes" | `Views/Admin/Index.cshtml:21` | `pending` |  |
| Section "Game servers": message "No game server registered yet." with the link "Add one" | `Views/Admin/Index.cshtml:32` | `pending` |  |
| Section "Game servers": one line per server with the status dot, the name (link to the server page) and the number of files | `Views/Admin/Index.cshtml:39` | `pending` |  |
| Panel "Maintenance": explanation and button "Reprocess all matches" (`POST /admin/reprocess`, asks "Rebuild every match now?") | `Views/Admin/Index.cshtml:51` | `pending` |  |

## Game servers (`/admin/servers`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Page title and heading "Game servers", administration menu | `Views/Admin/Servers.cshtml:3` | `pending` |  |
| Table column "Server": name (link to the server page) and the badge "Disabled" | `Views/Admin/Servers.cshtml:14` | `pending` |  |
| Table column "Identifier" | `Views/Admin/Servers.cshtml:15` | `pending` |  |
| Table column "Status": dot and state | `Views/Admin/Servers.cshtml:16` | `pending` |  |
| Table column "Statistics files" | `Views/Admin/Servers.cshtml:17` | `pending` |  |
| Panel "Add a game server": "Server name" (required, 100), "Address players connect to (optional)" (placeholder, 200), button "Create" (`POST /admin/servers`) | `Views/Admin/Servers.cshtml:44` | `pending` |  |
| Error "A name is required."; after creating, the server page opens with the token shown once | `Controllers/Admin/AdminController.cs:80` | `pending` |  |

## Game server (`/admin/servers/{id}`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Page title and heading with the server name, administration menu | `Views/Admin/Server.cshtml:4` | `pending` |  |
| Panel "How to connect this server" with the instructions | `Views/Admin/Server.cshtml:12` | `pending` |  |
| Panel "Status" that refreshes by itself | `Views/Admin/Server.cshtml:18` | `pending` |  |
| Panel "Details": form `POST /admin/servers/{id}` with "Server name" (required, 100) and "Address players connect to (optional)" (200) | `Views/Admin/Server.cshtml:28` | `pending` |  |
| Panel "Details": switch "Enabled", with the hidden `false` value | `Views/Admin/Server.cshtml:38` | `pending` |  |
| Panel "Details": the identifier of the server and the button "Save" | `Views/Admin/Server.cshtml:42` | `pending` |  |
| Panel "Key": explanation and button "Generate a new key" (`POST .../key`, asks "Generate a new key?") | `Views/Admin/Server.cshtml:51` | `pending` |  |
| Panel "Key": button "Delete" (`POST .../delete`, asks "Delete this server?") | `Views/Admin/Server.cshtml:54` | `pending` |  |
| Error "A server that already sent statistics cannot be deleted. Disable it instead." | `Controllers/Admin/AdminController.cs:144` | `pending` |  |

## Settings (`/admin/settings`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Page title and heading "Settings", administration menu | `Views/Admin/Settings.cshtml:3` | `pending` |  |
| Form `POST /admin/settings` with the antiforgery token, button "Save", message "Saved." | `Views/Admin/Settings.cshtml:9` | `pending` |  |
| Panel "Site": "Site name" (required) | `Views/Admin/Settings.cshtml:15` | `pending` |  |
| Panel "Site": "Public address of the site" with placeholder and help text | `Views/Admin/Settings.cshtml:19` | `pending` |  |
| Panel "Site": "Default language" | `Views/Admin/Settings.cshtml:25` | `pending` |  |
| Panel "Site": "Time zone" with the list of time zones | `Views/Admin/Settings.cshtml:34` | `pending` |  |
| Panel "Site": "Steam Web API key (optional)" with its help text | `Views/Admin/Settings.cshtml:45` | `pending` |  |
| Panel "Appearance": site logo, with preview, upload and removal | new | `pending` |  |
| Panel "Appearance": footer links | new | `pending` |  |
| Panel "Appearance": Google Analytics ID | new | `pending` |  |
| Panel "Seasons": "Season length, in months" (1 to 24) with its help text | `Views/Admin/Settings.cshtml:55` | `pending` |  |
| Panel "Ranking points": "Win" | `Views/Admin/Settings.cshtml:64` | `pending` |  |
| Panel "Ranking points": "Loss" | `Views/Admin/Settings.cshtml:65` | `pending` |  |
| Panel "Ranking points": "Rage quit" | `Views/Admin/Settings.cshtml:66` | `pending` |  |
| Panel "Ranking points": "MVP in special infected damage, per round" | `Views/Admin/Settings.cshtml:67` | `pending` |  |
| Panel "Ranking points": "MVP in common infected, per round" | `Views/Admin/Settings.cshtml:68` | `pending` |  |
| Panel "Ranking points": "Minimum matches to appear in the ranking" | `Views/Admin/Settings.cshtml:69` | `pending` |  |
| Panel "Ranking points": switch "Keep matches played on maps that are not in the campaign list", with the hidden `false` value, and the help text | `Views/Admin/Settings.cshtml:72` | `pending` |  |

## Modules (`/admin/modules`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Page title and heading "Modules", administration menu | `Views/Admin/Modules.cshtml:3` | `pending` |  |
| Form `POST /admin/modules`: note "A module that is off disappears from the menu...", the module switches, button "Save", message "Saved." | `Views/Admin/Modules.cshtml:8` | `pending` |  |

## Administrators (`/admin/administrators`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Page title and heading "Administrators", administration menu | `Views/Admin/Administrators.cshtml:3` | `pending` |  |
| Table column "Name" ("-" when there is none) | `Views/Admin/Administrators.cshtml:12` | `pending` |  |
| Table column "SteamID64", link to the Steam profile | `Views/Admin/Administrators.cshtml:13` | `pending` |  |
| Table column "Since" | `Views/Admin/Administrators.cshtml:14` | `pending` |  |
| Button "Remove" (`POST /admin/administrators/{id}/delete`, asks "Remove this administrator?") | `Views/Admin/Administrators.cshtml:28` | `pending` |  |
| Badge "You" on your own line, in place of the button | `Views/Admin/Administrators.cshtml:34` | `pending` |  |
| Panel "Add an administrator": "SteamID64" (number, required, placeholder), "Name" (100), button "Add" | `Views/Admin/Administrators.cshtml:46` | `pending` |  |
| Errors "That is not a valid SteamID64." and "You cannot remove yourself or the last administrator." | `Controllers/Admin/AdminController.cs:249`, `Controllers/Admin/AdminController.cs:258` | `pending` |  |

## Campaigns (`/admin/campaigns`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Page title and heading "Campaigns", administration menu | `Views/Admin/Campaigns.cshtml:3` | `pending` |  |
| Explanation of how the maps form a match | `Views/Admin/Campaigns.cshtml:8` | `pending` |  |
| One form per campaign (`POST /admin/campaigns`): name (required) and the maps, one per line, in play order | `Views/Admin/Campaigns.cshtml:16` | `pending` |  |
| Button "Save" of each campaign | `Views/Admin/Campaigns.cshtml:21` | `pending` |  |
| Button "Delete" of each campaign (`POST /admin/campaigns/{id}/delete`, asks "Delete this campaign?") | `Views/Admin/Campaigns.cshtml:22` | `pending` |  |
| Form for a new campaign: name (placeholder "New campaign"), maps (placeholder, required), button "Add" | `Views/Admin/Campaigns.cshtml:30` | `pending` |  |

## Game modes (`/admin/game-modes`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Page title and heading "Game modes", administration menu | `Views/Admin/GameModes.cshtml:3` | `pending` |  |
| Explanation of how the rules are applied | `Views/Admin/GameModes.cshtml:9` | `pending` |  |
| Table "Rules": columns Name, Configuration pattern, Team size, Minimum maps, Priority, Counts for the ranking | `Views/Admin/GameModes.cshtml:17` | `pending` |  |
| One form per rule (`POST /admin/game-modes`): Name (required), Configuration pattern (required), Team size (1 to 8, "Any" when empty), Minimum maps (from 1), Priority, Counts for the ranking (Yes/No) | `Views/Admin/GameModes.cshtml:32` | `pending` |  |
| Button "Save" of each rule | `Views/Admin/GameModes.cshtml:45` | `pending` |  |
| Button "Delete" of each rule (`POST /admin/game-modes/{id}/delete`, asks "Delete this rule?") | `Views/Admin/GameModes.cshtml:46` | `pending` |  |
| Line for a new rule: placeholder "New rule", pattern `*`, minimum maps 2, priority 50, button "Add" | `Views/Admin/GameModes.cshtml:55` | `pending` |  |
| Section "Configurations seen on your servers" | `Views/Admin/GameModes.cshtml:74` | `pending` |  |
| Message "Nothing yet. Configurations appear here when the first statistics files arrive." | `Views/Admin/GameModes.cshtml:77` | `pending` |  |
| Table columns Configuration, Team size, Last seen, Rule | `Views/Admin/GameModes.cshtml:85` | `pending` |  |
| Rule badge: "No rule: not ranked", the rule name, or the rule name with "Not ranked" | `Views/Admin/GameModes.cshtml:101` | `pending` |  |

## Community ranking (`/community`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| The pages answer only with the community ranking on | `Controllers/CommunityController.cs:14` | `pending` |  |
| Page title and heading "Community ranking" | `Views/Community/Index.cshtml:3` | `pending` |  |
| Text "Built from the votes of the players. It does not reset with the season." | `Views/Community/Index.cshtml:8` | `pending` |  |
| Button "Vote" | `Views/Community/Index.cshtml:10` | `pending` |  |
| Message "Nobody has enough comparisons to be listed yet." | `Views/Community/Index.cshtml:15` | `pending` |  |
| Table column "#" (position; the first place stands out) | `Views/Community/Index.cshtml:23` | `pending` |  |
| Table column "Player": avatar and name, linked to the player page when the ranking module is on | `Views/Community/Index.cshtml:24` | `pending` |  |
| Table column "Patent" | new | `pending` |  |
| Table column "Rating" | `Views/Community/Index.cshtml:25` | `pending` |  |
| Table column "Comparisons" | `Views/Community/Index.cshtml:26` | `pending` |  |
| Table column "Wins" | `Views/Community/Index.cshtml:27` | `pending` |  |
| Table column "Draws" | `Views/Community/Index.cshtml:28` | `pending` |  |
| Table column "Losses" | `Views/Community/Index.cshtml:29` | `pending` |  |
| The line of the signed-in player is highlighted | `Views/Community/Index.cshtml:35` | `pending` |  |
| Note "Minimum comparisons to be listed: N" | `Views/Community/Index.cshtml:67` | `pending` |  |

## Vote (`/community/vote`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Page title and heading "Who is better?" | `Views/Community/Vote.cshtml:3` | `pending` |  |
| Text "Your answers move players up and down the community ranking." | `Views/Community/Vote.cshtml:8` | `pending` |  |
| Button "Community ranking" (back to the ranking) | `Views/Community/Vote.cshtml:10` | `pending` |  |
| Warning when the answer was refused ("That pair is not the one waiting...", "Choose one of the answers.") | `Views/Community/_Ballot.cshtml:6` | `pending` |  |
| Not signed in: "Sign in with Steam to vote." with the Steam button, which comes back to this page | `Views/Community/_Ballot.cshtml:14` | `pending` |  |
| Too few matches: message with "Matches played: N / minimum" | `Views/Community/_Ballot.cshtml:23` | `pending` |  |
| Daily limit: message with "Votes per day: N" | `Views/Community/_Ballot.cshtml:30` | `pending` |  |
| Nobody left: message | `Views/Community/_Ballot.cshtml:36` | `pending` |  |
| Ballot form (`POST /community/vote`, antiforgery token); with scripts the answer is sent in the background and only the ballot is replaced | `Views/Community/_Ballot.cshtml:43` | `pending` |  |
| Without scripts the form posts normally and the page reloads, with the refusal shown as an error | `Controllers/CommunityController.cs:60` | `pending` |  |
| The two players side by side, each with avatar and name | `Views/Community/_Ballot.cshtml:50` | `pending` |  |
| Button "This one is better" under each player | `Views/Community/_Ballot.cshtml:52` | `pending` |  |
| Button "I don't know this player" under each player | `Views/Community/_Ballot.cshtml:53` | `pending` |  |
| Button "They are at the same level" | `Views/Community/_Ballot.cshtml:69` | `pending` |  |
| Button "I don't know either of them" | `Views/Community/_Ballot.cshtml:70` | `pending` |  |
| Counter "Votes left today: N / per day" | `Views/Community/_Ballot.cshtml:74` | `pending` |  |

## Community ranking administration (`/admin/community`)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Only for administrators, and only with the community ranking on | `Controllers/Admin/CommunityAdminController.cs:16` | `pending` |  |
| Page title and heading "Community ranking", administration menu | `Views/CommunityAdmin/Index.cshtml:3` | `pending` |  |
| Message "Ratings rebuilt. Votes replayed: N" | `Views/CommunityAdmin/Index.cshtml:9` | `pending` |  |
| Message "Ratings rebuilt without that voter. Votes removed: N" | `Views/CommunityAdmin/Index.cshtml:13` | `pending` |  |
| Tile "Votes" | `Views/CommunityAdmin/Index.cshtml:19` | `pending` |  |
| Tile "Rated players" | `Views/CommunityAdmin/Index.cshtml:20` | `pending` |  |
| Form `POST /admin/community` with the antiforgery token, button "Save", message "Saved." | `Views/CommunityAdmin/Index.cshtml:23` | `pending` |  |
| Panel "Voting": "Minimum matches to vote" | `Views/CommunityAdmin/Index.cshtml:30` | `pending` |  |
| Panel "Voting": "Votes per day" (with its minimum) | `Views/CommunityAdmin/Index.cshtml:34` | `pending` |  |
| Panel "Voting": "Minimum matches to be compared" and the help text | `Views/CommunityAdmin/Index.cshtml:38` | `pending` |  |
| Panel "Rating": "Minimum comparisons to be listed" | `Views/CommunityAdmin/Index.cshtml:51` | `pending` |  |
| Panel "Rating": "Elo K factor" (with its limits) | `Views/CommunityAdmin/Index.cshtml:55` | `pending` |  |
| Panel "Rating": "Initial rating" and the help text | `Views/CommunityAdmin/Index.cshtml:59` | `pending` |  |
| Panel "Maintenance": explanation and button "Rebuild the ratings" (asks "Rebuild every rating now?") | `Views/CommunityAdmin/Index.cshtml:73` | `pending` |  |
| Panel "Voters": explanation and the form to remove the votes of a SteamID64 (asks "Remove all votes of this voter?") | `Views/CommunityAdmin/Index.cshtml:85` | `pending` |  |
| Panel "Voters": message "Nobody has voted yet." | `Views/CommunityAdmin/Index.cshtml:94` | `pending` |  |
| Panel "Voters": table column "Player", avatar and name linked to the Steam profile | `Views/CommunityAdmin/Index.cshtml:102` | `pending` |  |
| Panel "Voters": table column "Votes" | `Views/CommunityAdmin/Index.cshtml:103` | `pending` |  |
| Panel "Voters": table column "Last vote" | `Views/CommunityAdmin/Index.cshtml:104` | `pending` |  |
| Panel "Voters": button "Remove votes" on each line (asks for confirmation) | `Views/CommunityAdmin/Index.cshtml:122` | `pending` |  |
| Error "That is not a valid SteamID64." | `Controllers/Admin/CommunityAdminController.cs:64` | `pending` |  |

## Welcome page (`/` with the ranking module off)

| Item | Where today | Status | Notes |
|---|---|---|---|
| Page title: the site name | `Views/Home/Welcome.cshtml:4` | `pending` |  |
| Box with the site name and "Use the menu to see what is available on this site." | `Views/Home/Welcome.cshtml:6` | `pending` |  |

## Layout

| Item | Where today | Status | Notes |
|---|---|---|---|
| Bootstrap 5 styles and `bootstrap.bundle` | `Views/Shared/_Bootstrap5Layout.cshtml:19` | `pending` |  |
| htmx loaded on every page | `Views/Shared/_Bootstrap5Layout.cshtml:114` | `pending` |  |
| `site.css` classes used by these pages (status dot, wizard steps, configuration block, avatars) | `Views/Shared/_Bootstrap5Layout.cshtml:20` | `pending` |  |
| Error and "Saved." messages at the top of the page | `Views/Shared/_Bootstrap5Layout.cshtml:100` | `pending` |  |
