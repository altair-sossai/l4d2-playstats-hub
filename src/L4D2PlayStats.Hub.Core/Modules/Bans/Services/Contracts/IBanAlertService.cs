using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Bans.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Bans.Services.Contracts;

public interface IBanAlertService
{
    Task<int> DetectAsync(long communityId, CancellationToken cancellationToken = default);
    Task<int> LinkedAsync(Ban ban, CancellationToken cancellationToken = default);
    Task<Page<BanAlertEntry>> PageAsync(int page, int size, CancellationToken cancellationToken = default);
    Task<int> OpenCountAsync(CancellationToken cancellationToken = default);
    Task<BanAlertEntry?> FindAsync(int alertId, CancellationToken cancellationToken = default);
    Task<bool> IgnoreAsync(int alertId, long? ignoredBy, CancellationToken cancellationToken = default);
    Task<int> ResolveAsync(long communityId, long? bannedBy, CancellationToken cancellationToken = default);
}
