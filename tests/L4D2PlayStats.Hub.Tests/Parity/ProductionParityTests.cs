using System.Text.Json;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Core.Modules.Seasons;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Helpers;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using L4D2PlayStats.Hub.Core.Modules.Matches.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Punishments.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Services.Contracts;
using ParsedStatistics = L4D2PlayStats.Statistics;

namespace L4D2PlayStats.Hub.Tests.Parity;

[TestClass]
[TestCategory("Parity")]
public class ProductionParityTests
{
    private const string DumpVariable = "HUB_PARITY_DUMP";
    private const string CurrentSeasonVariable = "HUB_PARITY_CURRENT_SEASON";
    private const string LiveRankingFile = "ranking_current.json";

    private static readonly DateTime CurrentRulesSince = new(2026, 7, 1);

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task SeasonRankingsMatchTheProductionApi()
    {
        var dump = Environment.GetEnvironmentVariable(DumpVariable);
        if (string.IsNullOrEmpty(dump) || !Directory.Exists(dump))
            Assert.Inconclusive($"Set {DumpVariable} to a folder created by the importer's dump command.");

        using var hub = new TestHub();

        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new RankingSettings { IncludeUnknownCampaigns = false }));

        var server = (await hub.CreateServerAsync("Production")).Server;
        var imported = await ImportAsync(hub, server, Directory.GetDirectories(Path.Combine(dump, "statistics")).Single());
        var matches = await hub.RunAsync(services => services.GetRequiredService<IMatchBuildService>().RebuildAllAsync());

        TestContext.WriteLine($"files: {imported}, matches: {matches}");

        var seasons = await hub.RunAsync(services => services.GetRequiredService<ISeasonService>().ListAsync());
        var mismatches = 0;
        var compared = 0;

        foreach (var season in seasons.OrderBy(s => s.StartsAt))
        {
            var expectedFile = ExpectedFile(dump, season);
            if (expectedFile == null || season.StartsAt < CurrentRulesSince)
                continue;

            var live = Path.GetFileName(expectedFile) == LiveRankingFile;

            var expected = JsonSerializer.Deserialize<List<ExpectedPlayer>>(await File.ReadAllTextAsync(expectedFile), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
            foreach (var punished in expected.Where(p => p.Punishment > 0))
                await hub.RunAsync(services => services.GetRequiredService<IPunishmentService>().AddAsync(season.Id, punished.CommunityId, punished.Punishment, null, null));

            var actual = await hub.RunAsync(services => services.GetRequiredService<IRankingService>().SeasonAsync(season.Id));
            var differences = Compare(expected, actual, live);

            compared++;
            mismatches += differences.Count;

            TestContext.WriteLine($"{season.StartsAt:yyyy-MM}: expected {expected.Count}, actual {actual.Count}, differences {differences.Count}");

            foreach (var difference in differences.Take(8))
                TestContext.WriteLine($"    {difference}");
        }

        Assert.IsGreaterThanOrEqualTo(3, compared);
        Assert.AreEqual(0, mismatches);
    }

    [TestMethod]
    public async Task UploadingOneByOneGivesTheSameResultAsReprocessing()
    {
        var dump = Environment.GetEnvironmentVariable(DumpVariable);
        if (string.IsNullOrEmpty(dump) || !Directory.Exists(dump))
            Assert.Inconclusive($"Set {DumpVariable} to a folder created by the importer's dump command.");

        var directory = Directory.GetDirectories(Path.Combine(dump, "statistics")).Single();
        var files = Directory.GetFiles(directory, "2026-0*").OrderBy(f => f, StringComparer.Ordinal).ToList();

        using var incremental = new TestHub();
        using var bulk = new TestHub();

        var incrementalServer = (await incremental.CreateServerAsync("Production")).Server;
        var bulkServer = (await bulk.CreateServerAsync("Production")).Server;

        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file);

            await incremental.RunAsync(async services =>
            {
                var gameServer = await services.GetRequiredService<IGameServerService>().FindAsync(incrementalServer.Id);

                await services.GetRequiredService<IStatisticsService>().UploadAsync(gameServer!, Path.GetFileName(file), content);
            });
        }

        await ImportAsync(bulk, bulkServer, directory, "2026-0*");
        await bulk.RunAsync(services => services.GetRequiredService<IMatchBuildService>().RebuildAllAsync());

        var incrementalMatches = await DescribeAsync(incremental);
        var bulkMatches = await DescribeAsync(bulk);

        TestContext.WriteLine($"files: {files.Count}, matches: {bulkMatches.Count}");

        CollectionAssert.AreEqual(bulkMatches, incrementalMatches);
    }

    private static async Task<int> ImportAsync(TestHub hub, GameServer server, string directory, string pattern = "*.txt")
    {
        var count = 0;

        await hub.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();
            var seasonService = services.GetRequiredService<ISeasonService>();

            foreach (var path in Directory.GetFiles(directory, pattern).OrderBy(f => f, StringComparer.Ordinal))
            {
                var fileName = Path.GetFileName(path);
                var content = await File.ReadAllTextAsync(path);
                var fileDate = StatisticsFileName.ToDate(fileName);

                if (fileDate == null || !ParsedStatistics.TryParse(content, out var statistics) || statistics == null)
                    continue;

                var season = await seasonService.GetOrCreateAsync(fileDate.Value);
                var file = new StatisticsFile
                {
                    GameServerId = server.Id,
                    SeasonId = season.Id,
                    FileName = fileName,
                    FileDate = fileDate.Value,
                    Content = content,
                    UploadedAtUtc = hub.Clock.UtcNow
                };

                file.Apply(statistics);
                dbContext.StatisticsFiles.Add(file);
                count++;
            }

            await dbContext.SaveChangesAsync();
        });

        return count;
    }

    private static Task<List<string>> DescribeAsync(TestHub hub)
    {
        return hub.RunAsync(async services =>
        {
            var matches = await services.GetRequiredService<HubDbContext>().Matches
                .AsNoTracking()
                .Include(m => m.Players)
                .OrderBy(m => m.LastFileDate)
                .ToListAsync();

            return matches.Select(match =>
            {
                var players = string.Join(",", match.Players.OrderBy(p => p.Order).Select(p => $"{p.CommunityId}:{p.Win}:{p.Loss}:{p.Draws}:{p.RageQuits}:{p.TeamA}:{p.TeamB}:{p.MvpSiDamage}:{p.MvpCommon}:{p.SiDamage}"));

                return $"{match.LastFileDate:O}|{match.Campaign}|{match.MapCount}|{match.TeamAScore}|{match.TeamBScore}|{match.CountsForRanking}|{players}";
            }).ToList();
        });
    }

    private static string? ExpectedFile(string dump, Season season)
    {
        var last = season.EndsAt.AddMonths(-1);
        var history = Path.Combine(dump, "expected", $"history_{season.StartsAt:yyyyMM}{last:yyyyMM}.json");

        if (File.Exists(history))
            return history;

        var current = Path.Combine(dump, "expected", LiveRankingFile);
        var currentSeason = Environment.GetEnvironmentVariable(CurrentSeasonVariable);

        return File.Exists(current) && currentSeason == season.StartsAt.ToString("yyyy-MM") ? current : null;
    }

    private static List<string> Compare(List<ExpectedPlayer> expected, IReadOnlyList<RankingPlayer> actual, bool live)
    {
        var differences = new List<string>();
        var byId = actual.ToDictionary(p => p.CommunityId);

        foreach (var player in expected)
        {
            if (!byId.TryGetValue(player.CommunityId, out var found))
            {
                differences.Add($"#{player.Position} {player.CommunityId} missing");
                continue;
            }

            var description = $"#{player.Position} {player.CommunityId}";

            if (found.Position != player.Position)
                differences.Add($"{description} position {found.Position}");

            if (found.Experience != player.Experience)
                differences.Add($"{description} experience {found.Experience} expected {player.Experience}");

            if (found.Wins != player.Wins || found.Loss != player.Loss || found.Draws != player.Draws || found.RageQuit != player.RageQuit)
                differences.Add($"{description} results {found.Wins}/{found.Loss}/{found.Draws}/{found.RageQuit} expected {player.Wins}/{player.Loss}/{player.Draws}/{player.RageQuit}");

            if (found.MvpSiDamage != player.MvpSiDamage || found.MvpCommon != player.MvpCommon)
                differences.Add($"{description} mvps {found.MvpSiDamage}/{found.MvpCommon} expected {player.MvpSiDamage}/{player.MvpCommon}");

            if (found.SiDamage != player.SiDamage || found.Common != player.Common || found.DmgTotal != player.DmgTotal)
                differences.Add($"{description} statistics differ");

            if (live && found.MainName != player.MainName)
                differences.Add($"{description} main name differs");

            if (live && found.PreviousExperience != player.PreviousExperience)
                differences.Add($"{description} previous experience {found.PreviousExperience} expected {player.PreviousExperience}");

            if (live && (found.MaxWinStreak != player.MaxWinStreak || found.MaxLossStreak != player.MaxLossStreak))
                differences.Add($"{description} streaks differ");
        }

        return differences;
    }

    private class ExpectedPlayer
    {
        public long CommunityId { get; set; }
        public int Position { get; set; }
        public string? MainName { get; set; }
        public decimal Experience { get; set; }
        public int Wins { get; set; }
        public int Loss { get; set; }
        public int Draws { get; set; }
        public int RageQuit { get; set; }
        public int MvpSiDamage { get; set; }
        public int MvpCommon { get; set; }
        public int SiDamage { get; set; }
        public int Common { get; set; }
        public int DmgTotal { get; set; }
        public int Punishment { get; set; }
        public decimal? PreviousExperience { get; set; }
        public int MaxWinStreak { get; set; }
        public int MaxLossStreak { get; set; }
    }
}
