# Site parity: skins

🌐 **English** | [Português](skins.pt.md) | [Español](skins.es.md)

Port of the skin shop and its administrator pages from the old site (`l4d2-playstats-web`) to the hub.

Paths in the column "Where in the old site" are relative to `L4D2PlayStats.Web/L4D2PlayStats.Web` unless they start with `Core/` (`L4D2PlayStats.Web/L4D2PlayStats.Core`).

Status: `done` (same content and look), `adapted` (present, changed for the stated reason), `not ported` (cannot exist in the hub).

## Shop page (`/skins`)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Route `GET /skins`, menu item "Skins" active (`ViewBag.Skins`) | Controllers/SkinsController.cs:11-14 | done |  |
| Page metadata: title "Skins", description, image and alt of the most expensive skin | PageMetadata/SkinsPageMetadata.cs:9-18, Views/Skins/Index.cshtml:18 | adapted | The description takes the site name from the settings instead of naming "L4D2 Competitive". The image is the thumbnail of the skin, as an absolute address: the site's own `/imgs/skins/{id}.jpg` for the skins that come with the hub, as in the old site, otherwise the one under the assets address. |
| Inline styles of the page (`.sk-*`, 700 lines) | Views/Skins/Index.cshtml:34-734 | done | Copied unchanged. |
| Antiforgery token on the page | Views/Skins/Index.cshtml:736 | done |  |
| Hero: kicker "Skin shop", title "Skins", lead text | Views/Skins/Index.cshtml:738-742 | done |  |
| Hero: season pill with calendar icon, last day of the season and days left | Views/Skins/Index.cshtml:744-747, Core/Skins/SkinSeason.cs:11-14 | done | Dates come from the hub season in the site time zone, of any length. Hidden when the administrator turns the season refund off. |
| Hero: button "How it works" that opens the collapsed panel (caret rotates) | Views/Skins/Index.cshtml:750-752, 80-86 | done |  |
| Hero: button "How do I use my skins in game?" | Views/Skins/Index.cshtml:753-755 | done |  |
| Wallet, signed out: star, "Sign in with Steam to see..." and the sign-in button with return to the shop | Views/Skins/Index.cshtml:760-769, 31 | done |  |
| Wallet, stars unavailable: warning card | Views/Skins/Index.cshtml:770-778 | adapted | The markup and the text are kept, but the hub reads the stars from its own database, so the wallet of a signed-in player is always available and this card never shows. |
| Wallet, signed in: star, balance, "star(s) available" label (singular and plural) | Views/Skins/Index.cshtml:781-788 | done |  |
| Wallet summary: stars earned as MVP and stars spent | Views/Skins/Index.cshtml:789 | done |  |
| Wallet: SI damage MVPs and common MVPs counts | Views/Skins/Index.cshtml:790, Core/Skins/Results/WalletResult.cs:5-6 | done | Back end added: `SkinStarsService` splits the stars into the two kinds of MVP. |
| How it works: "1. Earn stars", "2. Buy and equip", "3. Switch in game" boxes | Views/Skins/Index.cshtml:795-809 | done | With the season refund off, box 2 ends with "is yours forever" instead of "until the end of the ranking season". |
| How it works: "Skins per season" banner with the season end and days left | Views/Skins/Index.cshtml:811-817 | done | The text says the real length of the season ("every 2 months" by default, "every month" or "every N months" otherwise). Hidden with the season refund off. |
| How it works: credits line | Views/Skins/Index.cshtml:819 | done |  |
| Tabs: "Shop" with the count of skins | Views/Skins/Index.cshtml:1017-1022 | done |  |
| Tabs: "My skins" with the count of owned skins, only signed in | Views/Skins/Index.cshtml:1023-1030 | done |  |
| Tabs: "Authors" with the count of authors | Views/Skins/Index.cshtml:1031-1035 | done |  |
| Tab kept in the address (`#shop`, `#mine`, `#authors`) and restored on load | Views/Skins/Index.cshtml:1765-1771 | done |  |
| Toolbar: weapon filter with "All weapons" and the groups "Firearms" and "Melee weapons" | Views/Skins/Index.cshtml:1039-1054, Models/SkinsModel.cs:11-16 | done | The fifteen weapons of the old site, plus any other weapon an imported catalog brings. |
| Toolbar: rarity filter with "All rarities" and the five rarities, legendary first | Views/Skins/Index.cshtml:1055-1062, Models/SkinsModel.cs:37 | done |  |
| Toolbar: checkbox "Only the ones I can buy", only with a wallet | Views/Skins/Index.cshtml:1063-1068 | done |  |
| Filters applied in the browser to every tab; empty sections hidden; "No skins match these filters." | Views/Skins/Index.cshtml:1721-1763, 1093, 1118 | done |  |
| Shop tab: one section per rarity (legendary to common), coloured header | Views/Skins/Index.cshtml:1073-1092, 282-326 | done |  |
| Rarity header: count of skins and price range | Views/Skins/Index.cshtml:1083 | done |  |
| Order inside a rarity: price descending, weapon order, English title | Models/SkinsModel.cs:70-80 | done |  |
| Card: `col-xs-6 col-sm-4 col-md-3`, data attributes `weapon`, `rarity`, `affordable` | Views/Skins/Index.cshtml:980-981 | done |  |
| Card: anchor `id="skin-{id}"` (shop tab only), owned border, legendary glow | Views/Skins/Index.cshtml:982 | done |  |
| Card: thumbnail, lazy loaded | Views/Skins/Index.cshtml:984-985 | done | Served by the site from `/imgs/skins/{id}.jpg`, as in the old site, for the 196 skins that come with the hub. A skin added later is loaded from `{assets}/skins/{id}.jpg`. |
| Card: weapon label over the thumbnail | Views/Skins/Index.cshtml:986 | done |  |
| Card: status label "Equipped" (with check icon) or "Owned" | Views/Skins/Index.cshtml:987-996 | done |  |
| Card: price tag with the star | Views/Skins/Index.cshtml:997 | done |  |
| Card: title with tooltip, single line with ellipsis | Views/Skins/Index.cshtml:1000 | done |  |
| Card: "by {author}" with link to the author's Steam profile | Views/Skins/Index.cshtml:1001-1006 | done | An author imported without a Steam ID is shown without the link. |
| Card: rarity name in the rarity colour | Views/Skins/Index.cshtml:1007 | done |  |
| Card: photos cross-fade on hover (shots 1, 5 and 6, `-card` size, 1.6 s) | Views/Skins/Index.cshtml:1408-1474 | done | Checked against the production photos through a local proxy. |
| AVIF support probe; photos requested as `.avif` when supported, `.webp` otherwise | Views/Skins/Index.cshtml:1408-1421 | done |  |
| Action, signed out: "Sign in with Steam" link | Views/Skins/Index.cshtml:829-833 | done |  |
| Action, equipped: "Back to default" button | Views/Skins/Index.cshtml:835-841 | done |  |
| Action, owned: "Equip" button | Views/Skins/Index.cshtml:843-849 | done |  |
| Action, no wallet: disabled "Buy for {price}" | Views/Skins/Index.cshtml:851-855 | done |  |
| Action, not enough stars: disabled "Buy for {price}" and "{n} to go" | Views/Skins/Index.cshtml:857-862 | done |  |
| Action, can buy: "Buy for {price}" button with the cart icon | Views/Skins/Index.cshtml:864-866 | done |  |
| My skins tab: "How do I use my skins in game?" button | Views/Skins/Index.cshtml:1099-1103 | done |  |
| My skins tab: empty text "You do not own any skins yet..." | Views/Skins/Index.cshtml:1104-1107 | done |  |
| My skins tab: owned skins ordered by weapon, rarity, price | Views/Skins/Index.cshtml:1110-1118, Models/SkinsModel.cs:45-52 | done | A skin that left the catalog stays here for its owner, with a "No longer for sale" label in the details. |
| Authors tab: intro text | Views/Skins/Index.cshtml:1123 | done |  |
| Authors tab: one box per author with profile link, count of skins (singular and plural) and the titles | Views/Skins/Index.cshtml:1124-1139, 25-30 | done | Authors without a Steam ID are grouped by name and shown without the link. |
| Details modal: opens on a click anywhere on the card except actions and the author link | Views/Skins/Index.cshtml:1588-1598, 1622-1628 | done |  |
| Details modal: opens by itself for `/skins#skin-{id}` | Views/Skins/Index.cshtml:1773-1778 | done |  |
| Details: gallery with the main photo and six thumbnails when the skin has photos | Views/Skins/Index.cshtml:884-903, 1476-1506 | done | Shown when the skin has photos. The photos come from the assets address, which by default is the one of the old site. |
| Details: single picture when the skin has no photos, opens in the lightbox | Views/Skins/Index.cshtml:904-911 | done |  |
| Details: lightbox with zoom and pan (panzoom), click to zoom, wheel zoom | Views/Skins/Index.cshtml:1312, 1508-1542 | done |  |
| Details: lightbox previous and next buttons and arrow keys | Views/Skins/Index.cshtml:1544-1586, 606-636 | done |  |
| Details: tags for rarity, weapon, "Equipped" or "Owned" | Views/Skins/Index.cshtml:913-926 | done |  |
| Details: price | Views/Skins/Index.cshtml:927 | done |  |
| Details: author with link to the Steam profile | Views/Skins/Index.cshtml:928-936 | done |  |
| Details: localized description | Views/Skins/Index.cshtml:937-940 | done |  |
| Details: "Original" title and description from the Workshop | Views/Skins/Index.cshtml:880, 941-946 | done |  |
| Details: the same action button as the card | Views/Skins/Index.cshtml:947-949 | done |  |
| Details: "Chat command" with `!skin {id}` and the copy button, only when owned | Views/Skins/Index.cshtml:950-959 | done | Uses the shared `_CopyButton` partial: same icon, popover and behaviour. |
| Details: "View on Workshop" button | Views/Skins/Index.cshtml:960-962 | done | Hidden for a skin imported without a Workshop id. |
| Details: "Give as a gift (admin)" button for administrators | Views/Skins/Index.cshtml:963-968, 1691-1699 | done | Not offered for a skin that is no longer for sale. |
| Details modal footer: "Are you the author?" link and "Close" | Views/Skins/Index.cshtml:1143-1161 | done | Hidden only when both addresses of the notice are empty in the shop settings. |
| Author notice modal: four paragraphs | Views/Skins/Index.cshtml:1163-1180 | done |  |
| Author notice modal: "Message me on Steam" button | Views/Skins/Index.cshtml:1182-1184 | adapted | The address was the owner's Steam profile, fixed in the view. It is now the "Steam profile for contact" of the shop settings; the button is hidden while that is empty, which is the default. |
| Author notice modal: "Open a request on GitHub" button, prefilled with the skin title, id and Workshop address | Views/Skins/Index.cshtml:1185-1187, 1600-1608 | adapted | The address is the "Address for removal requests" of the shop settings. It starts as the new issue page of `l4d2-zone-server-assets`, as in the old site, and is prefilled the same way. |
| Author notice modal stacked above the details modal | Views/Skins/Index.cshtml:1610-1620 | done |  |
| Confirm purchase modal: text, balance before and after, equip notice, season notice, final notice | Views/Skins/Index.cshtml:1197-1212, 1630-1650 | done | With the season refund off the season notice is left out and the last line says "Purchases cannot be undone." |
| Purchase: `POST /api/shop/skins/{id}/buy`, success modal, page reloads on close | Views/Skins/Index.cshtml:1652-1667, Controllers/Api/SkinShopController.cs:31-41 | done |  |
| Equip: `POST /api/shop/skins/{id}/equip`, success modal, reload | Views/Skins/Index.cshtml:1669-1683, Controllers/Api/SkinShopController.cs:43-53 | done |  |
| Back to default: `POST /api/shop/weapons/{weapon}/unequip`, success modal, reload | Views/Skins/Index.cshtml:1701-1714, Controllers/Api/SkinShopController.cs:55-65 | done |  |
| Result modal and the error texts per status (`InsufficientStars`, `AlreadyOwned`, `StarsUnavailable`, `Busy`, `NotOwned`, `SkinNotFound`, generic) | Views/Skins/Index.cshtml:1214-1226, 1333-1341, 1367-1406 | done | The hub returns the same status names. |
| "How to use your skins in game" modal: intro, table of six commands, weapon names, five rules | Views/Skins/Index.cshtml:1228-1304, 1685-1689 | done |  |
| Gift modal available to administrators on the shop page | Views/Skins/Index.cshtml:1306-1309 | done |  |
| Mobile: hero padding, title size and wallet width under 700 px; gallery with three columns under 480 px | Views/Skins/Index.cshtml:600-604, 719-732 | done |  |

