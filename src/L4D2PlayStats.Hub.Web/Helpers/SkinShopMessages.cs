using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class SkinShopMessages
{
    public static string For(SkinShopStatus status)
    {
        return status switch
        {
            SkinShopStatus.SkinNotFound => "That skin is not available.",
            SkinShopStatus.AlreadyOwned => "That skin is already owned.",
            SkinShopStatus.NotOwned => "That skin is not owned.",
            SkinShopStatus.InsufficientStars => "There are not enough stars for that skin.",
            SkinShopStatus.StarsUnavailable => "The stars of that player could not be loaded.",
            SkinShopStatus.Busy => "The shop is busy. Try again in a moment.",
            _ => "Done."
        };
    }

    public static string For(SkinCatalogImportStatus status)
    {
        return status switch
        {
            SkinCatalogImportStatus.InvalidFile => "That file is not a valid skin catalog.",
            SkinCatalogImportStatus.EmptyCatalog => "That catalog has no skins.",
            SkinCatalogImportStatus.UrlNotConfigured => "Set the catalog address in the shop settings first.",
            SkinCatalogImportStatus.DownloadFailed => "The catalog could not be downloaded.",
            _ => "Done."
        };
    }
}
