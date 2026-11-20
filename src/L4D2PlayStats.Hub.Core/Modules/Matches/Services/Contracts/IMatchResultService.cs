using L4D2PlayStats.Hub.Core.Modules.Matches.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Matches.Services.Contracts;

public interface IMatchResultService
{
    Task<MatchResultPage> ListAsync(MatchFilter filter, CancellationToken cancellationToken = default);
    Task<MatchResult?> FindAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MatchStatisticsResult>> StatisticsAsync(MatchResult match, CancellationToken cancellationToken = default);
    Task<int?> FindIdAsync(DateTime firstFileDate, DateTime lastFileDate, CancellationToken cancellationToken = default);
    Task<int?> FindFileIdAsync(int matchId, DateTime fileDate, CancellationToken cancellationToken = default);
}