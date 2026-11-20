using L4D2PlayStats.Hub.Web.Models.Ranking;

namespace L4D2PlayStats.Hub.Web.Services.Contracts;

public interface IRankingHistoryService
{
    Task<IReadOnlyList<RankingPeriod>> ListAsync(CancellationToken cancellationToken = default);
    Task<HistoryModel?> FindAsync(string? historyId, CancellationToken cancellationToken = default);
    Task<HistoryModel?> LastAsync(CancellationToken cancellationToken = default);
    Task<string?> SeasonHistoryIdAsync(int seasonId, CancellationToken cancellationToken = default);
}
