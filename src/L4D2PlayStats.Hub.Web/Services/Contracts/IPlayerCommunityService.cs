using L4D2PlayStats.Hub.Web.Models.Players;

namespace L4D2PlayStats.Hub.Web.Services.Contracts;

public interface IPlayerCommunityService
{
    Task<PlayerCommunityModel?> BuildAsync(long communityId, long? compareWith, CancellationToken cancellationToken = default);
}
