using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Skins;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core.Skins;

[TestClass]
public class SkinSeasonRefundTests
{
    private const long Friend = StatisticsBuilder.PlayerA1;
    private const long Administrator = 76561198000000100;
    private const int StarsPerMatch = 4;

    private static readonly DateTime NextSeason = new(2027, 1, 2, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime SeasonAfterNext = new(2027, 3, 2, 12, 0, 0, DateTimeKind.Utc);

    [TestMethod]
    public void TheRefundAtTheEndOfTheSeasonIsOnByDefault()
    {
        Assert.IsTrue(new SkinShopSettings().RefundAtSeasonEnd);
    }

    [TestMethod]
    public async Task ANewSeasonRefundsThePurchasesOfTheOldOneAndKeepsTheGifts()
    {
        using var hub = new TestHub();

        var player = await StartAsync(hub);

        await RunAsync(hub);
        await hub.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.GoldMagnum));
        await hub.ShopAsync(shop => shop.GiftAsync(player, SkinSamples.GalaxyPump, Administrator, "Admin"));
        await hub.ShopAsync(shop => shop.GiftAsync(Friend, SkinSamples.DesertSmg, Administrator, "Admin"));

        hub.Clock.UtcNow = NextSeason;

        var summary = await RunAsync(hub);
        var wallet = await hub.ShopAsync(shop => shop.GetWalletAsync(player));
        var skins = await hub.ShopAsync(shop => shop.GetPlayerSkinsAsync(player));
        var friend = await hub.ShopAsync(shop => shop.GetPlayerSkinsAsync(Friend));
        var purchases = await hub.PurchasesAsync();