## Gift modal (`_GiftModal`)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Modal "Give a skin as a gift" with the lead text | Views/Shared/_GiftModal.cshtml:61-72 | done |  |
| Player: chosen player box (avatar, name, Steam ID, SteamID64) or "No player chosen" | Views/Shared/_GiftModal.cshtml:74-77, 138-148 | done | The avatar comes from `/avatar/{id}`. |
| Player: text field for a Steam ID in any format or a profile link; "Steam ID typed manually" | Views/Shared/_GiftModal.cshtml:78-85, 183-187 | done |  |
| Player: "Choose from the list" opens the player picker and comes back | Views/Shared/_GiftModal.cshtml:166-181 | done |  |
| Skin: chosen skin box (thumbnail, title, weapon, rarity, price) or "No skin chosen" | Views/Shared/_GiftModal.cshtml:87-90, 150-164 | done |  |
| Skin: "Choose a skin" opens the skin picker and comes back | Views/Shared/_GiftModal.cshtml:91-93, 189-198 | done |  |
| Confirm: validation messages for a missing player or skin | Views/Shared/_GiftModal.cshtml:200-213 | done |  |
| Confirm: `POST /api/shop/gift`, success message and reload, error per status, invalid Steam ID message on 400 | Views/Shared/_GiftModal.cshtml:215-237, Controllers/Api/SkinShopController.cs:85-96, 130-141 | done | Back end added: `SkinRecipient` reads `STEAM_x:y:z`, `[U:1:n]`, SteamID64 and profile links. |
| `GiftModal.open({ skin })` preselects a skin | Views/Shared/_GiftModal.cshtml:239-251 | done |  |
| Gift: free, equipped at once, keeps who gave it | Core/Skins/Services/SkinShopService.cs:186-228 | done |  |

