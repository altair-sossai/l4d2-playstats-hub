namespace L4D2PlayStats.Hub.Core.Modules.Skins.Services;

public interface ISkinService
{
    Task<List<Skin>> ListAsync(CancellationToken cancellationToken = default);
    Task<Skin?> FindAsync(string? skinId, CancellationToken cancellationToken = default);
    Task<bool> SetPriceAsync(string? skinId, int? price, long? updatedBy, CancellationToken cancellationToken = default);
}
