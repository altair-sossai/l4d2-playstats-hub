using L4D2PlayStats.Hub.Core.Modules.Skins.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Services;

public interface ISkinSeasonRefundService
{
    Task<SkinRefundSummary> RunAsync(CancellationToken cancellationToken = default);
}
