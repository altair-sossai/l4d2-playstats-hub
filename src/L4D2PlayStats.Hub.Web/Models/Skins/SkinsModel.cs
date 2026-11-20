using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;
using L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Skins.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Skins;

namespace L4D2PlayStats.Hub.Web.Models.Skins;

public class SkinsModel(IReadOnlyCollection<Skin> catalog, bool loggedIn, SkinWallet? wallet, PlayerSkins? playerSkins, SkinShopSettings settings, SkinSeasonModel season, SkinThumbnailStore thumbnails)
{
    public IReadOnlyCollection<Skin> Skins { get; } = [.. catalog.Where(skin => skin.Available)];
    public bool LoggedIn { get; } = loggedIn;
    public SkinWallet? Wallet { get; } = wallet;
    public bool WalletUnavailable => LoggedIn && Wallet == null;
    public PlayerSkins PlayerSkins { get; } = playerSkins ?? new PlayerSkins();
    public SkinShopSettings Settings { get; } = settings;
    public SkinSeasonModel Season { get; } = season;

    public IReadOnlyList<string> Weapons { get; } = SkinWeapons.Sort(SkinWeapons.All.Concat(catalog.Select(skin => skin.Weapon)));

    public IReadOnlyCollection<Skin> OwnedSkins { get; } =
    [
        .. catalog
            .Where(skin => (playerSkins ?? new PlayerSkins()).Owns(skin.Id))
            .OrderBy(skin => SkinWeapons.Order(skin.Weapon))
            .ThenByDescending(skin => skin.Rarity)
            .ThenByDescending(skin => skin.Price)
    ];

    public IReadOnlyCollection<Skin> SkinsOf(SkinRarity rarity)
    {
        return
        [
            .. Skins
                .Where(skin => skin.Rarity == rarity)
                .OrderByDescending(skin => skin.Price)
                .ThenBy(skin => SkinWeapons.Order(skin.Weapon))
                .ThenBy(skin => skin.Title(CultureInfo.InvariantCulture))
        ];
    }

    public IReadOnlyList<SkinAuthorModel> Authors(CultureInfo culture)
    {
        return
        [
            .. Skins
                .Where(skin => !string.IsNullOrEmpty(skin.AuthorName))
                .GroupBy(skin => skin.AuthorCommunityId?.ToString(CultureInfo.InvariantCulture) ?? skin.AuthorName!)
                .Select(group => new SkinAuthorModel(
                    group.First().AuthorName!,
                    SkinDisplay.AuthorUrl(group.First()),
                    [.. group.OrderBy(skin => skin.Title(culture), StringComparer.CurrentCultureIgnoreCase)]))
                .OrderBy(author => author.Name, StringComparer.CurrentCultureIgnoreCase)
        ];
    }

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

    public string Image(Skin skin)
    {
        return thumbnails.Image(Settings.AssetsUrl, skin);
    }

    public string? Shots(Skin skin)
    {
        return SkinDisplay.Shots(Settings.AssetsUrl, skin);
    }
}