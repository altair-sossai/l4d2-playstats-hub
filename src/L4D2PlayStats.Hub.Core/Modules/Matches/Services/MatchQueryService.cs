using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Matches.Extensions;
using L4D2PlayStats.Hub.Core.Modules.Matches.Models;
using L4D2PlayStats.Hub.Core.Modules.Matches.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Matches.Services;

public class MatchQueryService(HubDbContext dbContext) : IMatchQueryService
{
    public async Task<MatchPage> ListAsync(MatchFilter filter, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Matches.AsNoTracking().InPeriod(filter.SeasonId, filter.Year);

        if (filter.Ids != null)
        {
            var ids = filter.Ids.ToList();

            query = query.Where(m => ids.Contains(m.Id));
        }

        if (filter.GameServerId != null)
            query = query.Where(m => m.GameServerId == filter.GameServerId);

        if (filter.RankingOnly)
            query = query.Where(m => m.CountsForRanking);

        if (filter.CommunityId != null)
            query = query.Where(m => m.Players.Any(p => p.CommunityId == filter.CommunityId && (p.TeamA || p.TeamB || p.Win || p.Loss || p.Draws > 0 || p.RageQuits > 0)));

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);
        var total = await query.CountAsync(cancellationToken);

        var matches = await query
            .Include(m => m.GameServer)
            .Include(m => m.Players)
            .NewestFirst()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return new MatchPage(matches, total, page, pageSize);
    }

    public Task<Match?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Matches
            .AsNoTracking()
            .Include(m => m.GameServer)
            .Include(m => m.Season)
            .Include(m => m.Players)
            .Include(m => m.Files)
            .AsSplitQuery()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public Task<Match?> LastAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Matches
            .AsNoTracking()
            .Where(m => m.CountsForRanking)
            .Include(m => m.Players)
            .NewestFirst()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
