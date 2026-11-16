using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services;

public class PlayerMatchService(HubDbContext dbContext) : IPlayerMatchService
{
    public async Task<List<PlayerMatch>> LastAsync(int seasonId, IReadOnlyCollection<long> communityIds, int recentMatches, CancellationToken cancellationToken = default)
    {
        if (communityIds.Count == 0 || recentMatches <= 0)
            return [];

        var recent = await dbContext.Matches
            .AsNoTracking()
            .Where(m => m.SeasonId == seasonId)
            .Include(m => m.GameServer)
            .Include(m => m.Players)
            .OrderByDescending(m => m.LastFileDate)
            .ThenByDescending(m => m.Id)
            .Take(recentMatches)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var matches = recent
            .Where(m => m.Players.Any(p => (p.TeamA || p.TeamB) && communityIds.Contains(p.CommunityId)))
            .ToList();

        if (matches.Count == 0)
            return [];

        var matchIds = matches.Select(m => m.Id).ToList();

        var files = await dbContext.StatisticsFiles
            .AsNoTracking()
            .Where(f => f.MatchId != null && matchIds.Contains(f.MatchId.Value))
            .Select(f => new { f.MatchId, f.MatchOrder, f.FileDate, f.MapName, f.TeamAScore, f.TeamBScore })
            .ToListAsync(cancellationToken);

        var maps = files
            .GroupBy(f => f.MatchId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(f => f.MatchOrder)
                    .ThenBy(f => f.FileDate)
                    .Select(f => new PlayerMatchMap(f.MapName, f.TeamAScore, f.TeamBScore))
                    .ToList());

        return [.. matches.Select(m => new PlayerMatch(m, maps.GetValueOrDefault(m.Id) ?? []))];
    }
}
