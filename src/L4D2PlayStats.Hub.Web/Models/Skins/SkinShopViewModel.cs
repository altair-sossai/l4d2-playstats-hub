using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;
using L4D2PlayStats.Hub.Core.Modules.Skins.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;

namespace L4D2PlayStats.Hub.Web.Models.Skins;

public class SkinShopViewModel
{
    public IReadOnlyList<Skin> Skins { get; init; } = [];
    public IReadOnlyList<string> Weapons { get; init; } = [];
    public string? Weapon { get; init; }
    public SkinRarity? Rarity { get; init; }
    public bool OnlyOwned { get; init; }
    public bool SignedIn { get; init; }
    public SkinWallet? Wallet { get; init; }
    public PlayerSkins PlayerSkins { get; init; } = new();
    public SkinShopSettings Settings { get; init; } = new();
    public int Total { get; init; }
    public string ReturnUrl { get; init; } = AppConsts.SkinShopUrl;

    public bool Owns(Skin skin)
    {
        return PlayerSkins.Owns(skin.Id);
    }

    public bool IsEquipped(Skin skin)
    {
        return PlayerSkins.IsEquipped(skin);
    }

    public bool CanAfford(Skin skin)
    {
        return Wallet != null && Wallet.Balance >= skin.Price;
    }

    public int Missing(Skin skin)
    {
        return Math.Max(0, skin.Price - (Wallet?.Balance ?? 0));
    }
}
