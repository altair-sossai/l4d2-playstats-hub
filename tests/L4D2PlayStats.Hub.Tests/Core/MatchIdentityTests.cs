using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Matches.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class MatchIdentityTests
{
    private static readonly DateTime Night = new(2026, 11, 14, 21, 0, 0);

    [TestMethod]
    public async Task AMatchKeepsItsIdWhenItsNextMapArrives()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 600, 500));

        var before = (await MatchesAsync(hub)).Single();

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m3_coaster", Night.AddMinutes(30), 3, 900, 800));

        var after = (await MatchesAsync(hub)).Single();

        Assert.AreEqual(before.Id, after.Id);
        Assert.AreEqual(3, after.MapCount);
        Assert.AreEqual(900, after.TeamAScore);
        Assert.AreEqual(before.Players.Count, after.Players.Count);
        Assert.HasCount(3, after.Files);
        Assert.AreEqual(3 * before.Players.Sum(player => player.SiDamage), 2 * after.Players.Sum(player => player.SiDamage));
    }

    [TestMethod]
    public async Task TheMatchesOfANightKeepTheirIdsWhenAnotherMatchIsPlayed()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 600, 500));

        var first = (await MatchesAsync(hub)).Single();

        await hub.UploadAsync(server, StatisticsBuilder.Map("c8m1_apartment", Night.AddMinutes(60), 1, 400, 300));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c8m2_subway", Night.AddMinutes(75), 2, 700, 900));

        var matches = await MatchesAsync(hub);
        var second = matches.Single(match => match.Campaign == "No Mercy");

        await hub.UploadAsync(server, StatisticsBuilder.Map("c8m3_sewers", Night.AddMinutes(90), 3, 1000, 1200));

        var later = await MatchesAsync(hub);

        Assert.HasCount(2, matches);
        Assert.AreEqual(first.Id, matches.Single(match => match.Campaign == "Dark Carnival").Id);
        Assert.AreNotEqual(first.Id, second.Id);
        Assert.HasCount(2, later);
        Assert.AreEqual(first.Id, later.Single(match => match.Campaign == "Dark Carnival").Id);
        Assert.AreEqual(second.Id, later.Single(match => match.Campaign == "No Mercy").Id);
        Assert.AreEqual(3, later.Single(match => match.Campaign == "No Mercy").MapCount);
    }

    [TestMethod]
    public async Task AMatchKeepsItsIdWhenAScoreIsFixedAndWhenEverythingIsRebuilt()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 600, 500));

        var before = (await MatchesAsync(hub)).Single();
        var lastFile = before.Files.OrderBy(file => file.FileDate).Last();

        await hub.RunAsync(services => services.GetRequiredService<IStatisticsService>().UpdateScoreAsync(lastFile.Id, 400, 700));

        var fixedScore = (await MatchesAsync(hub)).Single();

        await hub.RunAsync(services => services.GetRequiredService<IMatchBuildService>().RebuildAllAsync());

        var rebuilt = (await MatchesAsync(hub)).Single();
        var players = await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().MatchPlayers.CountAsync());

        Assert.AreEqual(before.Id, fixedScore.Id);
        Assert.AreEqual(700, fixedScore.TeamBScore);
        Assert.AreEqual(before.Id, rebuilt.Id);
        Assert.AreEqual(before.Players.Count, rebuilt.Players.Count);
        Assert.AreEqual(before.Players.Count, players);
        CollectionAssert.AreEqual(
            before.Players.OrderBy(player => player.CommunityId).Select(player => (player.CommunityId, player.SiDamage, player.TeamA, player.TeamB)).ToList(),
            rebuilt.Players.OrderBy(player => player.CommunityId).Select(player => (player.CommunityId, player.SiDamage, player.TeamA, player.TeamB)).ToList());
    }

    [TestMethod]
    public async Task AMatchThatNoLongerExistsIsRemoved()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200));

        var before = (await MatchesAsync(hub)).Single();
        var file = before.Files.Single();

        await hub.RunAsync(services => services.GetRequiredService<IStatisticsService>().UpdateScoreAsync(file.Id, 0, 0));

        var matches = await MatchesAsync(hub);
        var players = await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().MatchPlayers.CountAsync());

        Assert.IsEmpty(matches);
        Assert.AreEqual(0, players);
    }

    private static Task<List<Match>> MatchesAsync(TestHub hub)
    {
        return hub.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches
            .AsNoTracking()
            .Include(match => match.Players)
            .Include(match => match.Files)
            .OrderBy(match => match.FirstFileDate)
            .ToListAsync());
    }
}