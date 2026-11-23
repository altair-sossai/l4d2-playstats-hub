using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Models;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ParsedStatistics = L4D2PlayStats.Statistics;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class PlayerReplacementTests
{
    private const long Left = StatisticsBuilder.PlayerA1;
    private const long Substitute = 76561198000000777;
    private const long Stranger = 76561198000000888;

    // In the sample, team A plays the first round half as survivors and the second as infected.
    private const int FirstHalf = 0;
    private const int SecondHalf = 1;

    private static readonly DateTime Night = new(2026, 11, 14, 21, 0, 0);

    // Match 1873 of l4d2.com.br: tom was a survivor in the first half of the first map and left; hard came in as an
    // infected of the same team in the second half and played the rest of the match.
    [TestMethod]
    public async Task ReplacingThePlayerWhoLeftDuringAMapGivesTheResultToTheSubstitute()
    {
        using var hub = await SubstitutionAsync();

        var before = await MatchAsync(hub);
        var files = await FilesAsync(hub);

        Assert.AreEqual(1, Row(before, Left)!.RageQuits);
        Assert.IsFalse(Row(before, Substitute)!.Win);

        var result = await ReplaceAsync(hub, files[0].Id, FirstHalf, true, Left, Substitute);
        var after = await MatchAsync(hub);

        Assert.IsTrue(result.Replaced, result.Error?.ToString());
        Assert.AreEqual(after.Id, result.MatchId);
        Assert.AreEqual(before.Id, after.Id);
        Assert.IsTrue(Row(after, Substitute)!.Win);
        Assert.AreEqual(0, Row(after, Substitute)!.RageQuits);
        Assert.IsNull(Row(after, Left));
    }

    [TestMethod]
    public async Task TheRankingFollowsTheReplacement()
    {
        using var hub = await SubstitutionAsync();

        var match = await MatchAsync(hub);
        var files = await FilesAsync(hub);
        var before = await RankingAsync(hub, match.SeasonId);

        await ReplaceAsync(hub, files[0].Id, FirstHalf, true, Left, Substitute);

        var after = await RankingAsync(hub, match.SeasonId);

        Assert.AreEqual(1, before.Single(p => p.CommunityId == Left).RageQuit);
        Assert.AreEqual(0, before.Single(p => p.CommunityId == Substitute).Wins);
        Assert.IsFalse(after.Any(p => p.CommunityId == Left));
        Assert.AreEqual(1, after.Single(p => p.CommunityId == Substitute).Wins);
    }

    [TestMethod]
    public async Task OnlyTheChosenLineChangesAndKeepsItsNumbers()
    {
        using var hub = await TwoMapsAsync();

        var files = await FilesAsync(hub);
        var original = Parse(files[0].Content);
        var line = original.Halves[FirstHalf].Players.Single(p => p.CommunityId == Left.ToString());

        await ReplaceAsync(hub, files[0].Id, FirstHalf, true, Left, Stranger);

        var after = await FilesAsync(hub);
        var changed = Parse(after[0].Content);
        var survivor = changed.Halves[FirstHalf].Players.Single(p => p.CommunityId == Stranger.ToString());

        Assert.IsFalse(changed.Halves[FirstHalf].Players.Any(p => p.CommunityId == Left.ToString()));
        Assert.IsTrue(changed.Halves[SecondHalf].InfectedPlayers.Any(p => p.CommunityId == Left.ToString()));
        Assert.IsFalse(changed.Halves[SecondHalf].InfectedPlayers.Any(p => p.CommunityId == Stranger.ToString()));
        Assert.AreEqual(line.SiDamage, survivor.SiDamage);
        Assert.AreEqual(line.Common, survivor.Common);
        Assert.AreEqual(line.Index, survivor.Index);
        Assert.AreEqual(files[1].Content, after[1].Content);
    }

    [TestMethod]
    public async Task AnInfectedCanBeReplacedToo()
    {
        using var hub = await TwoMapsAsync();

        var files = await FilesAsync(hub);
        var line = Parse(files[0].Content).Halves[SecondHalf].InfectedPlayers.Single(p => p.CommunityId == Left.ToString());

        var result = await ReplaceAsync(hub, files[0].Id, SecondHalf, false, Left, Stranger);
        var changed = Parse((await FilesAsync(hub))[0].Content);

        Assert.IsTrue(result.Replaced, result.Error?.ToString());
        Assert.AreEqual(line.DmgTotal, changed.Halves[SecondHalf].InfectedPlayers.Single(p => p.CommunityId == Stranger.ToString()).DmgTotal);
        Assert.IsTrue(changed.Halves[FirstHalf].Players.Any(p => p.CommunityId == Left.ToString()));
    }

    [TestMethod]
    public async Task ThePlayerNamesOfTheMapGetTheNewPlayer()
    {
        using var hub = await SubstitutionAsync();

        var files = await FilesAsync(hub);
        var namesBefore = Parse(files[0].Content).PlayerNames.Count;

        await ReplaceAsync(hub, files[0].Id, FirstHalf, true, Left, Stranger, "New\nGuy");

        var changed = Parse((await FilesAsync(hub))[0].Content);

        Assert.HasCount(namesBefore + 1, changed.PlayerNames);
        Assert.AreEqual("New Guy", changed.PlayerNames.Single(p => p.CommunityId == Stranger.ToString()).Name);
        Assert.AreEqual("New Guy", changed.Halves[FirstHalf].Players.Single(p => p.CommunityId == Stranger.ToString()).PlayerName);
    }

    [TestMethod]
    public async Task APlayerAlreadySeenKeepsTheNameOfTheSite()
    {
        using var hub = await SubstitutionAsync();

        var files = await FilesAsync(hub);

        await ReplaceAsync(hub, files[0].Id, FirstHalf, true, Left, Substitute, "Typed by the administrator");

        var changed = Parse((await FilesAsync(hub))[0].Content);

        Assert.AreEqual("hard", changed.Halves[FirstHalf].Players.Single(p => p.CommunityId == Substitute.ToString()).PlayerName);
        Assert.AreEqual(1, changed.PlayerNames.Count(p => p.CommunityId == Substitute.ToString()));
    }

    [TestMethod]
    public async Task ReplacingThePlayersBackGivesTheSameFile()
    {
        using var hub = await TwoMapsAsync();

        var before = await FilesAsync(hub);

        await ReplaceAsync(hub, before[1].Id, FirstHalf, true, Left, Stranger);
        await ReplaceAsync(hub, before[1].Id, FirstHalf, true, Stranger, Left);

        var after = Parse((await FilesAsync(hub))[1].Content);
        var original = Parse(before[1].Content);

        CollectionAssert.AreEqual(original.Halves.SelectMany(h => h.Players).Select(p => p.ToString()).ToList(), after.Halves.SelectMany(h => h.Players).Select(p => p.ToString()).ToList());
        CollectionAssert.AreEqual(original.Halves.SelectMany(h => h.InfectedPlayers).Select(p => p.ToString()).ToList(), after.Halves.SelectMany(h => h.InfectedPlayers).Select(p => p.ToString()).ToList());
    }

    [TestMethod]
    public async Task APlayerAlreadyInTheSameHalfAndSideIsRefusedAndNothingChanges()
    {
        using var hub = await TwoMapsAsync();

        var before = await FilesAsync(hub);
        var teammate = StatisticsBuilder.Sample().Survivors('A').First(id => id != Left);

        var result = await ReplaceAsync(hub, before[0].Id, FirstHalf, true, teammate, Left);

        Assert.AreEqual(PlayerReplacementError.AlreadyInHalf, result.Error);
        Assert.AreEqual(before[0].Content, (await FilesAsync(hub))[0].Content);
    }

    [TestMethod]
    public async Task APlayerOfTheOtherTeamIsRefused()
    {
        using var hub = await TwoMapsAsync();

        var files = await FilesAsync(hub);
        var rival = StatisticsBuilder.Sample().Survivors('B')[0];

        var result = await ReplaceAsync(hub, files[0].Id, FirstHalf, true, Left, rival);

        Assert.AreEqual(PlayerReplacementError.OtherTeam, result.Error);
    }

    [TestMethod]
    public async Task APlayerWhoIsNotOnTheChosenLineIsRefused()
    {
        using var hub = await TwoMapsAsync();

        var files = await FilesAsync(hub);

        Assert.AreEqual(PlayerReplacementError.NotInHalf, (await ReplaceAsync(hub, files[0].Id, FirstHalf, false, Left, Stranger)).Error);
        Assert.AreEqual(PlayerReplacementError.NotInHalf, (await ReplaceAsync(hub, files[0].Id, 5, true, Left, Stranger)).Error);
    }

    [TestMethod]
    public async Task TheSamePlayerOrAMapThatDoesNotExistIsRefused()
    {
        using var hub = await TwoMapsAsync();

        var files = await FilesAsync(hub);

        Assert.AreEqual(PlayerReplacementError.SamePlayer, (await ReplaceAsync(hub, files[0].Id, FirstHalf, true, Left, Left)).Error);
        Assert.AreEqual(PlayerReplacementError.FileNotFound, (await ReplaceAsync(hub, files[1].Id + 1000, FirstHalf, true, Left, Stranger)).Error);
    }

    // The player who left was a survivor in the first half of the first map; the substitute came in as an infected
    // in the second half and played the second map, which team A won.
    private static async Task<TestHub> SubstitutionAsync()
    {
        var hub = new TestHub();
        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200).WithSubstitute(Left, Substitute, "hard", fromHalf: SecondHalf));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 900, 450).WithSubstitute(Left, Substitute, "hard"));

        return hub;
    }

    private static async Task<TestHub> TwoMapsAsync()
    {
        var hub = new TestHub();
        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 900, 450));

        return hub;
    }

    private static Task<PlayerReplacementResult> ReplaceAsync(TestHub hub, int fileId, int half, bool survivor, long replaced, long replacement, string? name = null)
    {
        return hub.RunAsync(services => services.GetRequiredService<IStatisticsService>().ReplacePlayerAsync(fileId, half, survivor, replaced, replacement, name));
    }

    private static Task<Match> MatchAsync(TestHub hub)
    {
        return hub.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.AsNoTracking().Include(m => m.Players).OrderBy(m => m.Id).FirstAsync());
    }

    private static Task<List<StatisticsFile>> FilesAsync(TestHub hub)
    {
        return hub.RunAsync(services => services.GetRequiredService<HubDbContext>().StatisticsFiles.AsNoTracking().OrderBy(f => f.FileDate).ToListAsync());
    }

    private static Task<IReadOnlyList<RankingPlayer>> RankingAsync(TestHub hub, int seasonId)
    {
        return hub.RunAsync(services => services.GetRequiredService<IRankingService>().SeasonAsync(seasonId));
    }

    private static MatchPlayer? Row(Match match, long communityId)
    {
        return match.Players.SingleOrDefault(p => p.CommunityId == communityId);
    }

    private static ParsedStatistics Parse(string content)
    {
        return new ParsedStatistics(content);
    }
}
