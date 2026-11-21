using L4D2PlayStats.Hub.Web.Models.Ranking;

namespace L4D2PlayStats.Hub.Web.Services.Contracts;

public interface IRankingHistoryService
{
    Task<HistoryModel?> LastAsync(CancellationToken cancellationToken = default);
    Task<string?> PlayersPeriodAsync(string? historyId, CancellationToken cancellationToken = default);
}
