# Site parity: skins

Port of the skin shop and its administrator pages from the old site (`l4d2-playstats-web`) to the hub.

Paths in the column "Where in the old site" are relative to `L4D2PlayStats.Web/L4D2PlayStats.Web` unless they start with `Core/` (`L4D2PlayStats.Web/L4D2PlayStats.Core`).

Status: `done` (same content and look), `adapted` (present, changed for the stated reason), `not ported` (cannot exist in the hub).

## Shop page (`/skins`)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Route `GET /skins`, menu item "Skins" active (`ViewBag.Skins`) | Controllers/SkinsController.cs:11-14 | pending | |
| Page metadata: title "Skins", description, image and alt of the most expensive skin | PageMetadata/SkinsPageMetadata.cs:9-18, Views/Skins/Index.cshtml:18 | pending | |
| Inline styles of the page (`.sk-*`, 700 lines) | Views/Skins/Index.cshtml:34-734 | pending | |
| Antiforgery token on the page | Views/Skins/Index.cshtml:736 | pending | |
| Hero: kicker "Skin shop", title "Skins", lead text | Views/Skins/Index.cshtml:738-742 | pending | |
| Hero: season pill with calendar icon, last day of the season and days left | Views/Skins/Index.cshtml:744-747, Core/Skins/SkinSeason.cs:11-14 | pending | |
| Hero: button "How it works" that opens the collapsed panel (caret rotates) | Views/Skins/Index.cshtml:750-752, 80-86 | pending | |
| Hero: button "How do I use my skins in game?" | Views/Skins/Index.cshtml:753-755 | pending | |
| Wallet, signed out: star, "Sign in with Steam to see..." and the sign-in button with return to the shop | Views/Skins/Index.cshtml:760-769, 31 | pending | |
| Wallet, stars unavailable: warning card | Views/Skins/Index.cshtml:770-778 | pending | |
| Wallet, signed in: star, balance, "star(s) available" label (singular and plural) | Views/Skins/Index.cshtml:781-788 | pending | |
| Wallet summary: stars earned as MVP and stars spent | Views/Skins/Index.cshtml:789 | pending | |
| Wallet: SI damage MVPs and common MVPs counts | Views/Skins/Index.cshtml:790, Core/Skins/Results/WalletResult.cs:5-6 | pending | |
| How it works: "1. Earn stars", "2. Buy and equip", "3. Switch in game" boxes | Views/Skins/Index.cshtml:795-809 | pending | |
| How it works: "Skins per season" banner with the season end and days left | Views/Skins/Index.cshtml:811-817 | pending | |
| How it works: credits line | Views/Skins/Index.cshtml:819 | pending | |
| Tabs: "Shop" with the count of skins | Views/Skins/Index.cshtml:1017-1022 | pending | |
| Tabs: "My skins" with the count of owned skins, only signed in | Views/Skins/Index.cshtml:1023-1030 | pending | |
| Tabs: "Authors" with the count of authors | Views/Skins/Index.cshtml:1031-1035 | pending | |
| Tab kept in the address (`#shop`, `#mine`, `#authors`) and restored on load | Views/Skins/Index.cshtml:1765-1771 | pending | |
| Toolbar: weapon filter with "All weapons" and the groups "Firearms" and "Melee weapons" | Views/Skins/Index.cshtml:1039-1054, Models/SkinsModel.cs:11-16 | pending | |
| Toolbar: rarity filter with "All rarities" and the five rarities, legendary first | Views/Skins/Index.cshtml:1055-1062, Models/SkinsModel.cs:37 | pending | |
| Toolbar: checkbox "Only the ones I can buy", only with a wallet | Views/Skins/Index.cshtml:1063-1068 | pending | |
| Filters applied in the browser to every tab; empty sections hidden; "No skins match these filters." | Views/Skins/Index.cshtml:1721-1763, 1093, 1118 | pending | |
| Shop tab: one section per rarity (legendary to common), coloured header | Views/Skins/Index.cshtml:1073-1092, 282-326 | pending | |
| Rarity header: count of skins and price range | Views/Skins/Index.cshtml:1083 | pending | |
| Order inside a rarity: price descending, weapon order, English title | Models/SkinsModel.cs:70-80 | pending | |
| Card: `col-xs-6 col-sm-4 col-md-3`, data attributes `weapon`, `rarity`, `affordable` | Views/Skins/Index.cshtml:980-981 | pending | |
| Card: anchor `id="skin-{id}"` (shop tab only), owned border, legendary glow | Views/Skins/Index.cshtml:982 | pending | |
| Card: thumbnail, lazy loaded | Views/Skins/Index.cshtml:984-985 | pending | |
| Card: weapon label over the thumbnail | Views/Skins/Index.cshtml:986 | pending | |
| Card: status label "Equipped" (with check icon) or "Owned" | Views/Skins/Index.cshtml:987-996 | pending | |
| Card: price tag with the star | Views/Skins/Index.cshtml:997 | pending | |
| Card: title with tooltip, single line with ellipsis | Views/Skins/Index.cshtml:1000 | pending | |
| Card: "by {author}" with link to the author's Steam profile | Views/Skins/Index.cshtml:1001-1006 | pending | |
| Card: rarity name in the rarity colour | Views/Skins/Index.cshtml:1007 | pending | |
| Card: photos cross-fade on hover (shots 1, 5 and 6, `-card` size, 1.6 s) | Views/Skins/Index.cshtml:1408-1474 | pending | |
| AVIF support probe; photos requested as `.avif` when supported, `.webp` otherwise | Views/Skins/Index.cshtml:1408-1421 | pending | |
| Action, signed out: "Sign in with Steam" link | Views/Skins/Index.cshtml:829-833 | pending | |
| Action, equipped: "Back to default" button | Views/Skins/Index.cshtml:835-841 | pending | |
| Action, owned: "Equip" button | Views/Skins/Index.cshtml:843-849 | pending | |
| Action, no wallet: disabled "Buy for {price}" | Views/Skins/Index.cshtml:851-855 | pending | |
| Action, not enough stars: disabled "Buy for {price}" and "{n} to go" | Views/Skins/Index.cshtml:857-862 | pending | |
| Action, can buy: "Buy for {price}" button with the cart icon | Views/Skins/Index.cshtml:864-866 | pending | |
| My skins tab: "How do I use my skins in game?" button | Views/Skins/Index.cshtml:1099-1103 | pending | |
| My skins tab: empty text "You do not own any skins yet..." | Views/Skins/Index.cshtml:1104-1107 | pending | |
| My skins tab: owned skins ordered by weapon, rarity, price | Views/Skins/Index.cshtml:1110-1118, Models/SkinsModel.cs:45-52 | pending | |
| Authors tab: intro text | Views/Skins/Index.cshtml:1123 | pending | |
| Authors tab: one box per author with profile link, count of skins (singular and plural) and the titles | Views/Skins/Index.cshtml:1124-1139, 25-30 | pending | |
| Details modal: opens on a click anywhere on the card except actions and the author link | Views/Skins/Index.cshtml:1588-1598, 1622-1628 | pending | |
| Details modal: opens by itself for `/skins#skin-{id}` | Views/Skins/Index.cshtml:1773-1778 | pending | |
| Details: gallery with the main photo and six thumbnails when the skin has photos | Views/Skins/Index.cshtml:884-903, 1476-1506 | pending | |
| Details: single picture when the skin has no photos, opens in the lightbox | Views/Skins/Index.cshtml:904-911 | pending | |
| Details: lightbox with zoom and pan (panzoom), click to zoom, wheel zoom | Views/Skins/Index.cshtml:1312, 1508-1542 | pending | |
| Details: lightbox previous and next buttons and arrow keys | Views/Skins/Index.cshtml:1544-1586, 606-636 | pending | |
| Details: tags for rarity, weapon, "Equipped" or "Owned" | Views/Skins/Index.cshtml:913-926 | pending | |
| Details: price | Views/Skins/Index.cshtml:927 | pending | |
| Details: author with link to the Steam profile | Views/Skins/Index.cshtml:928-936 | pending | |
| Details: localized description | Views/Skins/Index.cshtml:937-940 | pending | |
| Details: "Original" title and description from the Workshop | Views/Skins/Index.cshtml:880, 941-946 | pending | |
| Details: the same action button as the card | Views/Skins/Index.cshtml:947-949 | pending | |
| Details: "Chat command" with `!skin {id}` and the copy button, only when owned | Views/Skins/Index.cshtml:950-959 | pending | |
| Details: "View on Workshop" button | Views/Skins/Index.cshtml:960-962 | pending | |
| Details: "Give as a gift (admin)" button for administrators | Views/Skins/Index.cshtml:963-968, 1691-1699 | pending | |
| Details modal footer: "Are you the author?" link and "Close" | Views/Skins/Index.cshtml:1143-1161 | pending | |
| Author notice modal: four paragraphs | Views/Skins/Index.cshtml:1163-1180 | pending | |
| Author notice modal: "Message me on Steam" button | Views/Skins/Index.cshtml:1182-1184 | pending | |
| Author notice modal: "Open a request on GitHub" button, prefilled with the skin title, id and Workshop address | Views/Skins/Index.cshtml:1185-1187, 1600-1608 | pending | |
| Author notice modal stacked above the details modal | Views/Skins/Index.cshtml:1610-1620 | pending | |
| Confirm purchase modal: text, balance before and after, equip notice, season notice, final notice | Views/Skins/Index.cshtml:1197-1212, 1630-1650 | pending | |
| Purchase: `POST /api/shop/skins/{id}/buy`, success modal, page reloads on close | Views/Skins/Index.cshtml:1652-1667, Controllers/Api/SkinShopController.cs:31-41 | pending | |
| Equip: `POST /api/shop/skins/{id}/equip`, success modal, reload | Views/Skins/Index.cshtml:1669-1683, Controllers/Api/SkinShopController.cs:43-53 | pending | |
| Back to default: `POST /api/shop/weapons/{weapon}/unequip`, success modal, reload | Views/Skins/Index.cshtml:1701-1714, Controllers/Api/SkinShopController.cs:55-65 | pending | |
| Result modal and the error texts per status (`InsufficientStars`, `AlreadyOwned`, `StarsUnavailable`, `Busy`, `NotOwned`, `SkinNotFound`, generic) | Views/Skins/Index.cshtml:1214-1226, 1333-1341, 1367-1406 | pending | |
| "How to use your skins in game" modal: intro, table of six commands, weapon names, five rules | Views/Skins/Index.cshtml:1228-1304, 1685-1689 | pending | |
| Gift modal available to administrators on the shop page | Views/Skins/Index.cshtml:1306-1309 | pending | |
| Mobile: hero padding, title size and wallet width under 700 px; gallery with three columns under 480 px | Views/Skins/Index.cshtml:600-604, 719-732 | pending | |

