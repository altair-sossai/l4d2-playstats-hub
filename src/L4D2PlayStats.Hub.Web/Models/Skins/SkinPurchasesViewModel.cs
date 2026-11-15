using L4D2PlayStats.Hub.Core.Modules.Skins;

namespace L4D2PlayStats.Hub.Web.Models.Skins;

public class SkinPurchasesViewModel
{
    public PagedList<SkinPurchaseRow> Rows { get; init; } = null!;
    public IReadOnlyList<Skin> Skins { get; init; } = [];
    public string? Query { get; init; }
    public int Purchases { get; init; }
    public int Gifts { get; init; }
    public int Stars { get; init; }
    public int Players { get; init; }
}
