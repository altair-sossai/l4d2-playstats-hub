using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Models;

public class SkinCatalogImportResult
{
    public SkinCatalogImportStatus Status { get; init; }
    public int Added { get; init; }
    public int Updated { get; init; }
    public int Removed { get; init; }
    public int Total { get; init; }
    public bool Succeeded => Status == SkinCatalogImportStatus.Success;

    public static SkinCatalogImportResult Failed(SkinCatalogImportStatus status)
    {
        return new SkinCatalogImportResult { Status = status };
    }
}
