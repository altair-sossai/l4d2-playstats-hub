using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Bans.Enums;
using L4D2PlayStats.Hub.Core.Modules.Bans.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Bans.Services.Contracts;

public interface IBanService
{
    Task<Ban?> FindAsync(int banId, CancellationToken cancellationToken = default);
    Task<Ban?> ActiveAsync(long communityId, CancellationToken cancellationToken = default);
    Task<HashSet<long>> BannedAsync(IReadOnlyCollection<long> communityIds, CancellationToken cancellationToken = default);
    Task<Page<Ban>> PageAsync(PlayerSearch search, BanListFilter filter, int page, int size, CancellationToken cancellationToken = default);
    Task<int> CountAsync(BanListFilter filter, CancellationToken cancellationToken = default);
    Task<List<Ban>> HistoryAsync(long communityId, CancellationToken cancellationToken = default);
    Task<BanSaveResult> AddAsync(BanRequest request, BanActor actor, CancellationToken cancellationToken = default);
    Task<BanSaveResult> EditAsync(int banId, DateTime? endsAtUtc, string? reason, BanActor actor, CancellationToken cancellationToken = default);
    Task<bool> LiftAsync(int banId, string? reason, BanActor actor, CancellationToken cancellationToken = default);
    Task<Ban?> CaptureAsync(GameBanCommand? command, string? serverName, CancellationToken cancellationToken = default);
    Task<bool> CaptureLiftAsync(GameUnbanCommand? command, CancellationToken cancellationToken = default);
    Task<Ban?> AttemptAsync(long communityId, CancellationToken cancellationToken = default);
    Task<BanImport> ImportAsync(string? text, BanActor actor, CancellationToken cancellationToken = default);
}
