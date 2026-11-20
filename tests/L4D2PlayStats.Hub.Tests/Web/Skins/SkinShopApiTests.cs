using System.Net;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Skins;

namespace L4D2PlayStats.Hub.Tests.Web.Skins;

[TestClass]
public class SkinShopApiTests
{
    private const string Shop = "/skins";
    private const string Purchases = "/admin/skins";
    private const string Prices = "/admin/skins/prices";

    [TestMethod]
    public async Task TheSignedInPlayerGetsTheWalletShownInTheMenu()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();
        using var client = factory.Client(player);
        using var anonymous = factory.Client();

        await factory.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.GoldMagnum));

        var response = await client.GetAsync("/api/shop/wallet");
        var wallet = await response.Content.ReadFromJsonAsync<JsonElement>();
        var notSignedIn = await anonymous.GetAsync("/api/shop/wallet");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(1, wallet.GetProperty("balance").GetInt32());
        Assert.AreEqual(4, wallet.GetProperty("stars").GetInt32());
        Assert.AreEqual(3, wallet.GetProperty("spent").GetInt32());
        CollectionAssert.AreEquivalent(new[] { "balance", "stars", "spent" }, wallet.EnumerateObject().Select(property => property.Name).ToList());
        Assert.AreEqual(HttpStatusCode.Unauthorized, notSignedIn.StatusCode);
    }

    [TestMethod]
    public async Task APlayerBuysEquipsAndGoesBackToTheDefaultSkin()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();
        using var client = factory.Client(player);

        var buy = await SkinWebFactoryExtensions.PostJsonAsync(client, Shop, "/api/shop/skins/desert-smg/buy");
        var bought = await buy.Content.ReadFromJsonAsync<JsonElement>();
        var afterBuying = await factory.ShopAsync(shop => shop.GetPlayerSkinsAsync(player));

        await factory.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.NeonSmg));

        var equip = await SkinWebFactoryExtensions.PostJsonAsync(client, Shop, "/api/shop/skins/desert-smg/equip");
        var equipped = await equip.Content.ReadFromJsonAsync<JsonElement>();
        var afterEquipping = await factory.ShopAsync(shop => shop.GetPlayerSkinsAsync(player));
        var unequip = await SkinWebFactoryExtensions.PostJsonAsync(client, Shop, "/api/shop/weapons/weapon_smg/unequip");
        var unequipped = await unequip.Content.ReadFromJsonAsync<JsonElement>();
        var afterUnequipping = await factory.ShopAsync(shop => shop.GetPlayerSkinsAsync(player));
        var purchase = (await factory.PurchasesAsync()).Single(p => p.SkinId == SkinSamples.DesertSmg);

        Assert.AreEqual(HttpStatusCode.OK, buy.StatusCode);
        Assert.AreEqual("Success", bought.GetProperty("status").GetString());
        Assert.IsTrue(bought.GetProperty("succeeded").GetBoolean());
        Assert.AreEqual(1, bought.GetProperty("price").GetInt32());
        Assert.AreEqual(3, bought.GetProperty("balance").GetInt32());
        Assert.AreEqual(SkinSamples.DesertSmg, afterBuying.Equipped[SkinSamples.Smg]);
        Assert.AreEqual($"Player {player}", purchase.PlayerName);
        Assert.AreEqual(4, purchase.StarsBefore);
        Assert.IsTrue(equipped.GetProperty("succeeded").GetBoolean());
        Assert.AreEqual(SkinSamples.DesertSmg, afterEquipping.Equipped[SkinSamples.Smg]);
        Assert.IsTrue(unequipped.GetProperty("succeeded").GetBoolean());
        Assert.IsEmpty(afterUnequipping.Equipped);
        Assert.HasCount(2, afterUnequipping.Owned);
    }

    [TestMethod]
    public async Task AFailedOperationSaysWhyAndChargesNothing()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();
        using var client = factory.Client(player);

        await factory.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.DesertSmg));

        var expensive = await Status(await SkinWebFactoryExtensions.PostJsonAsync(client, Shop, "/api/shop/skins/galaxy-pump/buy"));
        var twice = await Status(await SkinWebFactoryExtensions.PostJsonAsync(client, Shop, "/api/shop/skins/desert-smg/buy"));
        var unknown = await Status(await SkinWebFactoryExtensions.PostJsonAsync(client, Shop, "/api/shop/skins/no-such-skin/buy"));
        var notOwned = await Status(await SkinWebFactoryExtensions.PostJsonAsync(client, Shop, "/api/shop/skins/gold-magnum/equip"));
        var unknownWeapon = await Status(await SkinWebFactoryExtensions.PostJsonAsync(client, Shop, "/api/shop/weapons/weapon_rifle/unequip"));
        var wallet = await factory.ShopAsync(shop => shop.GetWalletAsync(player));

        Assert.AreEqual("InsufficientStars", expensive);
        Assert.AreEqual("AlreadyOwned", twice);
        Assert.AreEqual("SkinNotFound", unknown);
        Assert.AreEqual("NotOwned", notOwned);
        Assert.AreEqual("SkinNotFound", unknownWeapon);
        Assert.AreEqual(1, wallet.Spent);
        Assert.HasCount(1, await factory.PurchasesAsync());
    }

    [TestMethod]
    public async Task TheShopNeedsASignedInPlayerAndTheAntiforgeryToken()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();
        using var anonymous = factory.Client();
        using var client = factory.Client(player);

        var notSignedIn = await anonymous.PostAsJsonAsync("/api/shop/skins/desert-smg/buy", new { });
        var withoutToken = await client.PostAsJsonAsync("/api/shop/skins/desert-smg/buy", new { });
        var equipWithoutToken = await client.PostAsJsonAsync("/api/shop/skins/desert-smg/equip", new { });
        var unequipWithoutToken = await client.PostAsJsonAsync("/api/shop/weapons/weapon_smg/unequip", new { });

        Assert.AreEqual(HttpStatusCode.Unauthorized, notSignedIn.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, withoutToken.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, equipWithoutToken.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, unequipWithoutToken.StatusCode);
        Assert.IsEmpty(await factory.PurchasesAsync());
    }

    [TestMethod]
    public async Task OnlyAnAdministratorChangesPricesGivesAndRefunds()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();
        using var client = factory.Client(player);
        using var administrator = factory.Client(HubWebFactory.Administrator);

        await factory.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.DesertSmg));

        var price = await SkinWebFactoryExtensions.PostJsonAsync(client, Shop, "/api/shop/skins/desert-smg/price", new { price = 5 });
        var gift = await SkinWebFactoryExtensions.PostJsonAsync(client, Shop, "/api/shop/gift", new { communityId = player.ToString(), skinId = SkinSamples.GalaxyPump });
        var refund = await SkinWebFactoryExtensions.PostJsonAsync(client, Shop, "/api/shop/refund", new { communityId = player.ToString(), skinId = SkinSamples.DesertSmg });
        var refundAll = await SkinWebFactoryExtensions.PostJsonAsync(client, Shop, "/api/shop/refund-all");
        var withoutToken = await administrator.PostAsJsonAsync("/api/shop/refund-all", new { });
        var skin = (await factory.SkinsAsync()).Single(s => s.Id == SkinSamples.DesertSmg);

        Assert.AreEqual(HttpStatusCode.Forbidden, price.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, gift.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, refund.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, refundAll.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, withoutToken.StatusCode);
        Assert.IsFalse(skin.HasCustomPrice);
        Assert.HasCount(1, await factory.PurchasesAsync());
    }

    [TestMethod]
    public async Task AnAdministratorChangesAndResetsAPrice()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var change = await SkinWebFactoryExtensions.PostJsonAsync(client, Prices, "/api/shop/skins/gold-magnum/price", new { price = 25 });
        var changedBody = await change.Content.ReadFromJsonAsync<JsonElement>();
        var changed = (await factory.SkinsAsync()).Single(skin => skin.Id == SkinSamples.GoldMagnum);
        var tooLow = await SkinWebFactoryExtensions.PostJsonAsync(client, Prices, "/api/shop/skins/gold-magnum/price", new { price = 0 });
        var tooHigh = await SkinWebFactoryExtensions.PostJsonAsync(client, Prices, "/api/shop/skins/gold-magnum/price", new { price = 100001 });
        var unknown = await SkinWebFactoryExtensions.PostJsonAsync(client, Prices, "/api/shop/skins/no-such-skin/price", new { price = 5 });
        var afterInvalid = (await factory.SkinsAsync()).Single(skin => skin.Id == SkinSamples.GoldMagnum);
        var reset = await SkinWebFactoryExtensions.PostJsonAsync(client, Prices, "/api/shop/skins/gold-magnum/price", new { price = (int?)null });
        var resetBody = await reset.Content.ReadFromJsonAsync<JsonElement>();
        var afterReset = (await factory.SkinsAsync()).Single(skin => skin.Id == SkinSamples.GoldMagnum);

        Assert.AreEqual(HttpStatusCode.OK, change.StatusCode);
        Assert.AreEqual(25, changedBody.GetProperty("price").GetInt32());
        Assert.AreEqual(3, changedBody.GetProperty("defaultPrice").GetInt32());
        Assert.IsTrue(changedBody.GetProperty("custom").GetBoolean());
        Assert.AreEqual(25, changed.Price);
        Assert.AreEqual(HubWebFactory.Administrator, changed.PriceOverride!.UpdatedByCommunityId);
        Assert.AreEqual(HttpStatusCode.BadRequest, tooLow.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, tooHigh.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.AreEqual(25, afterInvalid.Price);
        Assert.AreEqual(HttpStatusCode.OK, reset.StatusCode);
        Assert.AreEqual(3, resetBody.GetProperty("price").GetInt32());
        Assert.IsFalse(resetBody.GetProperty("custom").GetBoolean());
        Assert.AreEqual(3, afterReset.Price);
        Assert.IsFalse(afterReset.HasCustomPrice);
    }

    [TestMethod]
    [DataRow("76561198000000200")]
    [DataRow("STEAM_0:0:19867236")]
    [DataRow("STEAM_1:0:19867236")]
    [DataRow("[U:1:39734472]")]
    [DataRow("U:1:39734472")]
    [DataRow(" https://steamcommunity.com/profiles/76561198000000200/ ")]
    public async Task AnAdministratorGivesASkinToASteamIdInAnyFormat(string steamId)
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var gift = await SkinWebFactoryExtensions.PostJsonAsync(client, Purchases, "/api/shop/gift", new { communityId = steamId, skinId = SkinSamples.GalaxyPump });
        var body = await gift.Content.ReadFromJsonAsync<JsonElement>();
        var purchase = (await factory.PurchasesAsync()).Single();
        var skins = await factory.ShopAsync(shop => shop.GetPlayerSkinsAsync(HubWebFactory.Player));

        Assert.AreEqual(HttpStatusCode.OK, gift.StatusCode);
        Assert.IsTrue(body.GetProperty("succeeded").GetBoolean());
        Assert.IsTrue(purchase.Gift);
        Assert.AreEqual(0, purchase.Price);
        Assert.AreEqual(HubWebFactory.Player, purchase.CommunityId);
        Assert.AreEqual(HubWebFactory.Administrator, purchase.GiftedByCommunityId);
        Assert.AreEqual($"Player {HubWebFactory.Administrator}", purchase.GiftedByName);
        Assert.AreEqual(SkinSamples.GalaxyPump, skins.Equipped["weapon_pumpshotgun"]);
    }

    [TestMethod]
    [DataRow("123", "galaxy-pump")]
    [DataRow("not a steam id", "galaxy-pump")]
    [DataRow("", "galaxy-pump")]
    [DataRow("76561198000000200", "")]
    public async Task AGiftWithoutAValidPlayerOrSkinIsRefused(string steamId, string skinId)
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var gift = await SkinWebFactoryExtensions.PostJsonAsync(client, Purchases, "/api/shop/gift", new { communityId = steamId, skinId });

        Assert.AreEqual(HttpStatusCode.BadRequest, gift.StatusCode);
        Assert.IsEmpty(await factory.PurchasesAsync());
    }

    [TestMethod]
    public async Task AGiftOfASkinThePlayerAlreadyHasSaysSo()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var first = await Status(await SkinWebFactoryExtensions.PostJsonAsync(client, Purchases, "/api/shop/gift", new { communityId = HubWebFactory.Player.ToString(), skinId = SkinSamples.GalaxyPump }));
        var second = await Status(await SkinWebFactoryExtensions.PostJsonAsync(client, Purchases, "/api/shop/gift", new { communityId = HubWebFactory.Player.ToString(), skinId = SkinSamples.GalaxyPump }));
        var unknown = await Status(await SkinWebFactoryExtensions.PostJsonAsync(client, Purchases, "/api/shop/gift", new { communityId = HubWebFactory.Player.ToString(), skinId = "no-such-skin" }));

        Assert.AreEqual("Success", first);
        Assert.AreEqual("AlreadyOwned", second);
        Assert.AreEqual("SkinNotFound", unknown);
        Assert.HasCount(1, await factory.PurchasesAsync());
    }

    [TestMethod]
    public async Task AnAdministratorRefundsOnePurchase()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.GoldMagnum));

        var refund = await Status(await SkinWebFactoryExtensions.PostJsonAsync(client, Purchases, "/api/shop/refund", new { communityId = player.ToString(), skinId = SkinSamples.GoldMagnum }));
        var again = await Status(await SkinWebFactoryExtensions.PostJsonAsync(client, Purchases, "/api/shop/refund", new { communityId = player.ToString(), skinId = SkinSamples.GoldMagnum }));
        var invalid = await SkinWebFactoryExtensions.PostJsonAsync(client, Purchases, "/api/shop/refund", new { communityId = "123", skinId = SkinSamples.GoldMagnum });
        var wallet = await factory.ShopAsync(shop => shop.GetWalletAsync(player));
        var skins = await factory.ShopAsync(shop => shop.GetPlayerSkinsAsync(player));

        Assert.AreEqual("Success", refund);
        Assert.AreEqual("NotOwned", again);
        Assert.AreEqual(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.AreEqual(0, wallet.Spent);
        Assert.AreEqual(4, wallet.Balance);
        Assert.IsEmpty(skins.Owned);
        Assert.IsEmpty(skins.Equipped);
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

        var response = await SkinWebFactoryExtensions.PostJsonAsync(client, Purchases, "/api/shop/refund-all");
        var summary = await response.Content.ReadFromJsonAsync<JsonElement>();
        var wallet = await factory.ShopAsync(shop => shop.GetWalletAsync(player));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(1, summary.GetProperty("purchases").GetInt32());
        Assert.AreEqual(1, summary.GetProperty("gifts").GetInt32());
        Assert.AreEqual(3, summary.GetProperty("stars").GetInt32());
        Assert.AreEqual(2, summary.GetProperty("players").GetInt32());
        Assert.AreEqual(0, summary.GetProperty("failed").GetInt32());
        Assert.AreEqual(0, wallet.Spent);
        Assert.IsEmpty(await factory.PurchasesAsync());
    }

    [TestMethod]
    public async Task WithTheShopOffItsApiStopsAnswering()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.SaveAsync(new ModuleSettings { SkinShop = false });

        var wallet = await client.GetAsync("/api/shop/wallet");
        var oldPurchases = await client.GetAsync("/admin/skins/purchases");

        Assert.AreEqual(HttpStatusCode.NotFound, wallet.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, oldPurchases.StatusCode);
    }

    private static async Task<string?> Status(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        return body.GetProperty("status").GetString();
    }
}