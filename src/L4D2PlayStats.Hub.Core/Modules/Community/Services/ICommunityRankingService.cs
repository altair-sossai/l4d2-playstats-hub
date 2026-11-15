using L4D2PlayStats.Hub.Core.Modules.Community.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Services;

public interface ICommunityRankingService
{
    Task<IReadOnlyList<CommunityRankingEntry>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommunityVoter>> VotersAsync(CancellationToken cancellationToken = default);
    Task<int> RebuildAsync(CancellationToken cancellationToken = default);
    Task<int> RemoveVoterAsync(long voterId, CancellationToken cancellationToken = default);
}
