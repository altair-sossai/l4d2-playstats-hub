using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Matches.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Matches.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ParsedStatistics = L4D2PlayStats.Statistics;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class TankWipeTests
{
    private static readonly DateTime Night = new(2026, 11, 14, 21, 0, 0);

    [TestMethod]
    public void AWipeWithTheTankStillUpGoesToThePlayerWhoDidTheMostDamageAsTank()
    {
        foreach (var half in StatisticsBuilder.Sample().Statistics.Halves)
        {
            var tank = half.InfectedPlayers.MaxBy(player => player.DmgTank + player.DmgTankIncap)!;

            Assert.AreEqual(tank.CommunityId, TankWipes.Player(half));
        }
    }

    [TestMethod]
    public void ThereIsNoTankWipeWhenASurvivorWasStillStanding()
    {
        var half = SampleHalf();

        half.Progress!.Survived = 1;

        Assert.IsNull(TankWipes.Player(half));
    }

    [TestMethod]
    public void ThereIsNoTankWipeWhenTheTankDiedBeforeTheSurvivorsWentDown()
    {
        var half = SampleHalf();

        half.RoundHalf!.StopTimeTank = half.RoundHalf.EndTime!.Value.AddSeconds(-30);

        Assert.IsNull(TankWipes.Player(half));
    }

    [TestMethod]
    public void ThereIsNoTankWipeInAHalfWithoutATank()
    {
        var half = SampleHalf();

        half.RoundHalf!.StartTimeTank = null;
        half.RoundHalf.StopTimeTank = null;

        Assert.IsNull(TankWipes.Player(half));
    }

    [TestMethod]
    public void ThereIsNoTankWipeWhenNoPlayerHitASurvivorAsTank()
    {
        var half = SampleHalf();

        foreach (var player in half.InfectedPlayers)
        {
            player.DmgTank = 0;
            player.DmgTankIncap = 0;
        }

        Assert.IsNull(TankWipes.Player(half));
    }

    [TestMethod]
    public async Task EachTankWipeOfTheMatchCountsForThePlayerWhoWasTheTank()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;
        var expected = await UploadTwoMapsAsync(hub, server);
        var rows = await RowsAsync(hub);

        Assert.IsNotEmpty(expected);

        foreach (var row in rows)
            Assert.AreEqual(expected.GetValueOrDefault(row.CommunityId), row.TankWipes, $"Player {row.CommunityId}");

        var match = await hub.RunAsync(services => services.GetRequiredService<IMatchResultService>().FindAsync(rows[0].MatchId));

        foreach (var team in match!.Teams)
        {
            Assert.AreEqual(team.Players.Sum(player => player.TankWipes), team.TankWipes);

            if (team.TankWipes > 0)
                Assert.AreEqual(1m, Math.Round(team.Players.Sum(player => player.TankWipesPercentage), 6));
        }
    }

    [TestMethod]
    public async Task TheBackfillCountsTheTankWipesOfStoredMatchesOnlyOnce()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;
        var expected = await UploadTwoMapsAsync(hub, server);

        await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().MatchPlayers.ExecuteUpdateAsync(setters => setters.SetProperty(p => p.TankWipes, 0)));

        var updated = await hub.RunAsync(services => services.GetRequiredService<IMatchBuildService>().BackfillTankWipesAsync());
        var rows = await RowsAsync(hub);

        Assert.AreEqual(expected.Count, updated);

        foreach (var row in rows)
            Assert.AreEqual(expected.GetValueOrDefault(row.CommunityId), row.TankWipes);

        await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().MatchPlayers.ExecuteUpdateAsync(setters => setters.SetProperty(p => p.TankWipes, 0)));

        Assert.AreEqual(0, await hub.RunAsync(services => services.GetRequiredService<IMatchBuildService>().BackfillTankWipesAsync()));
        Assert.IsTrue((await RowsAsync(hub)).All(row => row.TankWipes == 0));
    }

    private static ParsedStatistics.Half SampleHalf()
    {
        return StatisticsBuilder.Sample().Statistics.Halves[0];
    }

    private static async Task<Dictionary<long, int>> UploadTwoMapsAsync(TestHub hub, GameServer server)
    {
        var first = StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200);
        var last = StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 600, 500);

        await hub.UploadAsync(server, first);
        await hub.UploadAsync(server, last);

        return TankWipes.Count([first.Statistics, last.Statistics]);
    }

    private static Task<List<MatchPlayer>> RowsAsync(TestHub hub)
    {
        return hub.RunAsync(services => services.GetRequiredService<HubDbContext>().MatchPlayers.AsNoTracking().OrderBy(p => p.Id).ToListAsync());
    }
}
