namespace L4D2PlayStats.Hub.Core.Modules.Punishments.Services;

public interface IPunishmentService
{
    Task<Dictionary<long, int>> TotalsAsync(int seasonId, CancellationToken cancellationToken = default);
    Task<List<Punishment>> ListAsync(int seasonId, CancellationToken cancellationToken = default);
    Task<Punishment?> AddAsync(int seasonId, long communityId, int points, string? reason, long? createdBy, CancellationToken cancellationToken = default);
    Task<int> RemoveAsync(int seasonId, long communityId, CancellationToken cancellationToken = default);
}
