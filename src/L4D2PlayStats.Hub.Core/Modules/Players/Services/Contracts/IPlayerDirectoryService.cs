using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;

public interface IPlayerDirectoryService
{
    Task<Page<PlayerDirectoryEntry>> SearchAsync(string? term, int page, CancellationToken cancellationToken = default);
}
