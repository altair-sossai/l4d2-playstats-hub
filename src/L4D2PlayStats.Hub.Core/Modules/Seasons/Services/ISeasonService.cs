namespace L4D2PlayStats.Hub.Core.Modules.Seasons.Services;

public interface ISeasonService
{
    Task<List<Season>> ListAsync(CancellationToken cancellationToken = default);
    Task<Season?> FindAsync(int id, CancellationToken cancellationToken = default);
    Task<Season> GetOrCreateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<Season> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<DateTime> SiteNowAsync(CancellationToken cancellationToken = default);
}
