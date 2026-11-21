using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;

public interface IChatBanService
{
    Task<ChatBan?> FindAsync(long communityId, CancellationToken cancellationToken = default);
    Task<List<ChatBan>> ListAsync(CancellationToken cancellationToken = default);
    Task<Page<ChatBan>> PageAsync(PlayerSearch search, int page, int size, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<HashSet<long>> BannedAsync(IReadOnlyCollection<long> communityIds, CancellationToken cancellationToken = default);
    Task<ChatBan?> SaveAsync(long communityId, string? name, string? reason, long? createdBy, string? createdByName, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(long communityId, CancellationToken cancellationToken = default);
}
