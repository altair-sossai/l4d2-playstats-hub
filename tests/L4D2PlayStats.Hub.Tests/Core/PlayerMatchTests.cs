using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Services;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class PlayerMatchTests
{
    private const long Stranger = 76561198000000999;
    private const int RecentMatches = 50;

    private static readonly DateTime Night = new(2026, 11, 14, 21, 0, 0);

    [TestMethod]
    public async Task TheLastMatchesOfAPlayerCarryTheScoreOfEachMapInTheOrderPlayed()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 450, 900));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m3_coaster", Night.AddMinutes(30), 3, 800, 1200));

        var matches = await LastAsync(hub, [StatisticsBuilder.PlayerA1]);
        var match = matches.Single();

        CollectionAssert.AreEqual(new[] { "c2m1_highway", "c2m2_fairgrounds", "c2m3_coaster" }, match.Maps.Select(map => map.MapName).ToList());
        CollectionAssert.AreEqual(new[] { 300, 450, 800 }, match.Maps.Select(map => map.TeamAScore).ToList());
        CollectionAssert.AreEqual(new[] { 200, 900, 1200 }, match.Maps.Select(map => map.TeamBScore).ToList());
        Assert.AreEqual(800, match.Match.TeamAScore);
        Assert.AreEqual(1200, match.Match.TeamBScore);
        Assert.IsNotEmpty(match.Match.Players);
        Assert.IsNotNull(match.Match.GameServer);
    }

    [TestMethod]
    public async Task TheLastMatchesComeNewestFirst()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadMatchAsync(hub, server, Night.AddDays(-1), "c2m1_highway", "c2m2_fairgrounds");
        await UploadMatchAsync(hub, server, Night, "c5m1_waterfront", "c5m2_park");

        var matches = await LastAsync(hub, [StatisticsBuilder.PlayerA1]);

        CollectionAssert.AreEqual(new[] { "The Parish", "Dark Carnival" }, matches.Select(match => match.Match.Campaign).ToList());
    }

    [TestMethod]
    public async Task OnlyTheMostRecentMatchesOfTheSeasonAreSearched()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadMatchAsync(hub, server, Night.AddDays(-1), "c2m1_highway", "c2m2_fairgrounds");
        await UploadMatchAsync(hub, server, Night, "c5m1_waterfront", "c5m2_park");

        var matches = await LastAsync(hub, [StatisticsBuilder.PlayerA1], 1);

        Assert.AreEqual("The Parish", matches.Single().Match.Campaign);
    }

    [TestMethod]
    public async Task APlayerWhoWasNotOnATeamHasNoLastMatches()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadMatchAsync(hub, server, Night, "c2m1_highway", "c2m2_fairgrounds");

        Assert.IsEmpty(await LastAsync(hub, [Stranger]));
        Assert.IsEmpty(await LastAsync(hub, []));
    }

    [TestMethod]
    public async Task AMatchOfAnotherSeasonIsNotListed()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadMatchAsync(hub, server, Night, "c2m1_highway", "c2m2_fairgrounds");

        var seasonId = await SeasonIdAsync(hub);
        var matches = await hub.RunAsync(services => services.GetRequiredService<IPlayerMatchService>().LastAsync(seasonId + 1000, [StatisticsBuilder.PlayerA1], RecentMatches));

        Assert.IsEmpty(matches);
    }

    [TestMethod]
    public async Task EitherPlayerOfAComparisonBringsTheMatch()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;
        var opponent = StatisticsBuilder.Sample().Survivors('B')[0];

        await UploadMatchAsync(hub, server, Night, "c2m1_highway", "c2m2_fairgrounds");

        Assert.HasCount(1, await LastAsync(hub, [Stranger, opponent]));
    }

    [TestMethod]
    public void ACampaignCountsDrawsAsGamesAndGivesTheRateOfEachResult()
    {
        var campaign = new PlayerCampaign { Wins = 2, Loss = 1, Draws = 1, RageQuit = 1 };
        var empty = new PlayerCampaign();

        Assert.AreEqual(4, campaign.Games);
        Assert.AreEqual(0.5m, campaign.WinRate);
        Assert.AreEqual(0.25m, campaign.LossRate);
        Assert.AreEqual(0.25m, campaign.DrawRate);
        Assert.AreEqual(0, empty.Games);
        Assert.AreEqual(0m, empty.WinRate);
        Assert.AreEqual(0m, empty.LossRate);
        Assert.AreEqual(0m, empty.DrawRate);
    }

    [TestMethod]
    public async Task TheRankingGivesTheCampaignsOfAPlayer()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadMatchAsync(hub, server, Night.AddDays(-1), "c2m1_highway", "c2m2_fairgrounds");
        await UploadMatchAsync(hub, server, Night, "c5m1_waterfront", "c5m2_park");

        var ranking = await hub.RunAsync(services => services.GetRequiredService<IRankingService>().CurrentAsync());
        var player = ranking.Single(p => p.CommunityId == StatisticsBuilder.PlayerA1);

        CollectionAssert.AreEquivalent(new[] { "Dark Carnival", "The Parish" }, player.Campaigns.Keys.ToList());
        Assert.AreEqual(1, player.Campaigns["Dark Carnival"].Games);
        Assert.AreEqual(1m, player.Campaigns["Dark Carnival"].WinRate + player.Campaigns["Dark Carnival"].LossRate);
    }

    private static async Task UploadMatchAsync(TestHub hub, L4D2PlayStats.Hub.Core.Modules.Servers.GameServer server, DateTime start, params string[] maps)
    {
        for (var index = 0; index < maps.Length; index++)
            await hub.UploadAsync(server, StatisticsBuilder.Map(maps[index], start.AddMinutes(15 * index), index + 1, 300 * (index + 1), 250 * (index + 1)));
    }

    private static async Task<List<PlayerMatch>> LastAsync(TestHub hub, long[] communityIds, int recentMatches = RecentMatches)
    {
        var seasonId = await SeasonIdAsync(hub);

        return await hub.RunAsync(services => services.GetRequiredService<IPlayerMatchService>().LastAsync(seasonId, communityIds, recentMatches));
    }

    private static Task<int> SeasonIdAsync(TestHub hub)
    {
        return hub.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.AsNoTracking().Select(match => match.SeasonId).FirstAsync());
    }
}
