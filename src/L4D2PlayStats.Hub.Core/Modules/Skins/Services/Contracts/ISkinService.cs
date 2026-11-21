using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;

public interface ISkinService
{
    Task<List<Skin>> ListAsync(CancellationToken cancellationToken = default);
    Task<Page<Skin>> PageAsync(SkinRarity? rarity, string? weapon, IReadOnlyCollection<string>? ids, int page, int size, CancellationToken cancellationToken = default);
    Task<Skin?> FindAsync(string? skinId, CancellationToken cancellationToken = default);
    Task<bool> SetPriceAsync(string? skinId, int? price, long? updatedBy, CancellationToken cancellationToken = default);
}
