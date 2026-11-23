using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Helpers;
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

    private static readonly DateTime Night = new(2026, 11, 14, 21, 0, 0);

    [TestMethod]
    public async Task TheSubstituteGetsTheResultAndThePlayerWhoLeftLosesTheRageQuit()
    {
        using var hub = await SubstitutionAsync();

        var before = await MatchAsync(hub);

        Assert.AreEqual(1, Row(before, Left)!.RageQuits);
        Assert.IsTrue(Row(before, Substitute)!.TeamA);
        Assert.IsFalse(Row(before, Substitute)!.Win);

        var files = await FilesAsync(hub);
        var result = await ReplaceAsync(hub, before.Id, [files[0].Id], Left, Substitute);
        var after = await MatchAsync(hub);

        Assert.IsTrue(result.Replaced);
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

        await ReplaceAsync(hub, match.Id, [files[0].Id], Left, Substitute);

        var after = await RankingAsync(hub, match.SeasonId);

        Assert.AreEqual(1, before.Single(p => p.CommunityId == Left).RageQuit);
        Assert.AreEqual(0, before.Single(p => p.CommunityId == Substitute).Wins);
        Assert.IsFalse(after.Any(p => p.CommunityId == Left));
        Assert.AreEqual(1, after.Single(p => p.CommunityId == Substitute).Wins);
    }

    [TestMethod]
    public async Task TheSubstituteTakesTheNumbersOfThePlayerWhoLeftInTheChosenMap()
    {
        using var hub = await SubstitutionAsync();

        var match = await MatchAsync(hub);
        var files = await FilesAsync(hub);
        var original = Parse(files[0].Content);
        var leftSurvivor = original.Halves.SelectMany(h => h.Players).Single(p => p.CommunityId == Left.ToString());
        var leftInfected = original.Halves.SelectMany(h => h.InfectedPlayers).Single(p => p.CommunityId == Left.ToString());

        await ReplaceAsync(hub, match.Id, [files[0].Id], Left, Substitute);

        var changed = Parse((await FilesAsync(hub))[0].Content);
        var survivor = changed.Halves.SelectMany(h => h.Players).Single(p => p.CommunityId == Substitute.ToString());
        var infected = changed.Halves.SelectMany(h => h.InfectedPlayers).Single(p => p.CommunityId == Substitute.ToString());

        Assert.IsFalse(PlayerReplacement.Plays(changed, Left));
        Assert.AreEqual(leftSurvivor.SiDamage, survivor.SiDamage);
        Assert.AreEqual(leftSurvivor.Common, survivor.Common);
        Assert.AreEqual(leftSurvivor.Index, survivor.Index);
        Assert.AreEqual(leftInfected.DmgTotal, infected.DmgTotal);
        Assert.AreEqual("Sub", survivor.PlayerName);
        Assert.AreEqual(original.Halves.Sum(h => h.Players.Count), changed.Halves.Sum(h => h.Players.Count));
    }

    [TestMethod]
    public async Task OnlyTheChosenMapsChange()
    {
        using var hub = await SubstitutionAsync();

        var match = await MatchAsync(hub);
        var before = await FilesAsync(hub);

        await ReplaceAsync(hub, match.Id, [before[0].Id], Left, Substitute);

        var after = await FilesAsync(hub);

        Assert.AreNotEqual(before[0].Content, after[0].Content);
        Assert.AreEqual(before[1].Content, after[1].Content);
    }

    [TestMethod]
    public async Task ANewPlayerIsAddedToThePlayerNamesOfTheMap()
    {
        using var hub = await SubstitutionAsync();

        var match = await MatchAsync(hub);
        var files = await FilesAsync(hub);
        var namesBefore = Parse(files[0].Content).PlayerNames.Count;

        await ReplaceAsync(hub, match.Id, [files[0].Id], Left, Substitute);

        var changed = Parse((await FilesAsync(hub))[0].Content);

        Assert.HasCount(namesBefore + 1, changed.PlayerNames);
        Assert.AreEqual("Sub", changed.PlayerNames.Single(p => p.CommunityId == Substitute.ToString()).Name);
        Assert.IsTrue(changed.PlayerNames.Any(p => p.CommunityId == Left.ToString()));
    }

    [TestMethod]
    public async Task APlayerNeverSeenTakesTheNameGivenByTheAdministrator()
    {
        using var hub = await SubstitutionAsync();

        var match = await MatchAsync(hub);
        var files = await FilesAsync(hub);

        await ReplaceAsync(hub, match.Id, [files[1].Id], Substitute, Stranger, "New\nGuy");

        var changed = Parse((await FilesAsync(hub))[1].Content);

        Assert.AreEqual("New Guy", changed.PlayerNames.Single(p => p.CommunityId == Stranger.ToString()).Name);
    }

    [TestMethod]
    public async Task ReplacingThePlayersBackGivesTheSameFile()
    {
        using var hub = await SubstitutionAsync();

        var match = await MatchAsync(hub);
        var before = await FilesAsync(hub);

        await ReplaceAsync(hub, match.Id, [before[1].Id], Substitute, Left);
        await ReplaceAsync(hub, match.Id, [before[1].Id], Left, Substitute);

        var after = await FilesAsync(hub);

        Assert.AreEqual(before[1].Content, after[1].Content);
    }

    [TestMethod]
    public async Task AMapThePlayerWhoLeftDidNotPlayStopsTheReplacementInEveryMap()
    {
        using var hub = await SubstitutionAsync();

        var match = await MatchAsync(hub);
        var before = await FilesAsync(hub);

        var result = await ReplaceAsync(hub, match.Id, [before[0].Id, before[1].Id], Left, Stranger);
        var after = await FilesAsync(hub);

        Assert.AreEqual(PlayerReplacementError.NotInMap, result.Error);
        Assert.AreEqual("c2m2_fairgrounds", result.MapName);
        Assert.AreEqual(before[0].Content, after[0].Content);
        Assert.AreEqual(before[1].Content, after[1].Content);
    }

    [TestMethod]
    public async Task AMapTheSubstituteAlreadyPlayedIsRefused()
    {
        using var hub = await SubstitutionAsync();

        var match = await MatchAsync(hub);
        var files = await FilesAsync(hub);
        var teammate = StatisticsBuilder.Sample().Survivors('A').First(id => id != Left);

        var result = await ReplaceAsync(hub, match.Id, [files[1].Id], teammate, Substitute);

        Assert.AreEqual(PlayerReplacementError.AlreadyInMap, result.Error);
        Assert.AreEqual("c2m2_fairgrounds", result.MapName);
    }

    [TestMethod]
    public async Task TheSamePlayerOrNoMapIsRefused()
    {
        using var hub = await SubstitutionAsync();

        var match = await MatchAsync(hub);
        var files = await FilesAsync(hub);

        Assert.AreEqual(PlayerReplacementError.SamePlayer, (await ReplaceAsync(hub, match.Id, [files[0].Id], Left, Left)).Error);
        Assert.AreEqual(PlayerReplacementError.NoMaps, (await ReplaceAsync(hub, match.Id, [], Left, Substitute)).Error);
    }

    [TestMethod]
    public async Task AMapOfAnotherMatchOrAMatchThatDoesNotExistIsRefused()
    {
        using var hub = await SubstitutionAsync();

        var server = (await hub.CreateServerAsync("Other Server")).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c5m1_waterfront", Night, 1, 300, 200));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c5m2_park", Night.AddMinutes(15), 2, 600, 500));

        var matches = await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.AsNoTracking().OrderBy(m => m.Id).ToListAsync());
        var otherFile = (await FilesAsync(hub)).First(f => f.MatchId == matches[1].Id);

        Assert.AreEqual(PlayerReplacementError.MapNotInMatch, (await ReplaceAsync(hub, matches[0].Id, [otherFile.Id], Left, Stranger)).Error);
        Assert.AreEqual(PlayerReplacementError.MatchNotFound, (await ReplaceAsync(hub, matches[1].Id + 1000, [otherFile.Id], Left, Stranger)).Error);
    }

    // The player who left played the first map; the substitute played the second, which team A won.
    private static async Task<TestHub> SubstitutionAsync()
    {
        var hub = new TestHub();
        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 900, 450).WithSubstitute(Left, Substitute, "Sub"));

        return hub;
    }

    private static Task<PlayerReplacementResult> ReplaceAsync(TestHub hub, int matchId, int[] fileIds, long replaced, long replacement, string? name = null)
    {
        return hub.RunAsync(services => services.GetRequiredService<IStatisticsService>().ReplacePlayerAsync(matchId, fileIds, replaced, replacement, name));
    }

    private static Task<Match> MatchAsync(TestHub hub)
    {
        return hub.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.AsNoTracking().Include(m => m.Players).OrderBy(m => m.Id).FirstAsync());
    }

    private static Task<List<StatisticsFile>> FilesAsync(TestHub hub)
    {
        return hub.RunAsync(services => services.GetRequiredService<HubDbContext>().StatisticsFiles.AsNoTracking().OrderBy(f => f.FileDate).ToListAsync());
    }

    private static Task<IReadOnlyList<L4D2PlayStats.Hub.Core.Modules.Ranking.Models.RankingPlayer>> RankingAsync(TestHub hub, int seasonId)
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