## Gift modal (`_GiftModal`)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Modal "Give a skin as a gift" with the lead text | Views/Shared/_GiftModal.cshtml:61-72 | pending | |
| Player: chosen player box (avatar, name, Steam ID, SteamID64) or "No player chosen" | Views/Shared/_GiftModal.cshtml:74-77, 138-148 | pending | |
| Player: text field for a Steam ID in any format or a profile link; "Steam ID typed manually" | Views/Shared/_GiftModal.cshtml:78-85, 183-187 | pending | |
| Player: "Choose from the list" opens the player picker and comes back | Views/Shared/_GiftModal.cshtml:166-181 | pending | |
| Skin: chosen skin box (thumbnail, title, weapon, rarity, price) or "No skin chosen" | Views/Shared/_GiftModal.cshtml:87-90, 150-164 | pending | |
| Skin: "Choose a skin" opens the skin picker and comes back | Views/Shared/_GiftModal.cshtml:91-93, 189-198 | pending | |
| Confirm: validation messages for a missing player or skin | Views/Shared/_GiftModal.cshtml:200-213 | pending | |
| Confirm: `POST /api/shop/gift`, success message and reload, error per status, invalid Steam ID message on 400 | Views/Shared/_GiftModal.cshtml:215-237, Controllers/Api/SkinShopController.cs:85-96, 130-141 | pending | |
| `GiftModal.open({ skin })` preselects a skin | Views/Shared/_GiftModal.cshtml:239-251 | pending | |
| Gift: free, equipped at once, keeps who gave it | Core/Skins/Services/SkinShopService.cs:186-228 | pending | |