## Skin picker modal (`_SkinPickerModal`)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Large modal "Choose a skin" | Views/Shared/_SkinPickerModal.cshtml:117-126 | done |  |
| Search field (name, id or weapon), focused when the modal opens | Views/Shared/_SkinPickerModal.cshtml:128, 190-192, 222-224 | done |  |
| Weapon filter and rarity filter | Views/Shared/_SkinPickerModal.cshtml:129-141 | done |  |
| Grid of tiles ordered by rarity, price and weapon: thumbnail, title, weapon, rarity, price | Views/Shared/_SkinPickerModal.cshtml:11-17, 143-162 | done | Thumbnails served by the site, as on the shop page. |
| "No skins match these filters." | Views/Shared/_SkinPickerModal.cshtml:163 | done |  |
| `SkinPicker.open(callback)` and `SkinPicker.find(id)` | Views/Shared/_SkinPickerModal.cshtml:237-260 | done |  |
| Mobile: two tiles per row under 700 px | Views/Shared/_SkinPickerModal.cshtml:111-115 | done |  |

## Skin prices (`/admin/skins/prices`)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Route `GET /admin/skins/prices`, administrators only, menu "Admin" and "Skin prices" active | Controllers/SkinsAdminController.cs:14-15, 71-75 | done |  |
| Title "Skin prices" and lead text | Views/SkinsAdmin/Prices.cshtml:11, 24-28 | done |  |
| Stat: skins with a changed price | Views/SkinsAdmin/Prices.cshtml:30-35 | done |  |
| Filters: search by name or id, rarity, weapon (submit on change), "Search" button, "Clear filters" | Views/SkinsAdmin/Prices.cshtml:37-59, Controllers/SkinsAdminController.cs:81-86 | done |  |
| List ordered by rarity, price and weapon; 20 per page | Controllers/SkinsAdminController.cs:22, 87-92 | done | The skin id breaks ties so pages are stable. |
| Row: thumbnail, title, "weapon · rarity · id" | Views/SkinsAdmin/Prices.cshtml:66-78 | done |  |
| Row: current price; "Changed" label and the default price when it was changed | Views/SkinsAdmin/Prices.cshtml:79-87 | done | A skin that left the catalog also gets a "No longer for sale" label. |
| "No skins found." | Views/SkinsAdmin/Prices.cshtml:62-65 | done |  |
| Pagination "Showing a–b of n" that keeps the filters | Views/SkinsAdmin/Prices.cshtml:13, 92 | done |  |
| Price modal: picture, weapon, rarity, default price, price field (1 to 100,000), hint | Views/SkinsAdmin/Prices.cshtml:94-134, 181-198 | done |  |
| Price modal: "Save" (`POST /api/shop/skins/{id}/price`), message and reload | Views/SkinsAdmin/Prices.cshtml:157-179, 200-209, Controllers/Api/SkinShopController.cs:67-83 | done |  |
| Price modal: "Reset to default", only for a changed price | Views/SkinsAdmin/Prices.cshtml:192, 219-221 | done |  |
| Price modal: "Give as a gift" opens the gift modal with the skin | Views/SkinsAdmin/Prices.cshtml:211-217 | done | Hidden for a skin that is no longer for sale. |
| Price modal: invalid price message, price field focused and selected | Views/SkinsAdmin/Prices.cshtml:203-206, 223-225 | done |  |
| Price rules: a price equal to the default removes the change; keeps who changed it and when | Core/Skins/Services/SkinService.cs:41-64 | done |  |

