# Site parity: layout, ranking, history, patents and Anti-Cheat

🌐 **English** | [Português](site.pt.md) | [Español](site.es.md)

Every item of the old site (`l4d2-playstats-web`) for these screens, checked one by one against the hub. `Where` points to the old file, relative to `L4D2PlayStats.Web/L4D2PlayStats.Web`.

Status: `done` (same content and look), `adapted` (present, changed for the stated reason), `not ported` (cannot exist in the hub, with the reason).

## Layout (every page)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Charset, IE compatibility and viewport metas | Views/Shared/_Layout.cshtml:28-30 | done | |
| Meta description | _Layout.cshtml:31 | adapted | Same text per page; the default names the site (`SiteSettings.Name`) instead of the fixed "L4D2 Competitive". |
| Meta keywords | _Layout.cshtml:32 | adapted | The fixed list became the optional setting `SiteSettings.MetaKeywords`. |
| Meta author | _Layout.cshtml:33 | adapted | Site name instead of the fixed name. |
| Meta robots (index or noindex per page) | _Layout.cshtml:34 | done | |
| Canonical link | _Layout.cshtml:36 | adapted | Built from the public address in the settings, or from the request when it is empty, instead of the fixed `https://l4d2.com.br`. |
| Open Graph title, description, URL, type, site name and locale | _Layout.cshtml:37-45 | done | |
| Open Graph image, secure URL and alt | _Layout.cshtml:41-43 | adapted | The page's own image as before. The default image was a fixed file in the old storage; now it is the site logo when one is uploaded, and the tags are left out when there is none. |
| Twitter card, title, description, image and alt | _Layout.cshtml:47-51 | done | `summary` instead of `summary_large_image` when there is no image. |
| Title as "Page \| Site" | _Layout.cshtml:53, PageMetadata/Infrastructure/PageMetadata.cs | done | |
| Favicon | _Layout.cshtml:55 | done | |
| lity stylesheet and script | _Layout.cshtml:56, 535 | done | |
| Dark theme (default) and light theme stylesheets chosen by a cookie | _Layout.cshtml:58-65 | done | Cookie `hub_theme`. |
| Page styles: container width, link colour, navbar, logo, header icons, helpers (`vertical-align-middle`, `bold`, `line-break-anywhere`), tooltip width, copy icon | _Layout.cshtml:67-121 | done | Copied as they were. |
| Match card styles (`.match ...`) | _Layout.cshtml:123-143 | done | |
| `show-mobile` / `hide-mobile`, server badge, floating icon, "it is me" row colour per theme | _Layout.cshtml:145-178 | done | |
| `slim` column, phone rules for match cards and `table-responsive` with `data-label` | _Layout.cshtml:180-234 | done | |
| Narrow navbar padding between 768 and 930 px | _Layout.cshtml:236-241 | done | |
| Stars pill styles | _Layout.cshtml:243-278 | done | |
| `Styles` and `Scripts` sections | _Layout.cshtml:281, 536 | done | |
| `imgOnError` (empty avatar fallback) | _Layout.cshtml:285-288 | done | |
| `copyTextToClipboard` with popover and failure message | _Layout.cshtml:290-304 | done | |
| Navbar toggle button for phones | _Layout.cshtml:313-318 | done | |
| Logo linking to the home page | _Layout.cshtml:319-321 | adapted | The logo is uploaded by the administrator (it was a fixed file with the old site's name). Without one, the site name is shown as text. |
| Players online badge next to the logo, pulsating, linking to the server page | _Layout.cshtml:322-324 | done | Shown only when the live server module is on. |
| Menu: Ranking, Last matches, Server, Skins, Anti-Cheat | _Layout.cshtml:329-343 | done | Each item follows its module. Community (new) comes after them. |
| Menu "More": Players, Patents, History, with icons and a divider | _Layout.cshtml:344-369 | done | |
| Menu "Admin" for administrators: Chat bans, Skin purchases, Skin prices, with icons | _Layout.cshtml:370-398 | done | Plus the hub's Administration area and the Anti-Cheat settings. |
| Stars pill with the signed-in player's balance, tooltip and label | _Layout.cshtml:403-411, 647-681 | done | Reads `GET /api/shop/wallet`, cached in the session storage as before. |
| Signed-in player's avatar linking to the player page, with the name on phones | _Layout.cshtml:412-419 | done | |
| Settings menu: switch theme | _Layout.cshtml:430-447, Controllers/HomeController.cs:42 | done | `/theme/{theme}`. |
| Settings menu: English, Português, Español (the current one hidden) | _Layout.cshtml:449-475, 545-554, 690-703 | adapted | Plain links (`/language?culture=`) instead of an AJAX POST followed by a reload. Same result, works without script. |
| Settings menu: sign in with Steam / sign out | _Layout.cshtml:477-494 | done | |
| Footer links (server repository, GitHub, Steam) | _Layout.cshtml:507-521 | adapted | The three fixed links became the setting `SiteSettings.FooterLinks` (one per line). The default is a link to the project. |
| Floating "connect" icon with the server address in the tooltip, hidden on phones | _Layout.cshtml:524-529, Controllers/ConnectController.cs | adapted | The address comes from the registered game server with the most players (`/connect`), not from a fixed setting. Hidden when no server has an address. |
| jQuery, Bootstrap, diffDOM, Chart.js and lity scripts | _Layout.cshtml:531-535 | done | |
| Players online in the browser tab title ("🟢 5/8 \| ...") and the 4 second refresh | _Layout.cshtml:539-543, 585-608 | done | `GET /api/server` with the same fields (`players`, `max_players`). |
| `pulsate` animation | _Layout.cshtml:556-583, 705-707 | done | |
| Charts created from `data-chart` when they scroll into view | _Layout.cshtml:610-641, 683-685, 713 | done | |
| `isObject` helper | _Layout.cshtml:643-645 | done | |
| Tooltips on every `data-toggle="tooltip"` | _Layout.cshtml:688 | done | |
| Google Analytics tag | _Layout.cshtml:717-723 | adapted | The fixed ID became the optional setting `SiteSettings.GoogleAnalyticsId`. |
| HTML minification and HTTP compression | Program.cs:9-16, 65 | adapted | Responses are compressed (Brotli/Gzip). The markup is not minified: no visible effect, and one dependency less. |
| Warning in the log for every request that ends in an error, and the exception for failures | Middlewares/ErrorLoggingMiddleware.cs | done | Through the application's logger; a Linux install keeps it in the system journal instead of `logs/app.log`. |
| JSON without nulls and with enums as text | Program.cs:21-25 | done | |
| Steam sign-in with a 30 day sliding cookie | Program.cs:28-39 | done | |
| Conventional addresses such as `/Home/SetTheme`, `/Language/Change`, `/Account/Login` | Program.cs:67 | adapted | The hub uses explicit addresses (`/theme/...`, `/language`, `/login`, `/logout`). Addresses people actually open (`/`, `/history`, `/patents`, `/players`, `/skins`, `/anti-cheat`) are the same. |
| `POST /api/cache/clear` | Controllers/Api/CacheController.cs | not ported | The old site cached what it read from a separate API and had to be told to forget it. In the hub the site and the API are one application, and every upload refreshes the ranking by itself. |

## Home: ranking (`/`)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Page metadata: title "Ranking", description, image and name of the first player | PageMetadata/HomePageMetadata.cs | done | |
| Page styles: medal size, thin progress bar, red bar colour, table and paragraph margins | Views/Home/Index.cshtml:18-46 | done | |
| Header "Top 100 players" | Index.cshtml:75 | done | |
| "Next reset in N day(s)" | Index.cshtml:78, Models/RankingPeriodModel.cs | adapted | Counts the days to the end of the current season, whatever its length, instead of fixed two-month periods. |
| Link "View previous rankings" | Index.cshtml:80 | done | |
| Table with stripes, hover, borders, condensed | Index.cshtml:83 | done | |
| Columns: #, Patent, Player (avatar and name), XP, Wins, Loss, SI MVP awards, CI MVP awards, Win rate, Games, Rage quit | Index.cshtml:85-97 | done | |
| Columns hidden on phones (XP, Loss, MVPs, Win rate, Games, Rage quit) | Index.cshtml:89-96 | done | |
| Top 100 listed, first 50 visible, "Show more" button for the rest | Index.cshtml:103, 109, 198-207, 50-57 | done | |
| Signed-in player's row highlighted | Index.cshtml:109 | done | |
| Medals for the first three places with tooltip; "#N" for the others | Index.cshtml:111-125 | done | |
| Patent image with "Level NN - Name" tooltip | Index.cshtml:127-129 | done | |
| Avatar linking to the player page, with fallback image | Index.cshtml:130-134 | done | Avatars are downloaded on demand, also without a Steam API key. |
| Player name, shortened when too long, linking to the player page | Index.cshtml:136 | done | |
| "Level Up!" / "Level Drop!" pulsating labels with tooltip | Index.cshtml:137-148 | done | |
| XP progress bar: type and pulse by situation, striped at the maximum level, second segment for the last game's gain or loss | Index.cshtml:150-157, Models/RankingModel.cs | done | |
| Progress tooltip: level change, maximum patent, last game XP, next level, XP of total, percentage | Models/RankingModel.cs:44-71 | done | |
| Wins with the tooltip "Wins - N% Win rate" | Index.cshtml:158-162 | done | |
| Loss, SI MVP, CI MVP, Win rate, Games, Rage quit, each with its tooltip | Index.cshtml:163-192 | done | |
| With no ranking yet: the top six of the last ranking | Index.cshtml:60-65, Views/Home/_LastRankingTopSix.cshtml | done | |
| With no ranking and no history: the message "There is no ranking data at the moment..." | Index.cshtml:66-71 | done | |
| Top six: trophy, header, reference period | _LastRankingTopSix.cshtml:36-61 | adapted | The period text covers a season of any length. |
| Top six: one panel per player (highlighted if it is the signed-in player), place label by colour, name, round avatar, wins and losses, labels for win rate, SI MVP, CI MVP, games and rage quit | _LastRankingTopSix.cshtml:63-111 | done | |
| Top six: "View full ranking" button | _LastRankingTopSix.cshtml:113-115 | done | Opens that period in the history. |
| With the ranking module off | (new) | adapted | A short welcome page; the old site had no modules. |

## History (`/history/{id?}`)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Page metadata: "History - period", description, image and name of the winner | PageMetadata/HistoryPageMetadata.cs | done | |
| Page styles | Views/History/Index.cshtml:13-32 | done | |
| Header "History" | Index.cshtml:34 | done | |
| Period selector showing the displayed period | Index.cshtml:36-85 | done | All time, each year, and the finished seasons of the current year, as in the old site. A season of an earlier year still opens by its address. |
| Period names: "From MM/YYYY to MM/YYYY", "Year of YYYY", "All time" | Index.cshtml:42-57 | done | |
| Default period: the current year | Controllers/HistoryController.cs:26 | done | |
| Table columns: #, Player (avatar and name), Wins, Loss, Games, SI MVP awards, CI MVP awards, Win rate, Rage quit | Index.cshtml:87-101 | done | |
| Columns hidden on phones | Index.cshtml:93-99 | done | |
| Top 100, signed-in player's row highlighted, medals, avatar, shortened name, tooltips on every number | Index.cshtml:103-171 | done | |
| Unknown period: "History not found" with a button back to the ranking, not indexed | Views/History/NoHistory.cshtml, PageMetadata/HistoryNotFoundPageMetadata.cs | done | |
| Addresses `/ranking/season/{id}`, `/ranking/year/{year}`, `/ranking/all-time` | (hub's first version) | adapted | Redirect to the history. |

## Patents (`/patents`)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Page metadata: title, description, image and name of the highest patent | PageMetadata/PatentsPageMetadata.cs | adapted | The description names the site instead of the fixed name. |
| Header "Patents" | Views/Patents/Index.cshtml:8 | done | |
| Panel "Understanding the patent system", first paragraph | Index.cshtml:10-17 | adapted | Says "season" and its length in months instead of the fixed two-month periods. |
| Second paragraph with the XP of a win, a loss, MVP SI, MVP CI and rage quit | Index.cshtml:18-20 | done | Values from the ranking settings. |
| Table: Level, image with tooltip, description, experience required, highest first | Index.cshtml:24-49 | done | |
| Panel "XP Points Breakdown" with the five lines | Index.cshtml:51-64 | done | |

## Anti-Cheat (`/anti-cheat`)

The page belongs to the Anti-Cheat module, which is off by default. When the administrator turns it on, the page and its menu item appear.

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Page metadata | PageMetadata/AntiCheatPageMetadata.cs | adapted | Names the site instead of the fixed name. |
| Page styles (hero, cards, steps, window mock-up, FAQ, phone rules) | Views/AntiCheat/Index.cshtml:16-306 | done | |
| Hero: kicker, title, lead | Index.cshtml:308-313 | adapted | The kicker names the site. |
| Download button | Index.cshtml:315-318 | adapted | Address from the Anti-Cheat service or from the setting; hidden when neither gives a secure address. |
| Version, size, release date | Index.cshtml:319-333 | done | Asked to the Anti-Cheat service (`/v1/client/version`), cached for 5 minutes, 30 seconds after a failure. |
| "Verify the file": SHA-256 with copy button and the PowerShell command | Index.cshtml:334-366 | done | |
| Requirements line | Index.cshtml:367-370 | done | |
| Window mock-up with the log lines | Index.cshtml:372-398 | done | |
| "How to use it": three steps | Index.cshtml:400-415 | done | |
| "What the window shows": four states with their colours | Index.cshtml:417-428 | done | |
| "Common problems": five questions that open and close | Index.cshtml:430-447 | done | |
| "About -insecure (no VAC)" | Index.cshtml:449-458 | adapted | Names the site instead of the fixed address. |
| Privacy: what is sent and what is never sent | Index.cshtml:459-484 | done | |
| Service and download addresses | Core/AppConsts.cs:15-16 | adapted | Settings on `/admin/anti-cheat` instead of fixed addresses. |
