using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Servers;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public interface IRemoteCommandService
{
    Task<RemoteCommandResult> QueueAsync(GameServer server, long administratorId, string? administratorName, string? command, CancellationToken cancellationToken = default);
    Task<int> QueueInternalAsync(long administratorId, string? administratorName, string command, CancellationToken cancellationToken = default);
    Task<ServerCommand?> DequeueAsync(GameServer server, CancellationToken cancellationToken = default);
    Task<List<RemoteCommandLog>> ListAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountAsync(bool deliveredOnly = false, CancellationToken cancellationToken = default);
}