## Skin purchases (`/admin/skins/purchases`)

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Route `GET /admin/skins/purchases`, administrators only, menu "Admin" and "Skin purchases" active | Controllers/SkinsAdminController.cs:24-28 | adapted | The hub address is `/admin/skins`; `/admin/skins/purchases` redirects to it permanently and keeps the query string. |
| Title "Skin purchases" | Views/SkinsAdmin/Purchases.cshtml:14, 269-270 | done |  |
| Header: "Refund everything" button, only when there is something to refund | Views/SkinsAdmin/Purchases.cshtml:271-276 | done |  |
| Header: "Give a skin" button opens the gift modal | Views/SkinsAdmin/Purchases.cshtml:277-279 | done |  |
| Stats: purchases, stars spent, gifts given | Views/SkinsAdmin/Purchases.cshtml:282-295 | done |  |
| Notice: purchases are refunded at the end of the season, with the date | Views/SkinsAdmin/Purchases.cshtml:297-300 | done | Hidden with the season refund off. |
| Filters: search by player, Steam ID or skin; "Search" button | Views/SkinsAdmin/Purchases.cshtml:302-307, Models/SkinPurchaseRowModel.cs:20-31 | done | The name is the one saved with the purchase, then the name of the player profile, then the SteamID64. |
| Filters: "Filter by player" opens the player picker | Views/SkinsAdmin/Purchases.cshtml:308-313, 601-608 | done |  |
| Filters: chosen player chip with avatar, name, change and remove | Views/SkinsAdmin/Purchases.cshtml:314-326, 22-56 | done |  |
| Filters: "Clear filters" | Views/SkinsAdmin/Purchases.cshtml:327-330 | done |  |
| List ordered by date, newest first; 20 per page | Controllers/SkinsAdminController.cs:22, 43-54 | done |  |
| Row: skin thumbnail, title, "weapon · id" | Views/SkinsAdmin/Purchases.cshtml:356-360 | done |  |
| Row: player avatar, name and SteamID64 | Views/SkinsAdmin/Purchases.cshtml:361-367 | done |  |
| Row: price, or "Gift" label and "by {administrator}"; date | Views/SkinsAdmin/Purchases.cshtml:368-382 | adapted | The date is shown in the time zone of the site settings; the old site used the local time of the web server. |
| "No purchases found." | Views/SkinsAdmin/Purchases.cshtml:334-337 | done |  |
| Pagination that keeps the search and the player | Views/SkinsAdmin/Purchases.cshtml:16, 387 | done |  |
| Details modal: hero in the rarity colour, cover that links to the skin in the shop, rarity, gift badge, title, "weapon · id", "View in the shop" | Views/SkinsAdmin/Purchases.cshtml:389-419, 558-576, 596-599 | done |  |
| Details modal: player box with avatar, name, id, "only this player's purchases" and "player's profile" buttons | Views/SkinsAdmin/Purchases.cshtml:420-433, 577-581 | done | The profile button opens `/player/{id}`, ported by the players area. |
| Details modal: facts for price, balance before and after (purchases), gift given by (gifts), date | Views/SkinsAdmin/Purchases.cshtml:434-453, 582-588 | done |  |
| Details modal: "Refund purchase", confirmation text, "Confirm refund" (`POST /api/shop/refund`), message and reload | Views/SkinsAdmin/Purchases.cshtml:455-466, 610-637, Controllers/Api/SkinShopController.cs:98-109 | done |  |
| Refund everything modal: intro with totals, warning, checkbox that enables the button, running label | Views/SkinsAdmin/Purchases.cshtml:471-498, 643-660 | done |  |
| Refund everything: `POST /api/shop/refund-all`, summary, failed items message, reload | Views/SkinsAdmin/Purchases.cshtml:662-686, Controllers/Api/SkinShopController.cs:111-119 | adapted | The hub refunds everything in one database transaction, so `failed` is always 0 and the "items could not be refunded" message, kept in the page, never shows. |
| Gift modal on the page | Views/SkinsAdmin/Purchases.cshtml:500 | done |  |
| Mobile: rows wrap and the player goes to a second line under 700 px; hero stacks under 480 px | Views/Shared/_AdminStyles.cshtml, Views/SkinsAdmin/Purchases.cshtml:254-263 | done |  |

