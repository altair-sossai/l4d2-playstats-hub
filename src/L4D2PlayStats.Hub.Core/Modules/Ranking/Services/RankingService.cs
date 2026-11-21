using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Matches.Extensions;
using L4D2PlayStats.Hub.Core.Modules.Punishments.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace L4D2PlayStats.Hub.Core.Modules.Ranking.Services;

public class RankingService(
    HubDbContext dbContext,
    HubState hubState,
    IMemoryCache memoryCache,
    ISeasonService seasonService,
    IPunishmentService punishmentService,
    ISettingsService settingsService,
    IForcedNameStore forcedNames) : IRankingService
{
    public Task<IReadOnlyList<RankingPlayer>> SeasonAsync(int seasonId, CancellationToken cancellationToken = default)
    {
        return CachedAsync($"season_{seasonId}", async () =>
        {
            var punishments = await punishmentService.TotalsAsync(seasonId, cancellationToken);
            var matches = await LoadAsync(dbContext.Matches.Where(m => m.SeasonId == seasonId), cancellationToken);

            return await CalculateAsync(matches, punishments, cancellationToken);
        }, cancellationToken);
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
            var matches = await LoadAsync(dbContext.Matches.InYear(year), cancellationToken);

            return await CalculateAsync(matches, new Dictionary<long, int>(), cancellationToken);
        }, cancellationToken);
    }

    public Task<IReadOnlyList<RankingPlayer>> AllTimeAsync(CancellationToken cancellationToken = default)
    {
        return CachedAsync("all_time", async () =>
        {
            var matches = await LoadAsync(dbContext.Matches, cancellationToken);

            return await CalculateAsync(matches, new Dictionary<long, int>(), cancellationToken);
        }, cancellationToken);
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
        var players = RankingCalculator.Calculate(matches, punishments, settings);

        foreach (var player in players)
            player.ForcedName = forcedNames.Find(player.CommunityId);

        return players;
    }

    private async Task<T> CachedAsync<T>(string scope, Func<Task<T>> factory, CancellationToken cancellationToken)
    {
        var changes = hubState.DataChangeToken;
        var key = $"ranking_{scope}_{hubState.DataVersion}";

        if (memoryCache.TryGetValue(key, out T? cached))
            return cached!;

        var gate = hubState.Gate($"ranking_{scope}");

        await gate.WaitAsync(cancellationToken);

        try
        {
            if (memoryCache.TryGetValue(key, out cached))
                return cached!;

            var value = await factory();
            var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(AppConsts.RankingCacheInMinutes) };

            memoryCache.Set(key, value, options.AddExpirationToken(changes));

            return value;
        }
        finally
        {
            gate.Release();
        }
    }
}
