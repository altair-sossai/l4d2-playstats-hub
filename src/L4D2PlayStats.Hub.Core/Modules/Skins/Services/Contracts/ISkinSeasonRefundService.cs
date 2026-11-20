using L4D2PlayStats.Hub.Core.Modules.Skins.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;

public interface ISkinSeasonRefundService
{
    Task<SkinRefundSummary> RunAsync(CancellationToken cancellationToken = default);
}