## Skin picker modal (`_SkinPickerModal`)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Large modal "Choose a skin" | Views/Shared/_SkinPickerModal.cshtml:117-126 | pending | |
| Search field (name, id or weapon), focused when the modal opens | Views/Shared/_SkinPickerModal.cshtml:128, 190-192, 222-224 | pending | |
| Weapon filter and rarity filter | Views/Shared/_SkinPickerModal.cshtml:129-141 | pending | |
| Grid of tiles ordered by rarity, price and weapon: thumbnail, title, weapon, rarity, price | Views/Shared/_SkinPickerModal.cshtml:11-17, 143-162 | pending | |
| "No skins match these filters." | Views/Shared/_SkinPickerModal.cshtml:163 | pending | |
| `SkinPicker.open(callback)` and `SkinPicker.find(id)` | Views/Shared/_SkinPickerModal.cshtml:237-260 | pending | |
| Mobile: two tiles per row under 700 px | Views/Shared/_SkinPickerModal.cshtml:111-115 | pending | |

## Skin prices (`/admin/skins/prices`)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Route `GET /admin/skins/prices`, administrators only, menu "Admin" and "Skin prices" active | Controllers/SkinsAdminController.cs:14-15, 71-75 | pending | |
| Title "Skin prices" and lead text | Views/SkinsAdmin/Prices.cshtml:11, 24-28 | pending | |
| Stat: skins with a changed price | Views/SkinsAdmin/Prices.cshtml:30-35 | pending | |
| Filters: search by name or id, rarity, weapon (submit on change), "Search" button, "Clear filters" | Views/SkinsAdmin/Prices.cshtml:37-59, Controllers/SkinsAdminController.cs:81-86 | pending | |
| List ordered by rarity, price and weapon; 20 per page | Controllers/SkinsAdminController.cs:22, 87-92 | pending | |
| Row: thumbnail, title, "weapon · rarity · id" | Views/SkinsAdmin/Prices.cshtml:66-78 | pending | |
| Row: current price; "Changed" label and the default price when it was changed | Views/SkinsAdmin/Prices.cshtml:79-87 | pending | |
| "No skins found." | Views/SkinsAdmin/Prices.cshtml:62-65 | pending | |
| Pagination "Showing a–b of n" that keeps the filters | Views/SkinsAdmin/Prices.cshtml:13, 92 | pending | |
| Price modal: picture, weapon, rarity, default price, price field (1 to 100,000), hint | Views/SkinsAdmin/Prices.cshtml:94-134, 181-198 | pending | |
| Price modal: "Save" (`POST /api/shop/skins/{id}/price`), message and reload | Views/SkinsAdmin/Prices.cshtml:157-179, 200-209, Controllers/Api/SkinShopController.cs:67-83 | pending | |
| Price modal: "Reset to default", only for a changed price | Views/SkinsAdmin/Prices.cshtml:192, 219-221 | pending | |
| Price modal: "Give as a gift" opens the gift modal with the skin | Views/SkinsAdmin/Prices.cshtml:211-217 | pending | |
| Price modal: invalid price message, price field focused and selected | Views/SkinsAdmin/Prices.cshtml:203-206, 223-225 | pending | |
| Price rules: a price equal to the default removes the change; keeps who changed it and when | Core/Skins/Services/SkinService.cs:41-64 | pending | |

