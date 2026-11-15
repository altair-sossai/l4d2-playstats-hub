using L4D2PlayStats.Hub.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Matches;

public class MatchFilter
{
    public int? SeasonId { get; set; }
    public int? GameServerId { get; set; }
    public long? CommunityId { get; set; }
    public bool RankingOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 30;
}

public record MatchPage(IReadOnlyList<Match> Matches, int Total, int Page, int PageSize)
{
    public int Pages => Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
}

public interface IMatchQueryService
{
    Task<MatchPage> ListAsync(MatchFilter filter, CancellationToken cancellationToken = default);
    Task<Match?> FindAsync(int id, CancellationToken cancellationToken = default);
    Task<Match?> LastAsync(CancellationToken cancellationToken = default);
}

public class MatchQueryService(HubDbContext dbContext) : IMatchQueryService
{
    public async Task<MatchPage> ListAsync(MatchFilter filter, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Matches.AsNoTracking();

        if (filter.SeasonId != null)
            query = query.Where(m => m.SeasonId == filter.SeasonId);

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
            .OrderByDescending(m => m.LastFileDate)
            .ThenByDescending(m => m.Id)
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
            .OrderByDescending(m => m.LastFileDate)
            .ThenByDescending(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
