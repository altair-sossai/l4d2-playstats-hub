using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Matches.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Matches.Models;
using L4D2PlayStats.Hub.Core.Modules.Matches.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using ParsedStatistics = L4D2PlayStats.Statistics;

namespace L4D2PlayStats.Hub.Core.Modules.Matches.Services;

public class MatchResultService(HubDbContext dbContext, IMatchQueryService matchQueryService, IDisplayNameStore displayNames) : IMatchResultService
{
    public async Task<Page<MatchResult>> ListAsync(MatchFilter filter, CancellationToken cancellationToken = default)
    {
        var page = await matchQueryService.ListAsync(filter, cancellationToken);
        var ids = page.Items.Select(match => match.Id).ToList();

        var files = await dbContext.StatisticsFiles
            .AsNoTracking()
            .Where(file => file.MatchId != null && ids.Contains(file.MatchId.Value))
            .Select(file => new { file.Id, MatchId = file.MatchId!.Value, file.MatchOrder, file.FileDate, file.MapName, file.TeamAScore, file.TeamBScore })
            .ToListAsync(cancellationToken);

        var newest = await dbContext.StatisticsFiles
            .AsNoTracking()
            .Where(file => file.MatchId != null && ids.Contains(file.MatchId.Value) && file.FileDate == file.Match!.LastFileDate)
            .Select(file => new { MatchId = file.MatchId!.Value, file.Content })
            .ToListAsync(cancellationToken);

        var maps = files
            .GroupBy(file => file.MatchId)
            .ToDictionary(group => group.Key, group => group
                .OrderBy(file => file.MatchOrder)
                .ThenBy(file => file.FileDate)
                .Select(file => new MatchMapResult(file.Id, file.MapName, file.TeamAScore, file.TeamBScore))
                .ToList());

        var contents = newest
            .GroupBy(file => file.MatchId)
            .ToDictionary(group => group.Key, group => group.First().Content);

        return page.Map(match => MatchResultFactory.Create(match, maps.GetValueOrDefault(match.Id) ?? [], Parse(contents.GetValueOrDefault(match.Id)), displayNames));
    }

    public async Task<MatchResult?> FindAsync(int id, CancellationToken cancellationToken = default)
    {
        var match = await matchQueryService.FindAsync(id, cancellationToken);
        if (match == null)
            return null;

        var files = match.Files
            .OrderBy(file => file.MatchOrder)
            .ThenBy(file => file.FileDate)
            .ToList();

        var maps = files
            .Select(file => new MatchMapResult(file.Id, file.MapName, file.TeamAScore, file.TeamBScore))
            .ToList();

        return MatchResultFactory.Create(match, maps, Parse(files.LastOrDefault()?.Content), displayNames);
    }

    public async Task<IReadOnlyList<MatchStatisticsResult>> StatisticsAsync(MatchResult match, CancellationToken cancellationToken = default)
    {
        var files = await dbContext.StatisticsFiles
            .AsNoTracking()
            .Where(file => file.GameServerId == match.GameServerId && file.FileDate >= match.FirstFileDate && file.FileDate <= match.LastFileDate)
            .OrderBy(file => file.FileDate)
            .ThenBy(file => file.Id)
            .ToListAsync(cancellationToken);

        return [.. files.Select(file => new MatchStatisticsResult(file.Id, file.FileName, Parse(file.Content)))];
    }

    public async Task<int?> FindIdAsync(DateTime firstFileDate, DateTime lastFileDate, CancellationToken cancellationToken = default)
    {
        var matches = await dbContext.Matches
            .AsNoTracking()
            .Where(match => match.LastFileDate == lastFileDate || match.FirstFileDate == firstFileDate)
            .Select(match => new { match.Id, match.FirstFileDate, match.LastFileDate })
            .ToListAsync(cancellationToken);

        var found = matches
            .OrderByDescending(match => match.FirstFileDate == firstFileDate && match.LastFileDate == lastFileDate)
            .ThenBy(match => match.Id)
            .FirstOrDefault();

        return found?.Id;
    }

    public async Task<int?> FindFileIdAsync(int matchId, DateTime fileDate, CancellationToken cancellationToken = default)
    {
        var match = await dbContext.Matches
            .AsNoTracking()
            .Where(m => m.Id == matchId)
            .Select(m => new { m.GameServerId, m.FirstFileDate, m.LastFileDate })
            .FirstOrDefaultAsync(cancellationToken);

        if (match == null || fileDate < match.FirstFileDate || fileDate > match.LastFileDate)
            return null;

        var ids = await dbContext.StatisticsFiles
            .AsNoTracking()
            .Where(file => file.GameServerId == match.GameServerId && file.FileDate == fileDate)
            .OrderBy(file => file.Id)
            .Select(file => file.Id)
            .Take(1)
            .ToListAsync(cancellationToken);

        return ids.Count == 0 ? null : ids[0];
    }

    private static ParsedStatistics? Parse(string? content)
    {
        return ParsedStatistics.TryParse(content, out var statistics) ? statistics : null;
    }
}