## APIs

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| `GET /api/shop/wallet` for the signed-in player: `balance`, `stars`, `spent` (used by the stars in the navbar) | Controllers/Api/SkinShopController.cs:18-29 | done | New in the hub. It never answers 503, because the stars are local. |
| `POST /api/shop/skins/{skinId}/buy`: `status`, `succeeded`, `price`, `balance` | Controllers/Api/SkinShopController.cs:31-41, 121-129 | done |  |
| `POST /api/shop/skins/{skinId}/equip` | Controllers/Api/SkinShopController.cs:43-53 | done |  |
| `POST /api/shop/weapons/{weapon}/unequip` | Controllers/Api/SkinShopController.cs:55-65 | done |  |
| `POST /api/shop/skins/{skinId}/price` (administrator): 400 out of range, 404 unknown skin, `price`, `defaultPrice`, `custom` | Controllers/Api/SkinShopController.cs:67-83 | done |  |
| `POST /api/shop/gift` (administrator): Steam ID in any format | Controllers/Api/SkinShopController.cs:85-96, 130-141 | done |  |
| `POST /api/shop/refund` (administrator) | Controllers/Api/SkinShopController.cs:98-109 | done |  |
| `POST /api/shop/refund-all` (administrator): `purchases`, `gifts`, `stars`, `players`, `failed` | Controllers/Api/SkinShopController.cs:111-119, Core/Skins/Results/RefundAllResult.cs | adapted | `failed` is always 0, see the purchases page. |
| Antiforgery token required in the header `RequestVerificationToken` on every POST | Controllers/Api/SkinShopController.cs:32, 44, 56, 69, 87, 100, 113 | done |  |
| `GET /api/skins`: the skins someone owns (`id`, `weapon`), for the game server | Controllers/Api/SkinsController.cs:10-22 | done | Already in the hub, unchanged. |
| `GET /api/players/skins`: skins and equipment of every player, for the game server | Controllers/Api/PlayersController.cs:33-46 | done | Already in the hub, unchanged. |
| `GET /api/players/{communityId}/wallet`: `mvpSiDamage`, `mvpCommon`, `stars`, `spent`, `balance` | Controllers/Api/PlayersController.cs:48-56 | done | `mvpSiDamage` and `mvpCommon` added to the answer. |
| `POST /api/players/{communityId}/skins/equip` from the game server: equip or back to default | Controllers/Api/PlayersController.cs:58-69 | adapted | Same path and body. The game server proves who it is with its hub token (`Authorization` header) instead of the old shared secret. |

