using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.Skins.Models;
using L4D2PlayStats.Hub.Web.Infrastructure.Skins;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerSkinsModel(IReadOnlyCollection<Skin> skins, PlayerSkins playerSkins, SkinWallet? wallet, string? assetsUrl, SkinThumbnailStore thumbnails)
{
    public IReadOnlyCollection<Skin> OwnedSkins { get; } =
    [
        .. skins
            .Where(skin => playerSkins.Owns(skin.Id))
            .OrderByDescending(skin => skin.Rarity)
            .ThenByDescending(skin => skin.Price)
    ];

    public PlayerSkins PlayerSkins { get; } = playerSkins;
    public SkinWallet? Wallet { get; } = wallet;

    public bool IsEquipped(Skin skin)
    {
        return PlayerSkins.IsEquipped(skin);
    }

    public string? Image(Skin skin)
    {
        return thumbnails.Url(assetsUrl, skin.Id);
    }
}