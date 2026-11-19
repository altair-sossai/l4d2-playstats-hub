using L4D2PlayStats.Hub.Core.Modules.Players.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services;

public interface IPlayerDirectoryService
{
    Task<PlayerDirectoryPage> SearchAsync(string? term, int page, CancellationToken cancellationToken = default);
}