## Shop rules and background work

| Item | Where in the old site | Status | Notes |
|---|---|---|---|
| Stars: one for each SI damage MVP and each common MVP of the whole history | Core/Skins/Services/SkinShopService.cs:29-38, 373-382 | done |  |
| Buying: checks the skin, ownership and the balance; saves the price, the balance before and the player name; equips at once | Core/Skins/Services/SkinShopService.cs:69-138 | done | One database transaction under the write lock replaces the table storage transaction with retries. |
| Equipping needs ownership; one skin per weapon | Core/Skins/Services/SkinShopService.cs:140-157 | done |  |
| Back to default removes the equipment of the weapon | Core/Skins/Services/SkinShopService.cs:159-184 | done |  |
| Refund: returns the stars, removes the skin and its equipment | Core/Skins/Services/SkinShopService.cs:230-311 | done |  |
| Season refund: every ten minutes, purchases made before the current season are refunded; gifts are kept | BackgroundServices/SkinSeasonRefundBackgroundService.cs:8-35, Core/Skins/Services/SkinShopService.cs:246-260, 313-316 | adapted | Follows the seasons of the hub, of any length, and is a shop setting that starts on. |
| Catalog: id, weapon, rarity, price, Workshop id, photos, author, titles and descriptions in three languages, original title and description | Core/Skins/Skin.cs, Core/Skins/SkinAuthor.cs, Core/Skins/Data/skins.json | done | The catalog of the old site comes with the hub (`Core/Modules/Skins/Data/skins.json`, unchanged) and is imported by itself the first time the shop is turned on. The administrator can import it again, or another `skins.json`, on the catalog page. Every field is kept. |
| Thumbnails at `/imgs/skins/{id}.jpg`, photos at `{SkinShotsUrl}/{id}/` | Core/Skins/Skin.cs:33, Views/Skins/Index.cshtml:871-874 | done | The same 196 thumbnails are in `wwwroot/imgs/skins`. Photos at `{assets}/skin-shots/{id}/`, where the assets address starts as the `SkinShotsUrl` of the old site without its last folder (`https://l4d2playstats.blob.core.windows.net/assets`). A skin without a bundled thumbnail reads it from `{assets}/skins/{id}.jpg`. |

