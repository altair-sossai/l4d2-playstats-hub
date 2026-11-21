using L4D2PlayStats.Hub.Core.Modules.Community.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;

public interface ICommunityModerationService
{
    Task<bool> IsBlockedAsync(long voterId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommunityBlockedVoter>> BlockedAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task<int> BlockedCountAsync(CancellationToken cancellationToken = default);
    Task<bool> BlockAsync(long voterId, long? blockedBy, CancellationToken cancellationToken = default);
    Task<bool> UnblockAsync(long voterId, CancellationToken cancellationToken = default);
    Task<bool> RemoveVoteAsync(int voteId, CancellationToken cancellationToken = default);
}
