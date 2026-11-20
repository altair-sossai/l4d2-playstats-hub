using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;
using L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class SkinDisplay
{
    public static readonly SkinRarity[] Rarities = [SkinRarity.Legendary, SkinRarity.Epic, SkinRarity.Rare, SkinRarity.Uncommon, SkinRarity.Common];

    public static string RarityKey(SkinRarity rarity)
    {
        return $"Rarity.{rarity}";
    }

    public static string RarityName(SkinRarity rarity)
    {
        return rarity.ToString().ToLowerInvariant();
    }

    public static string RarityCss(SkinRarity rarity)
    {
        return $"{AppConsts.SkinRarityCssPrefix}{RarityName(rarity)}";
    }

    public static string PickerCss(SkinRarity rarity)
    {
        return $"{AppConsts.SkinPickerRarityCssPrefix}{RarityName(rarity)}";
    }

    public static string PurchaseCss(SkinRarity rarity)
    {
        return $"{AppConsts.SkinPurchaseRarityCssPrefix}{RarityName(rarity)}";
    }

    public static string? Shots(string? assetsUrl, Skin skin)
    {
        return skin.HasShots ? SkinAssets.ShotsFolder(assetsUrl, skin.Id) : null;
    }

    public static string? WorkshopUrl(Skin skin)
    {
        return string.IsNullOrEmpty(skin.WorkshopId) ? null : $"{CoreConsts.SkinWorkshopUrl}{Uri.EscapeDataString(skin.WorkshopId)}";
    }

    public static string? AuthorUrl(Skin skin)
    {
        return skin.AuthorCommunityId == null ? null : SteamIds.ProfileUrl(skin.AuthorCommunityId.Value);
    }

    public static string Anchor(Skin skin)
    {
        return $"{AppConsts.SkinAnchorPrefix}{skin.Id}";
    }

    public static object[] Arguments(string values)
    {
        return [.. values.Split(AppConsts.SkinTempDataSeparator)];
    }

    public static string CatalogMessage(SkinCatalogImportStatus status)
    {
        return status switch
        {
            SkinCatalogImportStatus.InvalidFile => AppConsts.SkinCatalogInvalidFile,
            SkinCatalogImportStatus.EmptyCatalog => AppConsts.SkinCatalogEmpty,
            SkinCatalogImportStatus.UrlNotConfigured => AppConsts.SkinCatalogUrlNotConfigured,
            _ => AppConsts.SkinCatalogDownloadFailed
        };
    }
}
