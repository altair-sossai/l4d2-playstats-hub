using L4D2PlayStats.Hub.Core.Modules.Skins.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Services;

public interface ISkinCatalogService
{
    Task<SkinCatalogImportResult> ImportAsync(string? json, CancellationToken cancellationToken = default);
    Task<SkinCatalogImportResult> ImportFromUrlAsync(CancellationToken cancellationToken = default);
}
