using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Punishments;
using L4D2PlayStats.Hub.Core.Modules.Seasons;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace L4D2PlayStats.Hub.Core.Modules.Ranking;

public interface IRankingService
{
    Task<IReadOnlyList<RankingPlayer>> SeasonAsync(int seasonId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RankingPlayer>> CurrentAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RankingPlayer>> YearAsync(int year, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RankingPlayer>> AllTimeAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RankingPlayer>> MatchAsync(int matchId, CancellationToken cancellationToken = default);
    Task<Dictionary<long, int>> StarsAsync(CancellationToken cancellationToken = default);
}

public class RankingService(
    HubDbContext dbContext,
    HubState hubState,
    IMemoryCache memoryCache,
    ISeasonService seasonService,
    IPunishmentService punishmentService,
    ISettingsService settingsService) : IRankingService
{
    public Task<IReadOnlyList<RankingPlayer>> SeasonAsync(int seasonId, CancellationToken cancellationToken = default)
    {
        return CachedAsync($"season_{seasonId}", async () =>
        {
            var punishments = await punishmentService.TotalsAsync(seasonId, cancellationToken);
            var matches = await LoadAsync(dbContext.Matches.Where(m => m.SeasonId == seasonId), cancellationToken);

            return await CalculateAsync(matches, punishments, cancellationToken);
        });
    }

    public async Task<IReadOnlyList<RankingPlayer>> CurrentAsync(CancellationToken cancellationToken = default)
    {
        var season = await seasonService.GetCurrentAsync(cancellationToken);

        return await SeasonAsync(season.Id, cancellationToken);
    }

    public Task<IReadOnlyList<RankingPlayer>> YearAsync(int year, CancellationToken cancellationToken = default)
    {
        return CachedAsync($"year_{year}", async () =>
        {
            var start = new DateTime(year, 1, 1);
            var end = start.AddYears(1);
            var matches = await LoadAsync(dbContext.Matches.Where(m => m.LastFileDate >= start && m.LastFileDate < end), cancellationToken);

            return await CalculateAsync(matches, new Dictionary<long, int>(), cancellationToken);
        });
    }

    public Task<IReadOnlyList<RankingPlayer>> AllTimeAsync(CancellationToken cancellationToken = default)
    {
        return CachedAsync("all_time", async () =>
        {
            var matches = await LoadAsync(dbContext.Matches, cancellationToken);

            return await CalculateAsync(matches, new Dictionary<long, int>(), cancellationToken);
        });
    }

    public async Task<IReadOnlyList<RankingPlayer>> MatchAsync(int matchId, CancellationToken cancellationToken = default)
    {
        var match = await dbContext.Matches.AsNoTracking().FirstOrDefaultAsync(m => m.Id == matchId, cancellationToken);
        if (match == null)
            return [];

        var punishments = await punishmentService.TotalsAsync(match.SeasonId, cancellationToken);
        var matches = await LoadAsync(dbContext.Matches.Where(m => m.Id == matchId), cancellationToken, false);

        return await CalculateAsync(matches, punishments, cancellationToken);
    }

    public Task<Dictionary<long, int>> StarsAsync(CancellationToken cancellationToken = default)
    {
        return CachedAsync("stars", async () =>
        {
            var rows = await dbContext.MatchPlayers
                .AsNoTracking()
                .Where(p => p.Match!.CountsForRanking && (p.TeamA || p.TeamB))
                .Select(p => new { p.CommunityId, p.TeamA, p.TeamB, p.MvpSiDamage, p.MvpCommon })
                .ToListAsync(cancellationToken);

            return rows
                .GroupBy(row => row.CommunityId)
                .ToDictionary(group => group.Key, group => group.Sum(row => (row.MvpSiDamage + row.MvpCommon) * ((row.TeamA ? 1 : 0) + (row.TeamB ? 1 : 0))));
        });
    }

    private async Task<List<RankingMatch>> LoadAsync(IQueryable<Match> query, CancellationToken cancellationToken, bool rankingOnly = true)
    {
        if (rankingOnly)
            query = query.Where(m => m.CountsForRanking);

        var matches = await query
            .AsNoTracking()
            .Include(m => m.Players)
            .OrderBy(m => m.LastFileDate)
            .ThenBy(m => m.Id)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return [.. matches.Select(m => new RankingMatch(m.Id, m.Campaign, m.Players))];
    }

    private async Task<IReadOnlyList<RankingPlayer>> CalculateAsync(List<RankingMatch> matches, IReadOnlyDictionary<long, int> punishments, CancellationToken cancellationToken)
    {
        var settings = await settingsService.GetAsync<RankingSettings>(cancellationToken);

        return RankingCalculator.Calculate(matches, punishments, settings);
    }

    private async Task<T> CachedAsync<T>(string scope, Func<Task<T>> factory)
    {
        var key = $"ranking_{scope}_{hubState.DataVersion}";

        var value = await memoryCache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(AppConsts.RankingCacheInMinutes);

            return await factory();
        });

        return value!;
    }
}
