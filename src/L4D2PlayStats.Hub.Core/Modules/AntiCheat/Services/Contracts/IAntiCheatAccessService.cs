using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Enums;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;

namespace L4D2PlayStats.Hub.Core.Modules.AntiCheat.Services.Contracts;

public interface IAntiCheatAccessService
{
    Task<AntiCheatAccessSnapshot> GetAsync(CancellationToken cancellationToken = default);
    Task<AntiCheatAccessSnapshot> ModeAsync(CancellationToken cancellationToken = default);
    Task<Page<AntiCheatAccessPlayer>> PageAsync(AntiCheatAccessList list, PlayerSearch search, int page, int size, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<long>> IdsAsync(AntiCheatAccessList list, CancellationToken cancellationToken = default);
    Task SetModeAsync(AntiCheatAccessMode mode, AntiCheatAccessActor actor, AntiCheatAccessSource source, CancellationToken cancellationToken = default);
    Task<AntiCheatAccessAddResult?> AddAsync(AntiCheatAccessList list, long communityId, string? name, AntiCheatAccessActor actor, AntiCheatAccessSource source, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(AntiCheatAccessList list, long communityId, AntiCheatAccessActor actor, AntiCheatAccessSource source, CancellationToken cancellationToken = default);
}
