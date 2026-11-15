using L4D2PlayStats.Hub.Core.Modules.Live.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public interface IPlayerConnectionService
{
    Task<List<PlayerConnectionResult>?> RegisterAsync(PlayerConnectionCommand? command, CancellationToken cancellationToken = default);
    Task<List<PlayerConnectionResult>> RelatedAsync(long communityId, CancellationToken cancellationToken = default);
    Task<PlayerConnectionDetails> DetailsAsync(long communityId, CancellationToken cancellationToken = default);
    Task<int> CleanupAsync(CancellationToken cancellationToken = default);
}
