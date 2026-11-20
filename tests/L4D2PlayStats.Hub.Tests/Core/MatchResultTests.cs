using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Matches.Models;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Helpers;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using L4D2PlayStats.Hub.Core.Modules.Matches.Services.Contracts;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class MatchResultTests
{
    private static readonly DateTime Night = new(2026, 11, 14, 21, 0, 0);

    [TestMethod]
    public async Task TheTeamsAreThePlayersOfTheLastMapInTheOrderOfItsPlayerList()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;
        var last = await UploadTwoMapsAsync(hub, server);
        var match = await SingleAsync(hub);

        foreach (var team in match.Teams)
        {
            var survivors = last.Survivors(team.Code).ToHashSet();
            var expected = last.Statistics.PlayerNames
                .Where(playerName => survivors.Contains(long.Parse(playerName.CommunityId!)))
                .Select(playerName => long.Parse(playerName.CommunityId!))
                .ToList();

            CollectionAssert.AreEqual(expected, team.Players.Select(player => player.CommunityId).ToList());
            Assert.IsTrue(team.Players.All(player => !string.IsNullOrEmpty(player.Name)));
        }

        Assert.AreEqual('A', match.Teams[0].Code);
        Assert.AreEqual('B', match.Teams[1].Code);
    }

    [TestMethod]
    public async Task TheScoreTheWinnerAndTheDifferenceComeFromTheLastMap()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadTwoMapsAsync(hub, server);

        var match = await SingleAsync(hub);

        Assert.AreEqual("Dark Carnival", match.Campaign);
        Assert.AreEqual(600, match.ScoreA);
        Assert.AreEqual(500, match.ScoreB);
        Assert.AreEqual(100, match.Diff);
        Assert.AreEqual('A', match.Winner!.Code);
        Assert.IsTrue(match.TeamAWon);
        Assert.IsFalse(match.Draw);
        Assert.IsTrue(match.Competitive);
        Assert.AreEqual(match.MatchEnd - match.MatchStart, match.MatchElapsed);
    }

    [TestMethod]
    public async Task TheMapsComeOldestFirstWithTheScoreAfterEachOne()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadTwoMapsAsync(hub, server);

        var match = await SingleAsync(hub);

        CollectionAssert.AreEqual(new[] { "c2m1_highway", "c2m2_fairgrounds" }, match.Maps.Select(map => map.MapName).ToList());
        CollectionAssert.AreEqual(new[] { 300, 600 }, match.Maps.Select(map => map.TeamAScore).ToList());
        CollectionAssert.AreEqual(new[] { 200, 500 }, match.Maps.Select(map => map.TeamBScore).ToList());
    }

    [TestMethod]
    public async Task ThePlayerNumbersAddUpOverEveryMapAndHalf()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;
        var last = await UploadTwoMapsAsync(hub, server);
        var match = await SingleAsync(hub);
        var player = match.Teams[0].Players[0];
        var id = player.CommunityId.ToString();

        var survivor = last.Statistics.Halves.SelectMany(half => half.Players).Where(p => p.CommunityId == id).ToList();
        var infected = last.Statistics.Halves.SelectMany(half => half.InfectedPlayers).Where(p => p.CommunityId == id).ToList();
        var mvps = last.Statistics.Halves.Count(half => half.MvpSiDamage?.CommunityId == id);

        Assert.AreEqual(2 * survivor.Sum(p => p.SiDamage), player.SiDamage);
        Assert.AreEqual(2 * survivor.Sum(p => p.Common), player.Common);
        Assert.AreEqual(2 * survivor.Sum(p => p.Skeets + p.SkeetsHurt + p.SkeetsMelee), player.Skeets);
        Assert.AreEqual(2 * survivor.Sum(p => p.Levels + p.LevelsHurt), player.Levels);
        Assert.AreEqual(2 * survivor.Sum(p => p.Crowns + p.CrownsHurt), player.Crowns);
        Assert.AreEqual(2 * infected.Sum(p => p.DmgTotal), player.DmgTotal);
        Assert.AreEqual(2 * infected.Sum(p => p.HunterDpDmg), player.HunterDpDmg);
        Assert.AreEqual(2 * mvps, player.MvpSiDamage);
        Assert.AreEqual(player.MvpSiDamage + player.MvpCommon, player.TotalMvp);
    }

    [TestMethod]
    public async Task TheTeamTotalIsTheSumOfItsPlayersAndTheSharesAddUpToOne()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadTwoMapsAsync(hub, server);

        var match = await SingleAsync(hub);

        foreach (var team in match.Teams)
        {
            Assert.AreEqual(team.Players.Sum(player => player.SiDamage), team.SiDamage);
            Assert.AreEqual(team.Players.Sum(player => player.DmgTotal), team.DmgTotal);
            Assert.AreEqual(team.Players.Sum(player => player.MvpSiDamage), team.MvpSiDamage);
            Assert.AreEqual(team.Players.Sum(player => player.FfGiven), team.FfGiven);

            Assert.IsGreaterThan(0, team.SiDamage);
            Assert.AreEqual(1m, Math.Round(team.Players.Sum(player => player.SiDamagePercentage), 6));
            Assert.AreEqual(1m, Math.Round(team.Players.Sum(player => player.MvpSiDamagePercentage), 6));

            foreach (var player in team.Players)
            {
                Assert.AreEqual(player.SiDamage / (decimal)team.SiDamage, player.SiDamagePercentage);
                Assert.AreEqual(team.WitchDamage == 0 ? 0 : player.WitchDamage / (decimal)team.WitchDamage, player.WitchDamagePercentage);
            }
        }
    }

    [TestMethod]
    public async Task TheListBringsTheSameResultAsTheSearchById()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadTwoMapsAsync(hub, server);
        await hub.UploadAsync(server, StatisticsBuilder.Map("c8m1_apartment", Night.AddDays(1), 1, 100, 50));

        var page = await hub.RunAsync(services => services.GetRequiredService<IMatchResultService>().ListAsync(new MatchFilter { PageSize = 1, Page = 2 }));
        var found = await hub.RunAsync(services => services.GetRequiredService<IMatchResultService>().FindAsync(page.Matches[0].Id));

        Assert.AreEqual(2, page.Total);
        Assert.AreEqual(2, page.Pages);
        Assert.HasCount(1, page.Matches);
        Assert.AreEqual("Dark Carnival", page.Matches[0].Campaign);
        Assert.AreEqual(server.Name, page.Matches[0].GameServerName);
        CollectionAssert.AreEqual(found!.Maps.ToList(), page.Matches[0].Maps.ToList());
        CollectionAssert.AreEqual(
            found.Teams.SelectMany(team => team.Players).Select(player => (player.CommunityId, player.Name, player.SiDamage, player.TotalMvp)).ToList(),
            page.Matches[0].Teams.SelectMany(team => team.Players).Select(player => (player.CommunityId, player.Name, player.SiDamage, player.TotalMvp)).ToList());
    }

    [TestMethod]
    public async Task TheStatisticsOfAMatchAreEveryFileBetweenItsFirstAndItsLastMap()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 500, 400));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(30), 3, 600, 500));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c8m1_apartment", Night.AddDays(1), 1, 100, 50));

        var match = (await ListAsync(hub)).Single(result => result.Campaign == "Dark Carnival");
        var statistics = await hub.RunAsync(services => services.GetRequiredService<IMatchResultService>().StatisticsAsync(match));

        Assert.HasCount(2, match.Maps);
        Assert.HasCount(3, statistics);
        CollectionAssert.AreEqual(new[] { "c2m1_highway", "c2m2_fairgrounds", "c2m2_fairgrounds" }, statistics.Select(result => result.Statistic!.GameRound!.MapName).ToList());
        CollectionAssert.AreEqual(new[] { 300, 500, 600 }, statistics.Select(result => result.Statistic!.Scoring!.TeamA!.Score).ToList());
        Assert.IsTrue(statistics.All(result => result.FileId > 0 && result.FileName.EndsWith(".txt", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task TheKeysOfAnOldAddressFindTheMatchAndItsFile()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await UploadTwoMapsAsync(hub, server);

        var match = await SingleAsync(hub);
        var start = StatisticsRowKey.From(match.LastFileDate);
        var end = StatisticsRowKey.From(match.FirstFileDate);

        var id = await hub.RunAsync(services => services.GetRequiredService<IMatchResultService>().FindIdAsync(StatisticsRowKey.ToDate(end)!.Value, StatisticsRowKey.ToDate(start)!.Value));
        var fileId = await hub.RunAsync(services => services.GetRequiredService<IMatchResultService>().FindFileIdAsync(match.Id, StatisticsRowKey.ToDate(start)!.Value));
        var unknown = await hub.RunAsync(services => services.GetRequiredService<IMatchResultService>().FindIdAsync(Night.AddYears(-1), Night.AddYears(-1)));
        var outside = await hub.RunAsync(services => services.GetRequiredService<IMatchResultService>().FindFileIdAsync(match.Id, Night.AddDays(3)));

        Assert.AreEqual(match.Id, id);
        Assert.AreEqual(match.Maps[^1].FileId, fileId);
        Assert.IsNull(unknown);
        Assert.IsNull(outside);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("abc")]
    [DataRow("-5")]
    [DataRow("1")]
    public void AnInvalidOldKeyGivesNoDate(string? rowKey)
    {
        Assert.IsNull(StatisticsRowKey.ToDate(rowKey));
    }

    [TestMethod]
    public void AnOldKeyIsTheFileDateCountedBackwards()
    {
        var fileDate = new DateTime(2026, 11, 12, 22, 35, 0).AddMilliseconds(8);

        Assert.AreEqual("8584070839854695807", StatisticsRowKey.From(fileDate));
        Assert.AreEqual(fileDate, StatisticsRowKey.ToDate("8584070839854695807"));
    }

    [TestMethod]
    public async Task ADrawHasNoWinnerByScore()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 500, 500));

        var match = await SingleAsync(hub);

        Assert.IsTrue(match.Draw);
        Assert.IsFalse(match.TeamAWon);
        Assert.IsFalse(match.TeamBWon);
        Assert.AreEqual(0, match.Diff);
        Assert.HasCount(4, match.Teams[0].Players);
        Assert.HasCount(4, match.Teams[1].Players);
    }

    private static async Task<StatisticsBuilder> UploadTwoMapsAsync(TestHub hub, GameServer server)
    {
        var last = StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 600, 500);

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200));
        await hub.UploadAsync(server, last);

        return last;
    }

    private static async Task<IReadOnlyList<MatchResult>> ListAsync(TestHub hub)
    {
        var page = await hub.RunAsync(services => services.GetRequiredService<IMatchResultService>().ListAsync(new MatchFilter()));

        return page.Matches;
    }

    private static async Task<MatchResult> SingleAsync(TestHub hub)
    {
        var id = await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.Select(match => match.Id).SingleAsync());
        var match = await hub.RunAsync(services => services.GetRequiredService<IMatchResultService>().FindAsync(id));

        return match!;
    }
}
