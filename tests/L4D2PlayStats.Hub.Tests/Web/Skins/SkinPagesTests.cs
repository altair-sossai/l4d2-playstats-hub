using System.Net;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Skins;
using Microsoft.Extensions.DependencyInjection;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Web.Skins;

[TestClass]
public class SkinPagesTests
{
    private const string AssetsUrl = "https://assets.example.test";
    private const string ContactUrl = "https://steamcommunity.com/profiles/76561198000000100";
    private const string RequestUrl = "https://example.test/issues/new";

    [TestMethod]
    public async Task TheShopOpensForVisitorsAndShowsTheCatalog()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.StartSkinShopAsync();

        var response = await client.GetAsync("/skins");
        var body = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("class=\"sk-hero\"", body);
        Assert.Contains("id=\"skin-gold-magnum\"", body);
        Assert.Contains("Golden Magnum", body);
        Assert.Contains("Galaxy Pump Shotgun", body);
        Assert.Contains("href=\"/login?returnUrl=%2Fskins\"", body);
        Assert.Contains("href=\"#shop\"", body);
        Assert.Contains("href=\"#authors\"", body);
        Assert.Contains("https://steamcommunity.com/profiles/76561198000000001", body);
        Assert.Contains("https://steamcommunity.com/sharedfiles/filedetails/?id=1000000003", body);
        Assert.Contains("Desert Uzi", body);
        Assert.Contains("sk-rarity-legendary", body);
        Assert.Contains("id=\"sk-use-modal\"", body);
        Assert.DoesNotContain("href=\"#mine\"", body);
        Assert.DoesNotContain("id=\"sk-only-affordable\"", body);
        Assert.DoesNotContain("class=\"sk-wallet-balance\"", body);
        Assert.DoesNotContain("id=\"gm-modal\"", body);
        Assert.DoesNotContain("sk-admin-gift\" data-skin", body);
        Assert.DoesNotContain("!skin gold-magnum", body);
    }

    [TestMethod]
    public async Task TheShopSaysWhenThereIsNoCatalogYet()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Player);

        await factory.StartSkinShopAsync(false);

        var response = await client.GetAsync("/skins");
        var body = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("There are no skins yet.", body);
        Assert.DoesNotContain("class=\"sk-card", body);
    }

    [TestMethod]
    public async Task ASignedInPlayerSeesTheWalletAndTheOwnedSkins()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();
        using var client = factory.Client(player);

        await factory.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.GoldMagnum));

        var wallet = await factory.ShopAsync(shop => shop.GetWalletAsync(player));
        var page = await client.GetStringAsync("/skins");

        Assert.Contains("<div class=\"sk-wallet-balance\">1</div>", page);
        Assert.Contains("star available", page);
        Assert.Contains("You earned <b>4 ⭐</b> as MVP and have spent <b>3 ⭐</b> on skins.", page);
        Assert.Contains($"{wallet.MvpSiDamage} SI damage MVPs &#x2B; {wallet.MvpCommon} common MVPs", page);
        Assert.Contains("href=\"#mine\"", page);
        Assert.Contains("id=\"sk-only-affordable\"", page);
        Assert.Contains("sk-unequip\" data-weapon=\"weapon_pistol_magnum\"", page);
        Assert.Contains("sk-buy\" data-skin=\"desert-smg\"", page);
        Assert.Contains("!skin gold-magnum", page);
        Assert.DoesNotContain("sk-buy\" data-skin=\"galaxy-pump\"", page);
        Assert.DoesNotContain("sk-buy\" data-skin=\"gold-magnum\"", page);
        Assert.DoesNotContain("!skin desert-smg", page);
        Assert.DoesNotContain("id=\"gm-modal\"", page);
    }

    [TestMethod]
    public async Task AnAdministratorGetsTheGiftModalOnTheShop()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.StartSkinShopAsync();

        var page = await client.GetStringAsync("/skins");

        Assert.Contains("id=\"gm-modal\"", page);
        Assert.Contains("id=\"sp-modal\"", page);
        Assert.Contains("id=\"pp-modal\"", page);
        Assert.Contains("sk-admin-gift\" data-skin=\"gold-magnum\"", page);
        Assert.Contains("class=\"sp-tile sp-legendary\"", page);
    }

    [TestMethod]
    public async Task TheShopUsesTheDefaultAssetsAddressUntilAnotherIsSet()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.StartSkinShopAsync();

        var without = await client.GetStringAsync("/skins");

        await factory.SaveAsync(new SkinShopSettings { AssetsUrl = AssetsUrl });

        var with = await client.GetStringAsync("/skins");

        Assert.Contains($"class=\"sk-gallery\" data-shots=\"{CoreConsts.SkinDefaultAssetsUrl}/skin-shots/gold-magnum/\" data-version=\"2\"", without);
        Assert.Contains($"<meta property=\"og:image\" content=\"{CoreConsts.SkinDefaultAssetsUrl}/skins/galaxy-pump.jpg\">", without);
        Assert.DoesNotContain(AssetsUrl, without);
        Assert.DoesNotContain(CoreConsts.SkinDefaultAssetsUrl, with);
        Assert.Contains($"src=\"{AssetsUrl}/skins/gold-magnum.jpg\"", with);
        Assert.Contains($"<meta property=\"og:image\" content=\"{AssetsUrl}/skins/galaxy-pump.jpg\">", with);
        Assert.Contains($"class=\"sk-gallery\" data-shots=\"{AssetsUrl}/skin-shots/gold-magnum/\" data-version=\"2\"", with);
        Assert.Contains($"href=\"{AssetsUrl}/skin-shots/gold-magnum/1.webp?v=2\"", with);
        Assert.DoesNotContain($"{AssetsUrl}/skin-shots/desert-smg/", with);
    }

    [TestMethod]
    public async Task TheSeasonTextsFollowTheSeasonLengthAndTheRefundSetting()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        await factory.StartSkinShopAsync();
        await factory.SaveAsync(new SeasonSettings { LengthInMonths = 3 });

        var refunding = await client.GetStringAsync("/skins");
        var purchases = await administrator.GetStringAsync("/admin/skins");

        await factory.SaveAsync(new SkinShopSettings { RefundAtSeasonEnd = false });

        var keeping = await client.GetStringAsync("/skins");
        var purchasesKeeping = await administrator.GetStringAsync("/admin/skins");

        Assert.Contains("class=\"sk-season\"", refunding);
        Assert.Contains("which happens every 3 months", refunding);
        Assert.Contains("At the end of the ranking season", purchases);
        Assert.DoesNotContain("class=\"sk-season\"", keeping);
        Assert.DoesNotContain("which happens every", keeping);
        Assert.Contains("is yours forever", keeping);
        Assert.DoesNotContain("At the end of the ranking season", purchasesKeeping);
    }

    [TestMethod]
    public async Task TheNoticeForAuthorsUsesTheAddressesOfTheSettings()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.StartSkinShopAsync();

        var standard = await client.GetStringAsync("/skins");

        await factory.SaveAsync(new SkinShopSettings { AuthorContactUrl = ContactUrl, AuthorRequestUrl = RequestUrl });

        var both = await client.GetStringAsync("/skins");

        await factory.SaveAsync(new SkinShopSettings { AuthorContactUrl = null, AuthorRequestUrl = null });

        var none = await client.GetStringAsync("/skins");

        Assert.Contains("sk-author-notice-open", standard);
        Assert.Contains("class=\"btn btn-default sk-author-notice-github\" href=\"https://github.com/altair-sossai/l4d2-zone-server-assets/issues/new\"", standard);
        Assert.DoesNotContain("class=\"btn btn-primary\" href=\"https://steamcommunity.com", standard);
        Assert.Contains($"class=\"btn btn-primary\" href=\"{ContactUrl}\"", both);
        Assert.Contains($"class=\"btn btn-default sk-author-notice-github\" href=\"{RequestUrl}\"", both);
        Assert.DoesNotContain("<button type=\"button\" class=\"btn btn-link btn-sm pull-left sk-author-notice-open\"", none);
    }

    [TestMethod]
    public async Task ASkinNoLongerForSaleStaysOnlyWithItsOwner()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();
        using var owner = factory.Client(player);
        using var visitor = factory.Client();

        await factory.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.DesertSmg));
        await factory.RunAsync(services => services.GetRequiredService<ISkinCatalogService>().ImportAsync(SkinSamples.Catalog(skins => skins.Remove(SkinSamples.Find(skins, SkinSamples.DesertSmg)))));

        var mine = await owner.GetStringAsync("/skins");
        var shop = await visitor.GetStringAsync("/skins");

        Assert.Contains("data-skin=\"desert-smg\"", mine);
        Assert.Contains("No longer for sale", mine);
        Assert.DoesNotContain("id=\"skin-desert-smg\"", mine);
        Assert.DoesNotContain("data-skin=\"desert-smg\"", shop);
    }

    [TestMethod]
    public async Task ThePurchasesPageListsFiltersAndPaginates()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();
        await factory.SaveAsync(new SkinShopSettings { AssetsUrl = AssetsUrl });

        var player = await factory.PlayerWithStarsAsync();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.ShopAsync(shop => shop.BuyAsync(player, "Rich Player", SkinSamples.GoldMagnum));
        await factory.ShopAsync(shop => shop.GiftAsync(HubWebFactory.Player, SkinSamples.GalaxyPump, HubWebFactory.Administrator, "The Admin"));

        var all = await client.GetStringAsync("/admin/skins");
        var bySearch = await client.GetStringAsync("/admin/skins?q=rich");
        var byPlayer = await client.GetStringAsync($"/admin/skins?player={HubWebFactory.Player}");
        var bySteamId = await client.GetStringAsync("/admin/skins?player=STEAM_1:0:19867236");
        var bySteam3Search = await client.GetStringAsync("/admin/skins?q=%5BU%3A1%3A39734472%5D");
        var nothing = await client.GetStringAsync("/admin/skins?q=nobody");
        var lastPage = await client.GetAsync("/admin/skins?page=9");

        Assert.Contains("<strong>1</strong>", all);
        Assert.Contains("<strong>3 ⭐</strong>", all);
        Assert.Contains("ska-refund-all-open", all);
        Assert.Contains($"data-player=\"{player}\" data-player-name=\"Rich Player\" data-avatar=\"/avatar/{player}\"", all);
        Assert.Contains($"data-image=\"{AssetsUrl}/skins/gold-magnum.jpg\"", all);
        Assert.Contains("data-shop-url=\"/skins#skin-gold-magnum\"", all);
        Assert.Contains("data-rarity=\"Legendary\" data-rarity-css=\"ska-rarity-legendary\"", all);
        Assert.Contains("data-gift=\"0\"", all);
        Assert.Contains("data-stars-before=\"4\"", all);
        Assert.Contains("data-stars-after=\"1\"", all);
        Assert.Contains("data-gift=\"1\" data-gifted-by=\"The Admin\"", all);
        Assert.Contains("by The Admin", all);
        Assert.Contains("id=\"gm-modal\"", all);
        Assert.Contains("Rich Player", bySearch);
        Assert.DoesNotContain("data-skin=\"galaxy-pump\"", bySearch);
        Assert.Contains("class=\"ska-filter-player\"", byPlayer);
        Assert.Contains("data-skin=\"galaxy-pump\"", byPlayer);
        Assert.DoesNotContain("data-skin=\"gold-magnum\"", byPlayer);
        Assert.Contains("class=\"ska-filter-player\"", bySteamId);
        Assert.Contains("data-skin=\"galaxy-pump\"", bySteamId);
        Assert.DoesNotContain("data-skin=\"gold-magnum\"", bySteamId);
        Assert.Contains("data-skin=\"galaxy-pump\"", bySteam3Search);
        Assert.DoesNotContain("data-skin=\"gold-magnum\"", bySteam3Search);
        Assert.Contains("class=\"admin-empty\"", nothing);
        Assert.AreEqual(HttpStatusCode.OK, lastPage.StatusCode);
    }

    [TestMethod]
    public async Task TheOldPurchasesAddressRedirectsToTheNewOne()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.StartSkinShopAsync();

        var response = await client.GetAsync("/admin/skins/purchases?q=rich&page=2");

        Assert.AreEqual(HttpStatusCode.MovedPermanently, response.StatusCode);
        Assert.AreEqual("/admin/skins?q=rich&page=2", response.Headers.Location!.OriginalString);
    }

    [TestMethod]
    public async Task ThePricesPageListsFiltersAndShowsChangedPrices()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.RunAsync(services => services.GetRequiredService<ISkinService>().SetPriceAsync(SkinSamples.DesertSmg, 9, HubWebFactory.Administrator));

        var all = await client.GetStringAsync("/admin/skins/prices");
        var byName = await client.GetStringAsync("/admin/skins/prices?q=magnum");
        var byFilters = await client.GetStringAsync("/admin/skins/prices?weapon=weapon_smg&rarity=Rare");
        var nothing = await client.GetStringAsync("/admin/skins/prices?q=nothing-like-this");

        Assert.Contains("data-skin=\"desert-smg\"", all);
        Assert.Contains("data-price=\"9\" data-default-price=\"1\" data-custom=\"1\"", all);
        Assert.Contains("data-price=\"3\" data-default-price=\"3\" data-custom=\"0\"", all);
        Assert.Contains("label label-warning", all);
        Assert.Contains("id=\"ska-price-modal\"", all);
        Assert.Contains("id=\"gm-modal\"", all);
        Assert.Contains("data-skin=\"gold-magnum\"", byName);
        Assert.DoesNotContain("data-skin=\"desert-smg\"", byName);
        Assert.Contains("data-skin=\"neon-smg\"", byFilters);
        Assert.DoesNotContain("data-skin=\"desert-smg\"", byFilters);
        Assert.Contains("class=\"admin-empty\"", nothing);
    }

    [TestMethod]
    public async Task AnAdministratorImportsTheCatalogFromAFile()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync(false);

        using var client = factory.Client(HubWebFactory.Administrator);

        var response = await SkinWebFactoryExtensions.PostFileAsync(client, "/admin/skins/catalog", "/admin/skins/catalog/upload", SkinSamples.CatalogFileName, SkinSamples.Catalog());
        var page = await client.GetStringAsync("/admin/skins/catalog");
        var skins = await factory.SkinsAsync();

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual("/admin/skins/catalog", response.Headers.Location!.OriginalString);
        Assert.Contains("alert-success", page);
        Assert.HasCount(SkinSamples.Count, skins);
    }

    [TestMethod]
    public async Task AFileThatIsNotACatalogIsRefusedOnThePage()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var response = await SkinWebFactoryExtensions.PostFileAsync(client, "/admin/skins/catalog", "/admin/skins/catalog/upload", "notes.txt", "this is not a catalog");
        var page = await client.GetStringAsync("/admin/skins/catalog");
        var skins = await factory.SkinsAsync();

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("alert-danger", page);
        Assert.HasCount(SkinSamples.Count, skins);
        Assert.IsTrue(skins.All(skin => skin.Available));
    }

    [TestMethod]
    public async Task AnAdministratorSavesTheShopSettings()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var saved = await HubWebFactory.PostFormAsync(client, "/admin/skins/settings", "/admin/skins/settings", new Dictionary<string, string>
        {
            ["RefundAtSeasonEnd"] = "false",
            ["AssetsUrl"] = $" {AssetsUrl}/ ",
            ["CatalogUrl"] = $"{AssetsUrl}/skins/skins.json",
            ["AuthorContactUrl"] = $" {ContactUrl} ",
            ["AuthorRequestUrl"] = ""
        });

        var settings = await factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<SkinShopSettings>());

        var invalid = await HubWebFactory.PostFormAsync(client, "/admin/skins/settings", "/admin/skins/settings", new Dictionary<string, string>
        {
            ["RefundAtSeasonEnd"] = "true",
            ["AssetsUrl"] = "ftp://assets.example.test",
            ["CatalogUrl"] = "skins.json"
        });

        var invalidContact = await HubWebFactory.PostFormAsync(client, "/admin/skins/settings", "/admin/skins/settings", new Dictionary<string, string>
        {
            ["RefundAtSeasonEnd"] = "true",
            ["AuthorContactUrl"] = "javascript:alert(1)"
        });

        var afterInvalid = await factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<SkinShopSettings>());
        var page = await client.GetStringAsync("/admin/skins/settings");

        Assert.AreEqual(HttpStatusCode.Redirect, saved.StatusCode);
        Assert.IsFalse(settings.RefundAtSeasonEnd);
        Assert.AreEqual(AssetsUrl, settings.AssetsUrl);
        Assert.AreEqual($"{AssetsUrl}/skins/skins.json", settings.CatalogUrl);
        Assert.AreEqual(ContactUrl, settings.AuthorContactUrl);
        Assert.IsNull(settings.AuthorRequestUrl);
        Assert.AreEqual(HttpStatusCode.Redirect, invalid.StatusCode);
        Assert.AreEqual(HttpStatusCode.Redirect, invalidContact.StatusCode);
        Assert.IsFalse(afterInvalid.RefundAtSeasonEnd);
        Assert.AreEqual(AssetsUrl, afterInvalid.AssetsUrl);
        Assert.AreEqual(ContactUrl, afterInvalid.AuthorContactUrl);
        Assert.Contains($"name=\"AuthorContactUrl\" value=\"{ContactUrl}\"", page);
    }

    [TestMethod]
    public async Task EverySkinPageOfTheAdministrationOpensWithDataAndOnTheSiteLayout()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.ShopAsync(shop => shop.BuyAsync(player, "Rich Player", SkinSamples.GoldMagnum));
        await factory.SaveAsync(new SkinShopSettings { AssetsUrl = AssetsUrl, CatalogUrl = $"{AssetsUrl}/skins/skins.json" });

        string[] pages =
        [
            "/skins", "/admin/skins", "/admin/skins?q=rich", "/admin/skins?page=2", "/admin/skins/prices", "/admin/skins/prices?q=magnum&weapon=weapon_pistol_magnum&rarity=Legendary",
            "/admin/skins/catalog", "/admin/skins/settings"
        ];

        foreach (var page in pages)
        {
            var response = await client.GetAsync(page);
            var body = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, page);
            Assert.Contains("/css/bootstrap.min.css", body, page);
            Assert.DoesNotContain("/lib/bootstrap/", body, page);
        }

        var catalog = await client.GetStringAsync("/admin/skins/catalog");

        Assert.Contains("href=\"/admin/skins/settings\"", catalog);
        Assert.Contains($"<code>{AssetsUrl}/skins/skins.json</code>", catalog);
    }
}