        Assert.AreEqual(1, summary.Purchases);
        Assert.AreEqual(0, summary.Gifts);
        Assert.AreEqual(3, summary.Stars);
        Assert.AreEqual(1, summary.Players);
        Assert.AreEqual(0, wallet.Spent);
        Assert.AreEqual(StarsPerMatch, wallet.Balance);
        Assert.IsFalse(skins.Owns(SkinSamples.GoldMagnum));
        Assert.IsFalse(skins.Equipped.ContainsKey(SkinSamples.Magnum));
        Assert.IsTrue(skins.Owns(SkinSamples.GalaxyPump));
        Assert.IsTrue(friend.Owns(SkinSamples.DesertSmg));
        Assert.AreEqual(SkinSamples.DesertSmg, friend.Equipped[SkinSamples.Smg]);
        Assert.HasCount(2, purchases);
        Assert.IsTrue(purchases.All(purchase => purchase.Gift));
    }

    [TestMethod]
    public async Task PurchasesOfTheCurrentSeasonAreKept()
    {
        using var hub = new TestHub();

        var player = await StartAsync(hub);

        await hub.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.GoldMagnum));

        var first = await RunAsync(hub);

        hub.Clock.Advance(TimeSpan.FromDays(10));

        var later = await RunAsync(hub);

        Assert.AreEqual(0, first.Purchases);
        Assert.AreEqual(0, later.Purchases);
        Assert.HasCount(1, await hub.PurchasesAsync());
    }

    [TestMethod]
    public async Task RunningTheRefundAgainChangesNothing()
    {
        using var hub = new TestHub();

        var player = await StartAsync(hub);

        await hub.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.GoldMagnum));

        hub.Clock.UtcNow = NextSeason;

        var first = await RunAsync(hub);

        await hub.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.DesertSmg));

        var second = await RunAsync(hub);

        hub.Clock.Advance(TimeSpan.FromDays(3));

        var third = await RunAsync(hub);
        var purchase = (await hub.PurchasesAsync()).Single();

        Assert.AreEqual(1, first.Purchases);
        Assert.AreEqual(0, second.Purchases);
        Assert.AreEqual(0, third.Purchases);
        Assert.AreEqual(SkinSamples.DesertSmg, purchase.SkinId);
    }

    [TestMethod]
    public async Task WithTheSettingOffANewSeasonRefundsNothing()
    {
        using var hub = new TestHub();

        var player = await StartAsync(hub);

        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new SkinShopSettings { RefundAtSeasonEnd = false }));
        await hub.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.GoldMagnum));

        hub.Clock.UtcNow = NextSeason;

        var summary = await RunAsync(hub);
        var wallet = await hub.ShopAsync(shop => shop.GetWalletAsync(player));

        Assert.AreEqual(0, summary.Purchases);
        Assert.AreEqual(3, wallet.Spent);
        Assert.HasCount(1, await hub.PurchasesAsync());
    }

    [TestMethod]
    public async Task TurningTheSettingOnInTheMiddleOfASeasonWaitsForTheNextOne()
    {
        using var hub = new TestHub();

        var player = await StartAsync(hub);

        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new SkinShopSettings { RefundAtSeasonEnd = false }));
        await hub.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.GoldMagnum));

        hub.Clock.UtcNow = NextSeason;

        await RunAsync(hub);
        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new SkinShopSettings { RefundAtSeasonEnd = true }));

        var sameSeason = await RunAsync(hub);
        var kept = await hub.PurchasesAsync();

        hub.Clock.UtcNow = SeasonAfterNext;

        var nextSeason = await RunAsync(hub);

        Assert.AreEqual(0, sameSeason.Purchases);
        Assert.HasCount(1, kept);
        Assert.AreEqual(1, nextSeason.Purchases);
        Assert.IsEmpty(await hub.PurchasesAsync());
    }

    [TestMethod]
    [DataRow(false, true)]
    [DataRow(true, false)]
    public async Task WhileTheShopIsOffNothingIsRefunded(bool skinShop, bool ranking)
    {
        using var hub = new TestHub();

        var player = await StartAsync(hub);

        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new ModuleSettings { SkinShop = skinShop, Ranking = ranking }));
        await hub.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.GoldMagnum));

        hub.Clock.UtcNow = NextSeason;

        var off = await RunAsync(hub);
        var kept = await hub.PurchasesAsync();

        await hub.EnableSkinShopAsync();

        var on = await RunAsync(hub);

        Assert.AreEqual(0, off.Purchases);
        Assert.HasCount(1, kept);
        Assert.AreEqual(1, on.Purchases);
        Assert.IsEmpty(await hub.PurchasesAsync());
    }

    [TestMethod]
    public async Task TheRefundFollowsTheSeasonsOfTheRankingAndNotAFixedCalendar()
    {
        using var hub = new TestHub();

        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new SeasonSettings { LengthInMonths = 4 }));

        var player = await StartAsync(hub);

        hub.Clock.UtcNow = new DateTime(2027, 1, 10, 12, 0, 0, DateTimeKind.Utc);

        await RunAsync(hub);
        await hub.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.GoldMagnum));

        hub.Clock.UtcNow = SeasonAfterNext;

        var sameSeason = await RunAsync(hub);
        var kept = await hub.PurchasesAsync();

        hub.Clock.UtcNow = new DateTime(2027, 5, 2, 12, 0, 0, DateTimeKind.Utc);

        var nextSeason = await RunAsync(hub);

        Assert.AreEqual(0, sameSeason.Purchases);
        Assert.HasCount(1, kept);
        Assert.AreEqual(1, nextSeason.Purchases);
        Assert.IsEmpty(await hub.PurchasesAsync());
    }

    [TestMethod]
    public async Task TheSeasonStartsAtMidnightInTheTimeZoneOfTheSite()
    {
        using var hub = new TestHub();

        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new SiteSettings { TimeZone = "America/Sao_Paulo" }));

        var player = await StartAsync(hub);

        hub.Clock.UtcNow = new DateTime(2027, 1, 1, 1, 0, 0, DateTimeKind.Utc);

        var beforeMidnight = await RunAsync(hub);

        await hub.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.GoldMagnum));

        hub.Clock.UtcNow = new DateTime(2027, 1, 1, 3, 30, 0, DateTimeKind.Utc);

        await hub.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.DesertSmg));

        var afterMidnight = await RunAsync(hub);
        var purchase = (await hub.PurchasesAsync()).Single();

        Assert.AreEqual(0, beforeMidnight.Purchases);
        Assert.AreEqual(1, afterMidnight.Purchases);
        Assert.AreEqual(3, afterMidnight.Stars);
        Assert.AreEqual(SkinSamples.DesertSmg, purchase.SkinId);
    }

    private static async Task<long> StartAsync(TestHub hub)
    {
        await hub.EnableSkinShopAsync();
        await hub.ImportSkinsAsync();

        return await hub.PlayerWithStarsAsync();
    }

    private static Task<SkinRefundSummary> RunAsync(TestHub hub)
    {
        return hub.RunAsync(services => services.GetRequiredService<ISkinSeasonRefundService>().RunAsync());
    }
}