using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;

namespace L4D2PlayStats.Hub.Web.Models.Skins;

public class SkinPricesViewModel
{
    public PagedList<Skin> Skins { get; init; } = null!;
    public IReadOnlyList<string> Weapons { get; init; } = [];
    public string? Query { get; init; }
    public string? Weapon { get; init; }
    public SkinRarity? Rarity { get; init; }
    public int CustomPrices { get; init; }
    public string ReturnUrl { get; init; } = AppConsts.SkinPricesUrl;
}
