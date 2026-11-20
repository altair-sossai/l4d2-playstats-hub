using L4D2PlayStats.Hub.Core.Modules.Skins;

namespace L4D2PlayStats.Hub.Web.Models.Skins;

public class SkinPurchasesAdminModel
{
    public required PagedListModel<SkinPurchaseRowModel> Purchases { get; init; }
    public string? Query { get; init; }
    public string? Player { get; init; }
    public string? PlayerName { get; init; }
    public required IReadOnlyCollection<Skin> Skins { get; init; }
    public string? AssetsUrl { get; init; }
    public required SkinSeasonModel Season { get; init; }
    public int TotalPurchases { get; init; }
    public int TotalStars { get; init; }
    public int TotalGifts { get; init; }
}