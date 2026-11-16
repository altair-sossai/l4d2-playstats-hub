using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Skins.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Services;

public class SkinStarsService(HubDbContext dbContext, HubState hubState, IMemoryCache memoryCache) : ISkinStarsService
{
    public async Task<SkinStars> GetAsync(long communityId, CancellationToken cancellationToken = default)
    {
        var stars = await memoryCache.GetOrCreateAsync($"{AppConsts.SkinStarsCacheKey}_{hubState.DataVersion}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(AppConsts.RankingCacheInMinutes);

            return await LoadAsync(cancellationToken);
        });

        return stars!.GetValueOrDefault(communityId, SkinStars.None);
    }

    private async Task<Dictionary<long, SkinStars>> LoadAsync(CancellationToken cancellationToken)
    {
        var rows = await dbContext.MatchPlayers
            .AsNoTracking()
            .Where(p => p.Match!.CountsForRanking && (p.TeamA || p.TeamB))
            .Select(p => new { p.CommunityId, p.TeamA, p.TeamB, p.MvpSiDamage, p.MvpCommon })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.CommunityId)
            .ToDictionary(
                group => group.Key,
                group => new SkinStars(
                    group.Sum(row => row.MvpSiDamage * Teams(row.TeamA, row.TeamB)),
                    group.Sum(row => row.MvpCommon * Teams(row.TeamA, row.TeamB))));
    }

    private static int Teams(bool teamA, bool teamB)
    {
        return (teamA ? 1 : 0) + (teamB ? 1 : 0);
    }
}
