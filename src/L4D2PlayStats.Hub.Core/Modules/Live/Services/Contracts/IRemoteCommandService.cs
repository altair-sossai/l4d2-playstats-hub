using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Servers;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;

public interface IRemoteCommandService
{
    Task<RemoteCommandResult> QueueAsync(GameServer server, long administratorId, string? administratorName, string? command, CancellationToken cancellationToken = default);
    Task<int> QueueInternalAsync(long administratorId, string? administratorName, string command, CancellationToken cancellationToken = default);
    Task<ServerCommand?> DequeueAsync(GameServer server, CancellationToken cancellationToken = default);
    Task<Page<RemoteCommandLog>> PageAsync(int page, int size, CancellationToken cancellationToken = default);
    Task<int> DeliveredCountAsync(CancellationToken cancellationToken = default);
}
