using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.GameModes.Services;
using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Matches.Services;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class MatchTests
{
    private static readonly DateTime Night = new(2026, 11, 14, 21, 0, 0);

    [TestMethod]
    public async Task ConsecutiveMapsOfACampaignFormOneMatch()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadCampaignAsync(hub, server, Night, "c2m1_highway", "c2m2_fairgrounds", "c2m3_coaster", "c2m4_barns", "c2m5_concert");

        var matches = await MatchesAsync(hub);

        Assert.HasCount(1, matches);
        Assert.AreEqual("Dark Carnival", matches[0].Campaign);
        Assert.AreEqual(5, matches[0].MapCount);
        Assert.IsTrue(matches[0].CountsForRanking);
        Assert.AreEqual(Night.AddMilliseconds(1), matches[0].FirstFileDate);
        CollectionAssert.AreEqual(new int?[] { 0, 1, 2, 3, 4 }, matches[0].Files.OrderBy(f => f.FileDate).Select(f => f.MatchOrder).ToList());
    }

    [TestMethod]
    public async Task TheFinalScoreComesFromTheLastMap()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 450, 900));

        var match = (await MatchesAsync(hub)).Single();

        Assert.AreEqual(450, match.TeamAScore);
        Assert.AreEqual(900, match.TeamBScore);
    }

    [TestMethod]
    public async Task ADifferentCampaignStartsANewMatch()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadCampaignAsync(hub, server, Night, "c2m1_highway", "c2m2_fairgrounds");
        await UploadCampaignAsync(hub, server, Night.AddHours(1), "c5m1_waterfront", "c5m2_park");

        var matches = await MatchesAsync(hub);

        Assert.HasCount(2, matches);
        CollectionAssert.AreEquivalent(new[] { "Dark Carnival", "The Parish" }, matches.Select(m => m.Campaign).ToList());
    }

    [TestMethod]
    public async Task ASingleMapDoesNotCountForTheRanking()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200));

        var match = (await MatchesAsync(hub)).Single();

        Assert.AreEqual(1, match.MapCount);
        Assert.IsFalse(match.CountsForRanking);
    }

    [TestMethod]
    public async Task ARepeatedMapIsIgnored()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night.AddMinutes(10), 2, 310, 220));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(25), 3, 600, 500));

        var match = (await MatchesAsync(hub)).Single();

        Assert.AreEqual(2, match.MapCount);
    }

    [TestMethod]
    public async Task AMapWithoutPointsIsIgnored()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 0, 0));

        Assert.IsEmpty(await MatchesAsync(hub));
    }

    [TestMethod]
    public async Task FilesArrivingOutOfOrderGiveTheSameMatch()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m3_coaster", Night.AddMinutes(30), 3, 900, 800));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 600, 500));

        var match = (await MatchesAsync(hub)).Single();

        Assert.AreEqual(3, match.MapCount);
        Assert.AreEqual(900, match.TeamAScore);
    }

    [TestMethod]
    public async Task AChangeOfConfigurationSplitsTheMatch()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200).WithConfiguration("ZoneMod v2.9.2b"));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 600, 500).WithConfiguration("NeoMod Tourney 0.4a"));

        var matches = await MatchesAsync(hub);

        Assert.HasCount(2, matches);
        Assert.IsTrue(matches.All(m => m.MapCount == 1));
    }

    [TestMethod]
    public async Task MatchesOfDifferentServersStayApart()
    {
        using var hub = new TestHub();

        var first = (await hub.CreateServerAsync("First")).Server;
        var second = (await hub.CreateServerAsync("Second")).Server;

        await hub.UploadAsync(first, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200));
        await hub.UploadAsync(second, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 600, 500));

        var matches = await MatchesAsync(hub);

        Assert.HasCount(2, matches);
        Assert.IsTrue(matches.All(m => m.MapCount == 1));
    }

    [TestMethod]
    public async Task MatchesDoNotCrossASeasonBoundary()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;
        var lastNight = new DateTime(2026, 10, 31, 23, 50, 0);

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", lastNight, 1, 300, 200));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", lastNight.AddMinutes(15), 2, 600, 500));

        var matches = await MatchesAsync(hub);

        Assert.HasCount(2, matches);
        Assert.AreNotEqual(matches[0].SeasonId, matches[1].SeasonId);
    }

    [TestMethod]
    public async Task MapsOfAnUnknownCampaignAreKeptByDefault()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("custom_map_1", Night, 1, 300, 200));
        await hub.UploadAsync(server, StatisticsBuilder.Map("custom_map_2", Night.AddMinutes(15), 2, 600, 500));

        var match = (await MatchesAsync(hub)).Single();

        Assert.IsNull(match.Campaign);
        Assert.AreEqual(2, match.MapCount);
        Assert.IsTrue(match.CountsForRanking);
    }

    [TestMethod]
    public async Task MapsOfAnUnknownCampaignCanBeLeftOut()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new RankingSettings { IncludeUnknownCampaigns = false }));
        await hub.UploadAsync(server, StatisticsBuilder.Map("custom_map_1", Night, 1, 300, 200));

        Assert.IsEmpty(await MatchesAsync(hub));
    }

    [TestMethod]
    public async Task ReprocessingAppliesNewRulesToOldMatches()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadCampaignAsync(hub, server, Night, "c2m1_highway", "c2m2_fairgrounds");

        await hub.RunAsync(async services =>
        {
            var gameModes = services.GetRequiredService<IGameModeService>();
            var rule = (await gameModes.ListRulesAsync()).Single();

            rule.MinimumMaps = 3;
            await gameModes.SaveRuleAsync(rule);
        });

        var untouched = (await MatchesAsync(hub)).Single();

        await hub.RunAsync(services => services.GetRequiredService<IMatchBuildService>().RebuildAllAsync());

        var reprocessed = (await MatchesAsync(hub)).Single();

        Assert.IsTrue(untouched.CountsForRanking);
        Assert.IsFalse(reprocessed.CountsForRanking);
    }

    [TestMethod]
    public async Task ReprocessingKeepsTheSameMatches()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadCampaignAsync(hub, server, Night, "c2m1_highway", "c2m2_fairgrounds", "c2m3_coaster");
        await UploadCampaignAsync(hub, server, Night.AddDays(1), "c5m1_waterfront", "c5m2_park");

        var before = await MatchesAsync(hub);
        var rebuilt = await hub.RunAsync(services => services.GetRequiredService<IMatchBuildService>().RebuildAllAsync());
        var after = await MatchesAsync(hub);

        Assert.AreEqual(2, rebuilt);
        CollectionAssert.AreEqual(before.Select(Describe).ToList(), after.Select(Describe).ToList());
    }

    [TestMethod]
    public async Task EveryPlayerSeenGetsAProfile()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Sample());

        var profile = await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().PlayerProfiles.SingleAsync(p => p.CommunityId == StatisticsBuilder.PlayerA1));

        Assert.IsFalse(string.IsNullOrEmpty(profile.Name));
    }

    private static string Describe(Match match)
    {
        var players = string.Join(",", match.Players.OrderBy(p => p.Order).Select(p => $"{p.CommunityId}:{p.Win}:{p.Loss}:{p.MvpSiDamage}:{p.MvpCommon}"));

        return $"{match.Campaign}|{match.MapCount}|{match.TeamAScore}|{match.TeamBScore}|{match.CountsForRanking}|{players}";
    }

    private static async Task UploadCampaignAsync(TestHub hub, GameServer server, DateTime start, params string[] maps)
    {
        for (var index = 0; index < maps.Length; index++)
            await hub.UploadAsync(server, StatisticsBuilder.Map(maps[index], start.AddMinutes(15 * index), index + 1, 300 * (index + 1), 250 * (index + 1)));
    }

    private static Task<List<Match>> MatchesAsync(TestHub hub)
    {
        return hub.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches
            .AsNoTracking()
            .Include(m => m.Players)
            .Include(m => m.Files)
            .OrderBy(m => m.LastFileDate)
            .ThenBy(m => m.Id)
            .ToListAsync());
    }
}