## Skin purchases (`/admin/skins/purchases`)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Route `GET /admin/skins/purchases`, administrators only, menu "Admin" and "Skin purchases" active | Controllers/SkinsAdminController.cs:24-28 | pending | |
| Title "Skin purchases" | Views/SkinsAdmin/Purchases.cshtml:14, 269-270 | pending | |
| Header: "Refund everything" button, only when there is something to refund | Views/SkinsAdmin/Purchases.cshtml:271-276 | pending | |
| Header: "Give a skin" button opens the gift modal | Views/SkinsAdmin/Purchases.cshtml:277-279 | pending | |
| Stats: purchases, stars spent, gifts given | Views/SkinsAdmin/Purchases.cshtml:282-295 | pending | |
| Notice: purchases are refunded at the end of the season, with the date | Views/SkinsAdmin/Purchases.cshtml:297-300 | pending | |
| Filters: search by player, Steam ID or skin; "Search" button | Views/SkinsAdmin/Purchases.cshtml:302-307, Models/SkinPurchaseRowModel.cs:20-31 | pending | |
| Filters: "Filter by player" opens the player picker | Views/SkinsAdmin/Purchases.cshtml:308-313, 601-608 | pending | |
| Filters: chosen player chip with avatar, name, change and remove | Views/SkinsAdmin/Purchases.cshtml:314-326, 22-56 | pending | |
| Filters: "Clear filters" | Views/SkinsAdmin/Purchases.cshtml:327-330 | pending | |
| List ordered by date, newest first; 20 per page | Controllers/SkinsAdminController.cs:22, 43-54 | pending | |
| Row: skin thumbnail, title, "weapon · id" | Views/SkinsAdmin/Purchases.cshtml:356-360 | pending | |
| Row: player avatar, name and SteamID64 | Views/SkinsAdmin/Purchases.cshtml:361-367 | pending | |
| Row: price, or "Gift" label and "by {administrator}"; date | Views/SkinsAdmin/Purchases.cshtml:368-382 | pending | |
| "No purchases found." | Views/SkinsAdmin/Purchases.cshtml:334-337 | pending | |
| Pagination that keeps the search and the player | Views/SkinsAdmin/Purchases.cshtml:16, 387 | pending | |
| Details modal: hero in the rarity colour, cover that links to the skin in the shop, rarity, gift badge, title, "weapon · id", "View in the shop" | Views/SkinsAdmin/Purchases.cshtml:389-419, 558-576, 596-599 | pending | |
| Details modal: player box with avatar, name, id, "only this player's purchases" and "player's profile" buttons | Views/SkinsAdmin/Purchases.cshtml:420-433, 577-581 | pending | |
| Details modal: facts for price, balance before and after (purchases), gift given by (gifts), date | Views/SkinsAdmin/Purchases.cshtml:434-453, 582-588 | pending | |
| Details modal: "Refund purchase", confirmation text, "Confirm refund" (`POST /api/shop/refund`), message and reload | Views/SkinsAdmin/Purchases.cshtml:455-466, 610-637, Controllers/Api/SkinShopController.cs:98-109 | pending | |
| Refund everything modal: intro with totals, warning, checkbox that enables the button, running label | Views/SkinsAdmin/Purchases.cshtml:471-498, 643-660 | pending | |
| Refund everything: `POST /api/shop/refund-all`, summary, failed items message, reload | Views/SkinsAdmin/Purchases.cshtml:662-686, Controllers/Api/SkinShopController.cs:111-119 | pending | |
| Gift modal on the page | Views/SkinsAdmin/Purchases.cshtml:500 | pending | |
| Mobile: rows wrap and the player goes to a second line under 700 px; hero stacks under 480 px | Views/Shared/_AdminStyles.cshtml, Views/SkinsAdmin/Purchases.cshtml:254-263 | pending | |

