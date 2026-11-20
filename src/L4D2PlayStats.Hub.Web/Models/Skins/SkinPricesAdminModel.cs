using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;

namespace L4D2PlayStats.Hub.Web.Models.Skins;

public class SkinPricesAdminModel(PagedListModel<Skin> skins, IReadOnlyCollection<Skin> allSkins, IReadOnlyList<string> weapons, string? assetsUrl, string? query, SkinRarity? rarity, string? weapon, int customPrices)
{
    public PagedListModel<Skin> Skins { get; } = skins;
    public IReadOnlyCollection<Skin> AllSkins { get; } = allSkins;
    public IReadOnlyList<string> Weapons { get; } = weapons;
    public string? AssetsUrl { get; } = assetsUrl;
    public string? Query { get; } = query;
    public SkinRarity? Rarity { get; } = rarity;
    public string? Weapon { get; } = weapon;
    public int CustomPrices { get; } = customPrices;
}