using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;

public interface ICommunityModerationService
{
    Task<bool> IsBlockedAsync(long voterId, CancellationToken cancellationToken = default);
    Task<Page<CommunityBlockedVoter>> BlockedAsync(int page, int size, CancellationToken cancellationToken = default);
    Task<bool> BlockAsync(long voterId, long? blockedBy, CancellationToken cancellationToken = default);
    Task<bool> UnblockAsync(long voterId, CancellationToken cancellationToken = default);
    Task<bool> RemoveVoteAsync(int voteId, CancellationToken cancellationToken = default);
}
