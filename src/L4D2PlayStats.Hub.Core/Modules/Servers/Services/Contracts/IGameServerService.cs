using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Servers.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;

public interface IGameServerService
{
    Task<List<GameServer>> ListAsync(CancellationToken cancellationToken = default);
    Task<Page<GameServer>> PageAsync(int page, int size, CancellationToken cancellationToken = default);
    Task<GameServer?> FindAsync(int id, CancellationToken cancellationToken = default);
    Task<GameServer?> FindAsync(string publicId, CancellationToken cancellationToken = default);
    Task<GameServerCredentials> CreateAsync(string name, string? address, CancellationToken cancellationToken = default);
    Task<GameServer?> UpdateAsync(int id, string name, string? address, bool enabled, CancellationToken cancellationToken = default);
    Task<GameServer?> SetAlternativeAddressAsync(int id, string? alternativeAddress, CancellationToken cancellationToken = default);
    Task<GameServerCredentials?> RegenerateKeyAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<GameServerAuthentication> AuthenticateAsync(string? token, CancellationToken cancellationToken = default);
    Task SignalAsync(GameServer server, GameServerSignal signal, CancellationToken cancellationToken = default);
}
