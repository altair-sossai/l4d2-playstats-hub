using L4D2PlayStats.Hub.Core.Modules.Skins.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;

public interface ISkinCatalogService
{
    Task<SkinCatalogImportResult> ImportAsync(string? json, CancellationToken cancellationToken = default);
    Task<SkinCatalogImportResult> ImportFromUrlAsync(CancellationToken cancellationToken = default);
    Task<SkinCatalogImportResult> ImportBundledAsync(CancellationToken cancellationToken = default);
    Task<SkinCatalogImportResult?> SeedAsync(CancellationToken cancellationToken = default);
}
