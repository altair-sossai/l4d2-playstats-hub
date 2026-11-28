using L4D2PlayStats.Hub.Web.Models.Bans;

namespace L4D2PlayStats.Hub.Web.Services.Contracts;

public interface IBanViewService
{
    Task<PlayerBanModel?> PlayerAsync(long communityId, CancellationToken cancellationToken = default);
}
