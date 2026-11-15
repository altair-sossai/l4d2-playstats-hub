using System.Net;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Skins;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Skins;

[TestClass]
public class SkinPagesTests
{
    private const string AssetsUrl = "https://assets.example.test";

    [TestMethod]
    public async Task TheShopOpensForVisitorsAndShowsTheCatalog()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.StartSkinShopAsync();

        var response = await client.GetAsync("/skins");
        var body = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Golden Magnum", body);
        Assert.Contains("Galaxy Pump Shotgun", body);
        Assert.Contains("!skin gold-magnum", body);
        Assert.DoesNotContain("__RequestVerificationToken", body);
        Assert.DoesNotContain("id=\"skin-balance\"", body);
    }

    [TestMethod]
    public async Task TheShopShowsAnEmptyCatalogBeforeTheFirstImport()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Player);

        await factory.StartSkinShopAsync(false);

        var response = await client.GetAsync("/skins");
        var body = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("class=\"card h-100 skin-card", body);
    }

    [TestMethod]
    public async Task ASignedInPlayerSeesTheWalletAndTheOwnedSkins()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();
        using var client = factory.Client(player);

        await factory.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.GoldMagnum));

        var all = await client.GetStringAsync("/skins");
        var mine = await client.GetStringAsync("/skins?owned=true");

        Assert.Contains("id=\"skin-balance\">★ 1<", all);
        Assert.Contains("/skins/unequip", all);
        Assert.Contains("/skins/desert-smg/buy", all);
        Assert.DoesNotContain("/skins/galaxy-pump/buy", all);
        Assert.Contains("id=\"skin-gold-magnum\"", mine);
        Assert.DoesNotContain("id=\"skin-desert-smg\"", mine);
    }

    [TestMethod]
    public async Task TheShopFiltersByWeaponAndRarity()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.StartSkinShopAsync();

        var byWeapon = await client.GetStringAsync("/skins?weapon=weapon_smg");
        var byRarity = await client.GetStringAsync("/skins?rarity=Legendary");
        var both = await client.GetStringAsync("/skins?weapon=weapon_smg&rarity=Rare");

        Assert.Contains("id=\"skin-desert-smg\"", byWeapon);
        Assert.Contains("id=\"skin-neon-smg\"", byWeapon);
        Assert.DoesNotContain("id=\"skin-gold-magnum\"", byWeapon);
        Assert.Contains("id=\"skin-gold-magnum\"", byRarity);
        Assert.Contains("id=\"skin-galaxy-pump\"", byRarity);
        Assert.DoesNotContain("id=\"skin-desert-smg\"", byRarity);
        Assert.Contains("id=\"skin-neon-smg\"", both);
        Assert.DoesNotContain("id=\"skin-desert-smg\"", both);
    }

    [TestMethod]
    public async Task AFilterChangedOnThePageGetsOnlyTheListOfSkins()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.StartSkinShopAsync();

        client.DefaultRequestHeaders.Add("HX-Request", "true");

        var body = await client.GetStringAsync("/skins?weapon=weapon_smg");

        client.DefaultRequestHeaders.Add("HX-History-Restore-Request", "true");

        var restored = await client.GetStringAsync("/skins?weapon=weapon_smg");

        Assert.Contains("id=\"skin-desert-smg\"", body);
        Assert.DoesNotContain("<html", body);
        Assert.DoesNotContain("<nav", body);
        Assert.Contains("id=\"skin-desert-smg\"", restored);
        Assert.Contains("<html", restored);
    }

    [TestMethod]
    public async Task TheShopUsesThePicturesOfTheAssetsAddress()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.StartSkinShopAsync();

        var without = await client.GetStringAsync("/skins");

        await factory.SaveAsync(new SkinShopSettings { AssetsUrl = AssetsUrl });

        var with = await client.GetStringAsync("/skins");

        Assert.DoesNotContain("skin-thumb", without);
        Assert.DoesNotContain("data-skin-shots", without);
        Assert.Contains($"src=\"{AssetsUrl}/skins/gold-magnum.jpg\"", with);
        Assert.Contains($"data-skin-shots=\"{AssetsUrl}/skin-shots/gold-magnum/\" data-skin-version=\"2\"", with);
        Assert.DoesNotContain($"data-skin-shots=\"{AssetsUrl}/skin-shots/desert-smg/\"", with);
    }

    [TestMethod]
    public async Task APlayerBuysEquipsAndUnequipsOnTheShopPage()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();
        using var client = factory.Client(player);

        var buy = await HubWebFactory.PostFormAsync(client, "/skins", "/skins/desert-smg/buy", new Dictionary<string, string> { ["returnUrl"] = "/skins?weapon=weapon_smg" });
        var afterBuying = await factory.ShopAsync(shop => shop.GetPlayerSkinsAsync(player));

        await factory.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.NeonSmg));

        var equip = await HubWebFactory.PostFormAsync(client, "/skins", "/skins/desert-smg/equip", []);
        var afterEquipping = await factory.ShopAsync(shop => shop.GetPlayerSkinsAsync(player));
        var unequip = await HubWebFactory.PostFormAsync(client, "/skins", "/skins/unequip", new Dictionary<string, string> { ["weapon"] = SkinSamples.Smg });
        var afterUnequipping = await factory.ShopAsync(shop => shop.GetPlayerSkinsAsync(player));
        var purchase = (await factory.PurchasesAsync()).Single(p => p.SkinId == SkinSamples.DesertSmg);

        Assert.AreEqual(HttpStatusCode.Redirect, buy.StatusCode);
        Assert.AreEqual("/skins?weapon=weapon_smg", buy.Headers.Location!.OriginalString);
        Assert.AreEqual(SkinSamples.DesertSmg, afterBuying.Equipped[SkinSamples.Smg]);
        Assert.AreEqual($"Player {player}", purchase.PlayerName);
        Assert.AreEqual(HttpStatusCode.Redirect, equip.StatusCode);
        Assert.AreEqual("/skins", equip.Headers.Location!.OriginalString);
        Assert.AreEqual(SkinSamples.DesertSmg, afterEquipping.Equipped[SkinSamples.Smg]);
        Assert.AreEqual(HttpStatusCode.Redirect, unequip.StatusCode);
        Assert.IsEmpty(afterUnequipping.Equipped);
        Assert.HasCount(2, afterUnequipping.Owned);
    }

    [TestMethod]
    public async Task APurchaseThatFailsTellsThePlayerWhy()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();
        using var client = factory.Client(player);

        var buy = await HubWebFactory.PostFormAsync(client, "/skins", "/skins/galaxy-pump/buy", new Dictionary<string, string> { ["returnUrl"] = "https://example.test/" });
        var page = await client.GetStringAsync("/skins");

        Assert.AreEqual(HttpStatusCode.Redirect, buy.StatusCode);
        Assert.AreEqual("/skins", buy.Headers.Location!.OriginalString);
        Assert.Contains("alert-danger", page);
        Assert.IsEmpty(await factory.PurchasesAsync());
    }

    [TestMethod]
    public async Task BuyingNeedsASignedInPlayerAndTheAntiforgeryToken()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();
        using var anonymous = factory.Client();
        using var client = factory.Client(player);

        var notSignedIn = await anonymous.PostAsync("/skins/desert-smg/buy", new FormUrlEncodedContent([]));
        var withoutToken = await client.PostAsync("/skins/desert-smg/buy", new FormUrlEncodedContent([]));

        Assert.AreEqual(HttpStatusCode.Unauthorized, notSignedIn.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, withoutToken.StatusCode);
        Assert.IsEmpty(await factory.PurchasesAsync());
    }

    [TestMethod]
    public async Task AnAdministratorGivesAndRefundsASkin()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var gift = await HubWebFactory.PostFormAsync(client, "/admin/skins", "/admin/skins/gift", new Dictionary<string, string>
        {
            ["communityId"] = HubWebFactory.Player.ToString(),
            ["skinId"] = SkinSamples.GalaxyPump
        });

        var purchase = (await factory.PurchasesAsync()).Single();
        var page = await client.GetStringAsync("/admin/skins");
        var search = await client.GetStringAsync("/admin/skins?q=nobody");

        var refund = await HubWebFactory.PostFormAsync(client, "/admin/skins", "/admin/skins/refund", new Dictionary<string, string>
        {
            ["communityId"] = HubWebFactory.Player.ToString(),
            ["skinId"] = SkinSamples.GalaxyPump
        });

        Assert.AreEqual(HttpStatusCode.Redirect, gift.StatusCode);
        Assert.IsTrue(purchase.Gift);
        Assert.AreEqual(HubWebFactory.Player, purchase.CommunityId);
        Assert.AreEqual(HubWebFactory.Administrator, purchase.GiftedByCommunityId);
        Assert.Contains("Galaxy Pump Shotgun", page);
        Assert.Contains($"name=\"communityId\" value=\"{HubWebFactory.Player}\"", page);
        Assert.DoesNotContain($"name=\"communityId\" value=\"{HubWebFactory.Player}\"", search);
        Assert.AreEqual(HttpStatusCode.Redirect, refund.StatusCode);
        Assert.IsEmpty(await factory.PurchasesAsync());
    }

    [TestMethod]
    public async Task AGiftToAnInvalidSteamIdIsRefused()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var gift = await HubWebFactory.PostFormAsync(client, "/admin/skins", "/admin/skins/gift", new Dictionary<string, string>
        {
            ["communityId"] = "123",
            ["skinId"] = SkinSamples.GalaxyPump
        });

        var page = await client.GetStringAsync("/admin/skins");

        Assert.AreEqual(HttpStatusCode.Redirect, gift.StatusCode);
        Assert.Contains("alert-danger", page);
        Assert.IsEmpty(await factory.PurchasesAsync());
    }

    [TestMethod]
    public async Task AnAdministratorRefundsEverything()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.GoldMagnum));
        await factory.ShopAsync(shop => shop.GiftAsync(HubWebFactory.Player, SkinSamples.DesertSmg, HubWebFactory.Administrator, "Admin"));

        var response = await HubWebFactory.PostFormAsync(client, "/admin/skins", "/admin/skins/refund-all", []);
        var page = await client.GetStringAsync("/admin/skins");
        var wallet = await factory.ShopAsync(shop => shop.GetWalletAsync(player));

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("alert-success", page);
        Assert.AreEqual(0, wallet.Spent);
        Assert.IsEmpty(await factory.PurchasesAsync());
    }

    [TestMethod]
    public async Task AnAdministratorChangesAndResetsAPrice()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var change = await HubWebFactory.PostFormAsync(client, "/admin/skins/prices", "/admin/skins/prices", new Dictionary<string, string>
        {
            ["skinId"] = SkinSamples.GoldMagnum,
            ["price"] = "25",
            ["returnUrl"] = "/admin/skins/prices?rarity=Legendary"
        });

        var changed = (await factory.SkinsAsync()).Single(skin => skin.Id == SkinSamples.GoldMagnum);

        var invalid = await HubWebFactory.PostFormAsync(client, "/admin/skins/prices", "/admin/skins/prices", new Dictionary<string, string>
        {
            ["skinId"] = SkinSamples.GoldMagnum,
            ["price"] = "0"
        });

        var afterInvalid = (await factory.SkinsAsync()).Single(skin => skin.Id == SkinSamples.GoldMagnum);
        var reset = await HubWebFactory.PostFormAsync(client, "/admin/skins/prices", "/admin/skins/prices", new Dictionary<string, string> { ["skinId"] = SkinSamples.GoldMagnum });
        var afterReset = (await factory.SkinsAsync()).Single(skin => skin.Id == SkinSamples.GoldMagnum);

        Assert.AreEqual(HttpStatusCode.Redirect, change.StatusCode);
        Assert.AreEqual("/admin/skins/prices?rarity=Legendary", change.Headers.Location!.OriginalString);
        Assert.AreEqual(25, changed.Price);
        Assert.AreEqual(HubWebFactory.Administrator, changed.PriceOverride!.UpdatedByCommunityId);
        Assert.AreEqual(HttpStatusCode.Redirect, invalid.StatusCode);
        Assert.AreEqual(25, afterInvalid.Price);
        Assert.AreEqual(HttpStatusCode.Redirect, reset.StatusCode);
        Assert.AreEqual(3, afterReset.Price);
        Assert.IsFalse(afterReset.HasCustomPrice);
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
            ["CatalogUrl"] = $"{AssetsUrl}/skins/skins.json"
        });

        var settings = await factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<SkinShopSettings>());

        var invalid = await HubWebFactory.PostFormAsync(client, "/admin/skins/settings", "/admin/skins/settings", new Dictionary<string, string>
        {
            ["RefundAtSeasonEnd"] = "true",
            ["AssetsUrl"] = "ftp://assets.example.test",
            ["CatalogUrl"] = "skins.json"
        });

        var afterInvalid = await factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<SkinShopSettings>());
        var page = await client.GetStringAsync("/admin/skins/settings");

        Assert.AreEqual(HttpStatusCode.Redirect, saved.StatusCode);
        Assert.IsFalse(settings.RefundAtSeasonEnd);
        Assert.AreEqual(AssetsUrl, settings.AssetsUrl);
        Assert.AreEqual($"{AssetsUrl}/skins/skins.json", settings.CatalogUrl);
        Assert.AreEqual(HttpStatusCode.Redirect, invalid.StatusCode);
        Assert.IsFalse(afterInvalid.RefundAtSeasonEnd);
        Assert.AreEqual(AssetsUrl, afterInvalid.AssetsUrl);
        Assert.Contains("alert-danger", page);
    }

    [TestMethod]
    public async Task EverySkinPageOfTheAdministrationOpensWithData()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.ShopAsync(shop => shop.BuyAsync(player, "Rich Player", SkinSamples.GoldMagnum));
        await factory.RunAsync(services => services.GetRequiredService<ISkinService>().SetPriceAsync(SkinSamples.DesertSmg, 9, HubWebFactory.Administrator));
        await factory.SaveAsync(new SkinShopSettings { AssetsUrl = AssetsUrl, CatalogUrl = $"{AssetsUrl}/skins/skins.json" });

        string[] pages =
        [
            "/admin/skins", "/admin/skins?q=rich", "/admin/skins?page=2", "/admin/skins/prices", "/admin/skins/prices?q=magnum&weapon=weapon_pistol_magnum&rarity=Legendary",
            "/admin/skins/catalog", "/admin/skins/settings"
        ];

        foreach (var page in pages)
        {
            var response = await client.GetAsync(page);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, page);
        }

        var purchases = await client.GetStringAsync("/admin/skins?q=rich");
        var prices = await client.GetStringAsync("/admin/skins/prices?q=magnum");

        Assert.Contains("Rich Player", purchases);
        Assert.Contains("Golden Magnum", prices);
        Assert.DoesNotContain("Desert SMG", prices);
    }
}
