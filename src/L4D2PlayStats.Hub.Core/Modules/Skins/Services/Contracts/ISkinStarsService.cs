using L4D2PlayStats.Hub.Core.Modules.Skins.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;

public interface ISkinStarsService
{
    Task<SkinStars> GetAsync(long communityId, CancellationToken cancellationToken = default);
}