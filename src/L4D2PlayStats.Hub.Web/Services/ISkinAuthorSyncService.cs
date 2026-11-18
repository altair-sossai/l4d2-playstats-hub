using L4D2PlayStats.Hub.Web.Models.Skins;

namespace L4D2PlayStats.Hub.Web.Services;

public interface ISkinAuthorSyncService
{
    Task<SkinAuthorSyncResult> SyncAsync(bool everyAuthor, CancellationToken cancellationToken = default);
}