## APIs

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| `GET /api/shop/wallet` for the signed-in player: `balance`, `stars`, `spent` (used by the stars in the navbar) | Controllers/Api/SkinShopController.cs:18-29 | pending | |
| `POST /api/shop/skins/{skinId}/buy`: `status`, `succeeded`, `price`, `balance` | Controllers/Api/SkinShopController.cs:31-41, 121-129 | pending | |
| `POST /api/shop/skins/{skinId}/equip` | Controllers/Api/SkinShopController.cs:43-53 | pending | |
| `POST /api/shop/weapons/{weapon}/unequip` | Controllers/Api/SkinShopController.cs:55-65 | pending | |
| `POST /api/shop/skins/{skinId}/price` (administrator): 400 out of range, 404 unknown skin, `price`, `defaultPrice`, `custom` | Controllers/Api/SkinShopController.cs:67-83 | pending | |
| `POST /api/shop/gift` (administrator): Steam ID in any format | Controllers/Api/SkinShopController.cs:85-96, 130-141 | pending | |
| `POST /api/shop/refund` (administrator) | Controllers/Api/SkinShopController.cs:98-109 | pending | |
| `POST /api/shop/refund-all` (administrator): `purchases`, `gifts`, `stars`, `players`, `failed` | Controllers/Api/SkinShopController.cs:111-119, Core/Skins/Results/RefundAllResult.cs | pending | |
| Antiforgery token required in the header `RequestVerificationToken` on every POST | Controllers/Api/SkinShopController.cs:32, 44, 56, 69, 87, 100, 113 | pending | |
| `GET /api/skins`: the skins someone owns (`id`, `weapon`), for the game server | Controllers/Api/SkinsController.cs:10-22 | pending | |
| `GET /api/players/skins`: skins and equipment of every player, for the game server | Controllers/Api/PlayersController.cs:33-46 | pending | |
| `GET /api/players/{communityId}/wallet`: `mvpSiDamage`, `mvpCommon`, `stars`, `spent`, `balance` | Controllers/Api/PlayersController.cs:48-56 | pending | |
| `POST /api/players/{communityId}/skins/equip` from the game server: equip or back to default | Controllers/Api/PlayersController.cs:58-69 | pending | |

