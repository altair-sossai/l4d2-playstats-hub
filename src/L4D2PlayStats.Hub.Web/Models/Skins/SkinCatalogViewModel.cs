using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;

namespace L4D2PlayStats.Hub.Web.Models.Skins;

public class SkinCatalogViewModel
{
    public SkinShopSettings Settings { get; init; } = new();
    public int Total { get; init; }
    public int Available { get; init; }
    public int Unavailable { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}
