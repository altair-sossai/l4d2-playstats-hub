using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Skins.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Skins;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core.Skins;

[TestClass]
public class SkinStarsTests
{
    private const long Stranger = 76561198999999999;

    [TestMethod]
    public async Task TheStarsAreSplitBetweenTheTwoKindsOfMvp()
    {
        using var hub = new TestHub();

        var player = await hub.PlayerWithStarsAsync(2);
        var stars = await StarsAsync(hub, player);
        var ranking = await hub.StarsAsync();
        var rows = await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().MatchPlayers.AsNoTracking().Where(row => row.CommunityId == player && row.Match!.CountsForRanking).ToListAsync());

        Assert.AreEqual(rows.Sum(row => row.MvpSiDamage * Teams(row.TeamA, row.TeamB)), stars.MvpSiDamage);
        Assert.AreEqual(rows.Sum(row => row.MvpCommon * Teams(row.TeamA, row.TeamB)), stars.MvpCommon);
        Assert.AreEqual(ranking[player], stars.Total);
        Assert.IsGreaterThan(0, stars.Total);
    }

    [TestMethod]
    public async Task EveryPlayerOfTheRankingHasTheSameStarsInTheShop()
    {
        using var hub = new TestHub();

        await hub.PlayerWithStarsAsync();

        var ranking = await hub.StarsAsync();

        foreach (var (player, total) in ranking)
            Assert.AreEqual(total, (await StarsAsync(hub, player)).Total, player.ToString());
    }

    [TestMethod]
    public async Task APlayerWhoNeverPlayedHasNoStars()
    {
        using var hub = new TestHub();

        await hub.PlayerWithStarsAsync();

        var stars = await StarsAsync(hub, Stranger);

        Assert.AreEqual(SkinStars.None, stars);
        Assert.AreEqual(0, stars.Total);
    }

    [TestMethod]
    public async Task TheStarsFollowNewMatches()
    {
        using var hub = new TestHub();

        var before = await StarsAsync(hub, StatisticsBuilder.PlayerA1);
        var player = await hub.PlayerWithStarsAsync();
        var after = await StarsAsync(hub, player);

        Assert.AreEqual(0, before.Total);
        Assert.IsGreaterThan(0, after.Total);
    }

    [TestMethod]
    public async Task TheWalletCarriesTheSplitAndKeepsItAfterAPurchase()
    {
        using var hub = new TestHub();

        await hub.ImportSkinsAsync();

        var player = await hub.PlayerWithStarsAsync();
        var stars = await StarsAsync(hub, player);
        var result = await hub.ShopAsync(shop => shop.BuyAsync(player, null, SkinSamples.GoldMagnum));
        var wallet = await hub.ShopAsync(shop => shop.GetWalletAsync(player));

        Assert.AreEqual(stars.MvpSiDamage, wallet.MvpSiDamage);
        Assert.AreEqual(stars.MvpCommon, wallet.MvpCommon);
        Assert.AreEqual(stars.Total, wallet.Stars);
        Assert.AreEqual(stars.MvpSiDamage, result.Wallet!.MvpSiDamage);
        Assert.AreEqual(stars.MvpCommon, result.Wallet.MvpCommon);
        Assert.AreEqual(3, result.Wallet.Spent);
        Assert.AreEqual(wallet.Balance, result.Wallet.Balance);
    }

    private static int Teams(bool teamA, bool teamB)
    {
        return (teamA ? 1 : 0) + (teamB ? 1 : 0);
    }

    private static Task<SkinStars> StarsAsync(TestHub hub, long player)
    {
        return hub.RunAsync(services => services.GetRequiredService<ISkinStarsService>().GetAsync(player));
    }
}
