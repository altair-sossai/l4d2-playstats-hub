using L4D2PlayStats.Hub.Core.Modules.Skins.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;

namespace L4D2PlayStats.Hub.Web.Models.Skins;

public class SkinCatalogViewModel
{
    public SkinShopSettings Settings { get; init; } = new();
    public int Total { get; init; }
    public int Available { get; init; }
    public int Unavailable { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public SkinAuthorStatus Authors { get; init; } = new(0, 0, null, null);
    public DateTime? AuthorsSyncedAt { get; init; }
    public DateTime? AuthorRenamedAt { get; init; }
}
