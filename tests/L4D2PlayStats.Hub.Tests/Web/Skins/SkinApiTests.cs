using System.Net;
using System.Text.Json;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Skins;

namespace L4D2PlayStats.Hub.Tests.Web.Skins;

[TestClass]
public class SkinApiTests
{
    [TestMethod]
    public async Task ThePluginGetsOnlyTheSkinsSomeoneOwns()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();
        var before = await client.GetFromJsonAsync<JsonElement>("/api/skins");

        await factory.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.GoldMagnum));

        var response = await client.GetAsync("/api/skins");
        var after = await response.Content.ReadFromJsonAsync<JsonElement>();
        var skin = after[0];

        Assert.AreEqual(JsonValueKind.Array, before.ValueKind);
        Assert.AreEqual(0, before.GetArrayLength());
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(1, after.GetArrayLength());
        Assert.AreEqual(SkinSamples.GoldMagnum, skin.GetProperty("id").GetString());
        Assert.AreEqual(SkinSamples.Magnum, skin.GetProperty("weapon").GetString());
        CollectionAssert.AreEquivalent(new[] { "id", "weapon" }, skin.EnumerateObject().Select(property => property.Name).ToList());
    }

    [TestMethod]
    public async Task ThePluginGetsTheSkinsAndTheEquipmentOfEveryPlayer()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();

        await factory.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.DesertSmg));
        await factory.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.NeonSmg));
        await factory.ShopAsync(shop => shop.GiftAsync(HubWebFactory.Player, SkinSamples.GoldMagnum, HubWebFactory.Administrator, "Admin"));
        await factory.ShopAsync(shop => shop.UnequipAsync(HubWebFactory.Player, SkinSamples.Magnum));

        var players = await client.GetFromJsonAsync<JsonElement>("/api/players/skins");
        var all = players.EnumerateArray().ToList();
        var rich = all.Single(item => item.GetProperty("communityId").GetString() == player.ToString());
        var gifted = all.Single(item => item.GetProperty("communityId").GetString() == HubWebFactory.Player.ToString());
        var equipped = rich.GetProperty("equipped")[0];

        Assert.HasCount(2, all);
        Assert.AreEqual(JsonValueKind.String, rich.GetProperty("communityId").ValueKind);
        CollectionAssert.AreEquivalent(new[] { "communityId", "skins", "equipped" }, rich.EnumerateObject().Select(property => property.Name).ToList());
        CollectionAssert.AreEqual(new[] { SkinSamples.DesertSmg, SkinSamples.NeonSmg }, rich.GetProperty("skins").EnumerateArray().Select(skin => skin.GetString()).ToList());
        Assert.AreEqual(1, rich.GetProperty("equipped").GetArrayLength());
        Assert.AreEqual(SkinSamples.Smg, equipped.GetProperty("weapon").GetString());
        Assert.AreEqual(SkinSamples.NeonSmg, equipped.GetProperty("skin").GetString());
        CollectionAssert.AreEquivalent(new[] { "weapon", "skin" }, equipped.EnumerateObject().Select(property => property.Name).ToList());
        Assert.AreEqual(SkinSamples.GoldMagnum, gifted.GetProperty("skins")[0].GetString());
        Assert.AreEqual(0, gifted.GetProperty("equipped").GetArrayLength());
    }

    [TestMethod]
    public async Task ThePluginGetsTheWalletOfAPlayer()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();

        await factory.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.GoldMagnum));

        var response = await client.GetAsync($"/api/players/{player}/wallet");
        var wallet = await response.Content.ReadFromJsonAsync<JsonElement>();
        var stranger = await client.GetFromJsonAsync<JsonElement>("/api/players/76561198999999999/wallet");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(4, wallet.GetProperty("stars").GetInt32());
        Assert.AreEqual(3, wallet.GetProperty("spent").GetInt32());
        Assert.AreEqual(1, wallet.GetProperty("balance").GetInt32());
        Assert.AreEqual(0, stranger.GetProperty("stars").GetInt32());
        Assert.AreEqual(0, stranger.GetProperty("balance").GetInt32());
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("unknown:key")]
    [DataRow("test-server:wrong")]
    public async Task OnlyAGameServerCanChangeTheEquippedSkin(string? token)
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.StartSkinShopAsync();

        var player = await factory.PlayerWithStarsAsync();

        await factory.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.DesertSmg));
        await factory.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.NeonSmg));

        if (token != null)
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);

        var response = await client.PostAsJsonAsync($"/api/players/{player}/skins/equip", new { weapon = SkinSamples.Smg, skin = SkinSamples.DesertSmg });
        var skins = await factory.ShopAsync(shop => shop.GetPlayerSkinsAsync(player));

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.AreEqual(SkinSamples.NeonSmg, skins.Equipped[SkinSamples.Smg]);
    }

    [TestMethod]
    public async Task TheGameEquipsAndUnequipsTheSkinsOfAPlayer()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        var credentials = await factory.CreateServerAsync();
        var player = await factory.PlayerWithStarsAsync(credentials);
        using var server = factory.ServerClient(credentials);

        await factory.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.DesertSmg));
        await factory.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.NeonSmg));

        var equip = await server.PostAsJsonAsync($"/api/players/{player}/skins/equip", new { weapon = SkinSamples.Smg, skin = SkinSamples.DesertSmg });
        var equipBody = await equip.Content.ReadFromJsonAsync<JsonElement>();
        var equipped = await factory.ShopAsync(shop => shop.GetPlayerSkinsAsync(player));

        var unequip = await server.PostAsJsonAsync($"/api/players/{player}/skins/equip", new { weapon = SkinSamples.Smg, skin = "" });
        var unequipBody = await unequip.Content.ReadFromJsonAsync<JsonElement>();
        var unequipped = await factory.ShopAsync(shop => shop.GetPlayerSkinsAsync(player));

        var notOwned = await server.PostAsJsonAsync($"/api/players/{player}/skins/equip", new { weapon = SkinSamples.Magnum, skin = SkinSamples.GoldMagnum });
        var notOwnedBody = await notOwned.Content.ReadFromJsonAsync<JsonElement>();

        var withoutWeapon = await server.PostAsJsonAsync($"/api/players/{player}/skins/equip", new { skin = SkinSamples.DesertSmg });

        Assert.AreEqual(HttpStatusCode.OK, equip.StatusCode);
        Assert.AreEqual("Success", equipBody.GetProperty("status").GetString());
        Assert.IsTrue(equipBody.GetProperty("succeeded").GetBoolean());
        Assert.AreEqual(SkinSamples.DesertSmg, equipped.Equipped[SkinSamples.Smg]);
        Assert.AreEqual(HttpStatusCode.OK, unequip.StatusCode);
        Assert.IsTrue(unequipBody.GetProperty("succeeded").GetBoolean());
        Assert.IsEmpty(unequipped.Equipped);
        Assert.AreEqual(HttpStatusCode.OK, notOwned.StatusCode);
        Assert.AreEqual("NotOwned", notOwnedBody.GetProperty("status").GetString());
        Assert.IsFalse(notOwnedBody.GetProperty("succeeded").GetBoolean());
        Assert.AreEqual(HttpStatusCode.BadRequest, withoutWeapon.StatusCode);
    }
}
