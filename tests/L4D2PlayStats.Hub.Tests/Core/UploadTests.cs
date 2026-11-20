using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Models;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class UploadTests
{
    private static readonly DateTime Night = new(2026, 11, 14, 21, 0, 0);

    [TestMethod]
    public async Task ACompleteFileIsStoredAndCanBeDeleted()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;
        var result = await hub.UploadAsync(server, StatisticsBuilder.Sample());
        var file = await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().StatisticsFiles.SingleAsync());

        Assert.IsTrue(result.MustBeDeleted);
        Assert.AreEqual(StatisticsBuilder.SampleFileName, result.FileName);
        Assert.AreEqual("c8m1_apartment", file.MapName);
        Assert.AreEqual("VanillaMod v2.8", file.ConfigurationName);
        Assert.AreEqual(4, file.TeamSize);
        Assert.AreEqual(301, file.TeamAScore);
        Assert.AreEqual(392, file.TeamBScore);
        Assert.AreEqual(new DateTime(2023, 2, 4, 3, 13, 0).AddMilliseconds(11), file.FileDate);
    }

    [TestMethod]
    public async Task SendingTheSameFileAgainDoesNotDuplicateIt()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Sample());
        await hub.UploadAsync(server, StatisticsBuilder.Sample());

        var counts = await hub.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            return (Files: await dbContext.StatisticsFiles.CountAsync(), Matches: await dbContext.Matches.CountAsync());
        });

        Assert.AreEqual(1, counts.Files);
        Assert.AreEqual(1, counts.Matches);
    }

    [TestMethod]
    public async Task TwoServersCanSendFilesWithTheSameName()
    {
        using var hub = new TestHub();

        var first = (await hub.CreateServerAsync("First")).Server;
        var second = (await hub.CreateServerAsync("Second")).Server;

        await hub.UploadAsync(first, StatisticsBuilder.Sample());
        await hub.UploadAsync(second, StatisticsBuilder.Sample());

        var files = await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().StatisticsFiles.CountAsync());

        Assert.AreEqual(2, files);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("not a statistics file")]
    public async Task UnreadableContentIsRejected(string? content)
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        hub.Clock.UtcNow = new DateTime(2023, 2, 4, 3, 30, 0, DateTimeKind.Utc);

        await Assert.ThrowsExactlyAsync<StatisticsRejectedException>(() => UploadAsync(hub, server, StatisticsBuilder.SampleFileName, content));
    }

    [TestMethod]
    public async Task ARecentFileWithOnlyTheFirstHalfIsKeptOnTheServer()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;
        var content = FirstHalfOnly();

        hub.Clock.UtcNow = new DateTime(2023, 2, 4, 3, 30, 0, DateTimeKind.Utc);

        await Assert.ThrowsExactlyAsync<StatisticsRejectedException>(() => UploadAsync(hub, server, StatisticsBuilder.SampleFileName, content));
    }

    [TestMethod]
    public async Task AnOldFileThatNeverCompletedIsDiscarded()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;
        var result = await UploadAsync(hub, server, StatisticsBuilder.SampleFileName, FirstHalfOnly());
        var files = await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().StatisticsFiles.CountAsync());

        Assert.IsTrue(result.MustBeDeleted);
        Assert.AreEqual(0, files);
    }

    [TestMethod]
    public async Task AFileWithoutADateInTheNameIsDiscarded()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;
        var result = await UploadAsync(hub, server, "statistics.txt", StatisticsBuilder.SampleContent());

        Assert.IsTrue(result.MustBeDeleted);
    }

    [TestMethod]
    public async Task EveryConfigurationSeenIsRecorded()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200).WithConfiguration("ZoneMod v2.9.2b"));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 600, 500).WithConfiguration("ZoneMod v2.9.2b"));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c5m1_waterfront", Night.AddDays(1), 1, 300, 200).WithConfiguration("3v3 Zonemod v2.9.2b", 3));

        var configurations = await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().GameConfigurations.OrderBy(c => c.Name).ToListAsync());

        Assert.HasCount(2, configurations);
        Assert.AreEqual("3v3 Zonemod v2.9.2b", configurations[0].Name);
        Assert.AreEqual(3, configurations[0].TeamSize);
        Assert.AreEqual("ZoneMod v2.9.2b", configurations[1].Name);
    }

    [TestMethod]
    public async Task FixingAScoreChangesTheWinner()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 600, 500));

        var before = await RankingAsync(hub);
        var lastFile = await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().StatisticsFiles.OrderByDescending(f => f.FileDate).FirstAsync());

        await hub.RunAsync(services => services.GetRequiredService<IStatisticsService>().UpdateScoreAsync(lastFile.Id, 400, 700));

        var after = await RankingAsync(hub);
        var winnerBefore = before.First(p => p.Wins == 1).CommunityId;

        Assert.AreEqual(1, after.First(p => p.CommunityId == winnerBefore).Loss);
        Assert.AreEqual(0, after.First(p => p.CommunityId == winnerBefore).Wins);
    }

    [TestMethod]
    [DataRow(-1, 10)]
    [DataRow(10, 10001)]
    public async Task AnImpossibleScoreIsRefused(int teamAScore, int teamBScore)
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Sample());

        var file = await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().StatisticsFiles.SingleAsync());
        var updated = await hub.RunAsync(services => services.GetRequiredService<IStatisticsService>().UpdateScoreAsync(file.Id, teamAScore, teamBScore));

        Assert.IsNull(updated);
    }

    private static Task<IReadOnlyList<RankingPlayer>> RankingAsync(TestHub hub)
    {
        return hub.RunAsync(async services =>
        {
            var season = await services.GetRequiredService<HubDbContext>().Seasons.SingleAsync();

            return await services.GetRequiredService<IRankingService>().SeasonAsync(season.Id);
        });
    }

    private static Task<UploadResult> UploadAsync(TestHub hub, GameServer server, string? fileName, string? content)
    {
        return hub.RunAsync(async services =>
        {
            var gameServer = await services.GetRequiredService<IGameServerService>().FindAsync(server.Id);

            return await services.GetRequiredService<IStatisticsService>().UploadAsync(gameServer!, fileName, content);
        });
    }

    private static string FirstHalfOnly()
    {
        var content = StatisticsBuilder.SampleContent();
        var secondHalf = content.IndexOf("[RoundHalf:1]", StringComparison.Ordinal);

        return content[..secondHalf];
    }
}
