using L4D2PlayStats.Hub.Web.Models.Ranking;

namespace L4D2PlayStats.Hub.Web.Services.Contracts;

public interface IRankingHistoryService
{
    Task<HistoryModel?> LastAsync(CancellationToken cancellationToken = default);
    Task<string?> PlayersPeriodAsync(string historyId, CancellationToken cancellationToken = default);
    Task<string?> SeasonPeriodAsync(int seasonId, CancellationToken cancellationToken = default);
    Task<string?> YearPeriodAsync(int year, CancellationToken cancellationToken = default);
}