## Shop rules and background work

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Stars: one for each SI damage MVP and each common MVP of the whole history | Core/Skins/Services/SkinShopService.cs:29-38, 373-382 | pending | |
| Buying: checks the skin, ownership and the balance; saves the price, the balance before and the player name; equips at once | Core/Skins/Services/SkinShopService.cs:69-138 | pending | |
| Equipping needs ownership; one skin per weapon | Core/Skins/Services/SkinShopService.cs:140-157 | pending | |
| Back to default removes the equipment of the weapon | Core/Skins/Services/SkinShopService.cs:159-184 | pending | |
| Refund: returns the stars, removes the skin and its equipment | Core/Skins/Services/SkinShopService.cs:230-311 | pending | |
| Season refund: every ten minutes, purchases made before the current season are refunded; gifts are kept | BackgroundServices/SkinSeasonRefundBackgroundService.cs:8-35, Core/Skins/Services/SkinShopService.cs:246-260, 313-316 | pending | |
| Catalog: id, weapon, rarity, price, Workshop id, photos, author, titles and descriptions in three languages, original title and description | Core/Skins/Skin.cs, Core/Skins/SkinAuthor.cs, Core/Skins/Data/skins.json | pending | |
| Thumbnails at `/imgs/skins/{id}.jpg`, photos at `{SkinShotsUrl}/{id}/` | Core/Skins/Skin.cs:33, Views/Skins/Index.cshtml:871-874 | pending | |

## Hub-only pages

| Item | Where in the hub | Status | Notes |
|---|---|---|---|
| Skin catalog page (`/admin/skins/catalog`): totals, last change, import from a file, import from the configured address | Views/SkinsAdmin/Catalog.cshtml | pending | |
| Shop settings page (`/admin/skins/settings`): refund at the end of a season, address of the assets, address of the catalog | Views/SkinsAdmin/Settings.cshtml | pending | |
| Tabs that link the four administrator pages | Views/Shared/_SkinsAdminNav.cshtml | pending | |
