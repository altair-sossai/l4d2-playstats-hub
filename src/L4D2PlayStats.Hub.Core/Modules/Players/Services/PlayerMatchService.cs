using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Matches.Extensions;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services;

public class PlayerMatchService(HubDbContext dbContext) : IPlayerMatchService
{
    public async Task<List<int>> RecentMatchIdsAsync(int? seasonId, int? year, IReadOnlyCollection<long> communityIds, int recentMatches, CancellationToken cancellationToken = default)
    {
        if (communityIds.Count == 0 || recentMatches <= 0)
            return [];

        var ids = communityIds.ToList();

        var recent = await dbContext.Matches
            .AsNoTracking()
            .InPeriod(seasonId, year)
            .NewestFirst()
            .Take(recentMatches)
            .Select(m => new { m.Id, Present = m.Players.Any(p => (p.TeamA || p.TeamB) && ids.Contains(p.CommunityId)) })
            .ToListAsync(cancellationToken);

        return [.. recent.Where(m => m.Present).Select(m => m.Id)];
    }
}
