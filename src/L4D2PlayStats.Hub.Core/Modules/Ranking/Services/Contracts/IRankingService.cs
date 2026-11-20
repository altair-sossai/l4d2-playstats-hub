using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;

public interface IRankingService
{
    Task<IReadOnlyList<RankingPlayer>> SeasonAsync(int seasonId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RankingPlayer>> CurrentAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RankingPlayer>> YearAsync(int year, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RankingPlayer>> AllTimeAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RankingPlayer>> MatchAsync(int matchId, CancellationToken cancellationToken = default);
}