## Hub-only pages

| Item | Where in the hub | Status | Notes |
|---|---|---|---|
| Skin catalog page (`/admin/skins/catalog`): totals, last change, import of the bundled catalog, import from a file, import from the configured address | Views/SkinsAdmin/Catalog.cshtml | done | Restyled with `_AdminStyles` (header, stats, panels). The last change is shown in the site time zone. New: the import of the bundled catalog. |
| Shop settings page (`/admin/skins/settings`): refund at the end of a season, address of the assets, address of the catalog | Views/SkinsAdmin/Settings.cshtml | done | Restyled with Bootstrap 3 panels. New: the two addresses of the notice for skin authors, and the default address of the assets, shown when the field is empty. |
| Tabs that link the four administrator pages | Views/Shared/_SkinsAdminNav.cshtml | adapted | The tabs are gone: the four pages are in the Skin shop group of the administration side menu (`Views/Shared/_AdminMenu.cshtml`), which every administrator page shows, so the way to the rest of the administration is not lost on these pages. The content of the pages is unchanged. |

## Wording

The rarity names in Portuguese and Spanish are back to the feminine forms of the old site ("Lendária", "Épica", "Rara"; "Legendaria", "Épica", "Rara"), through the fragment files `pt-BR.skins.json` and `es-ES.skins.json`.

## How it was checked

- The shop page, signed out and in Portuguese, was read in a browser on the hub and on https://l4d2.com.br/skins and the two documents were compared node by node (tags, classes, attributes and texts of the page body and of every modal, about 13,700 lines each). The only differences left are the antiforgery token, the footer links and the site name, which come from the settings.
- Screenshots at 1280 and 400 pixels wide, signed out, show the same page on both sites (only the menu items of other areas differ).
- Signed in as the administrator on a copy of the demo data: refund of a purchase, purchase with the confirmation, back to default, equip, gift with the player picker and the skin picker, price change and reset, filters, tabs kept in the address, `#skin-{id}` links, photos on hover, gallery and lightbox with the production photos, light theme, Portuguese and Spanish.
- The prices and purchases pages need an administrator, so they could not be opened on the live site; they were checked against the old views line by line.
