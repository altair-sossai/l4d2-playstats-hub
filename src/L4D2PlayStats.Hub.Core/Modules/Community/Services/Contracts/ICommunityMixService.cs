using L4D2PlayStats.Hub.Core.Modules.Community.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;

public interface ICommunityMixService
{
    Task<CommunityMixResult> BalanceAsync(IReadOnlyList<long> communityIds, CancellationToken cancellationToken = default);
}
