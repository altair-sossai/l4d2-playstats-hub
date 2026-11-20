using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Matches.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using L4D2PlayStats.Hub.Core.Modules.Matches.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class PlayerMatchTests
{
    private const long Stranger = 76561198000000999;
    private const int RecentMatches = 50;

    private static readonly DateTime Night = new(2026, 11, 14, 21, 0, 0);

    [TestMethod]
    public async Task TheRecentMatchesOfAPlayerCarryTheScoreOfEachMapInTheOrderPlayed()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 450, 900));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m3_coaster", Night.AddMinutes(30), 3, 800, 1200));

        var ids = await RecentAsync(hub, [StatisticsBuilder.PlayerA1]);
        var page = await hub.RunAsync(services => services.GetRequiredService<IMatchResultService>().ListAsync(new MatchFilter { Ids = ids }));
        var match = page.Matches.Single();

        CollectionAssert.AreEqual(new[] { "c2m1_highway", "c2m2_fairgrounds", "c2m3_coaster" }, match.Maps.Select(map => map.MapName).ToList());
        CollectionAssert.AreEqual(new[] { 300, 450, 800 }, match.Maps.Select(map => map.TeamAScore).ToList());
        CollectionAssert.AreEqual(new[] { 200, 900, 1200 }, match.Maps.Select(map => map.TeamBScore).ToList());
        Assert.AreEqual(800, match.ScoreA);
        Assert.AreEqual(1200, match.ScoreB);
        Assert.IsNotEmpty(match.TeamA!.Players);
        Assert.IsNotNull(match.GameServerName);
    }

    [TestMethod]
    public async Task TheRecentMatchesComeNewestFirst()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadMatchAsync(hub, server, Night.AddDays(-1), "c2m1_highway", "c2m2_fairgrounds");
        await UploadMatchAsync(hub, server, Night, "c5m1_waterfront", "c5m2_park");

        var ids = await RecentAsync(hub, [StatisticsBuilder.PlayerA1]);

        CollectionAssert.AreEqual(new[] { "The Parish", "Dark Carnival" }, await CampaignsAsync(hub, ids));
    }

    [TestMethod]
    public async Task OnlyTheMostRecentMatchesOfThePeriodAreSearched()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadMatchAsync(hub, server, Night.AddDays(-1), "c2m1_highway", "c2m2_fairgrounds");
        await UploadMatchAsync(hub, server, Night, "c5m1_waterfront", "c5m2_park");

        var ids = await RecentAsync(hub, [StatisticsBuilder.PlayerA1], 1);

        CollectionAssert.AreEqual(new[] { "The Parish" }, await CampaignsAsync(hub, ids));
    }

    [TestMethod]
    public async Task APlayerWhoWasNotOnATeamHasNoRecentMatches()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadMatchAsync(hub, server, Night, "c2m1_highway", "c2m2_fairgrounds");

        Assert.IsEmpty(await RecentAsync(hub, [Stranger]));
        Assert.IsEmpty(await RecentAsync(hub, []));
    }

    [TestMethod]
    public async Task AMatchOfAnotherSeasonOrYearIsNotListed()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadMatchAsync(hub, server, Night, "c2m1_highway", "c2m2_fairgrounds");

        var seasonId = await SeasonIdAsync(hub);
        var service = (IServiceProvider services) => services.GetRequiredService<IPlayerMatchService>();

        Assert.IsEmpty(await hub.RunAsync(services => service(services).RecentMatchIdsAsync(seasonId + 1000, null, [StatisticsBuilder.PlayerA1], RecentMatches)));
        Assert.IsEmpty(await hub.RunAsync(services => service(services).RecentMatchIdsAsync(null, Night.Year - 1, [StatisticsBuilder.PlayerA1], RecentMatches)));
        Assert.HasCount(1, await hub.RunAsync(services => service(services).RecentMatchIdsAsync(null, Night.Year, [StatisticsBuilder.PlayerA1], RecentMatches)));
        Assert.HasCount(1, await hub.RunAsync(services => service(services).RecentMatchIdsAsync(null, null, [StatisticsBuilder.PlayerA1], RecentMatches)));
    }

    [TestMethod]
    public async Task EitherPlayerOfAComparisonBringsTheMatch()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;
        var opponent = StatisticsBuilder.Sample().Survivors('B')[0];

        await UploadMatchAsync(hub, server, Night, "c2m1_highway", "c2m2_fairgrounds");

        Assert.HasCount(1, await RecentAsync(hub, [Stranger, opponent]));
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

    private static async Task UploadMatchAsync(TestHub hub, GameServer server, DateTime start, params string[] maps)
    {
        for (var index = 0; index < maps.Length; index++)
            await hub.UploadAsync(server, StatisticsBuilder.Map(maps[index], start.AddMinutes(15 * index), index + 1, 300 * (index + 1), 250 * (index + 1)));
    }

    private static async Task<List<int>> RecentAsync(TestHub hub, long[] communityIds, int recentMatches = RecentMatches)
    {
        var seasonId = await SeasonIdAsync(hub);

        return await hub.RunAsync(services => services.GetRequiredService<IPlayerMatchService>().RecentMatchIdsAsync(seasonId, null, communityIds, recentMatches));
    }

    private static async Task<List<string?>> CampaignsAsync(TestHub hub, List<int> ids)
    {
        var page = await hub.RunAsync(services => services.GetRequiredService<IMatchResultService>().ListAsync(new MatchFilter { Ids = ids }));

        return [.. page.Matches.Select(match => match.Campaign)];
    }


    private static Task<int> SeasonIdAsync(TestHub hub)
    {
        return hub.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.AsNoTracking().Select(match => match.SeasonId).FirstAsync());
    }
}
