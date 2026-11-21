using L4D2PlayStats.Hub.Core.Modules.Players.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;

public interface IPlayerRegistryService
{
    Task<PlayerRegistryBackfill> BackfillAsync(CancellationToken cancellationToken = default);
    Task<bool> RefreshAsync(IReadOnlyCollection<long> communityIds, CancellationToken cancellationToken = default);
    Task<bool> SeenAsync(long communityId, string? name, CancellationToken cancellationToken = default);
    void Publish(IEnumerable<PlayerProfile> profiles);
}
