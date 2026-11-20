namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const int SkinListPageSize = 20;
    public const string SkinCatalogUrl = "/admin/skins/catalog";
    public const string SkinSettingsUrl = "/admin/skins/settings";
    public const string SkinBundledThumbnailsPath = "imgs/skins";
    public const string SkinEmptyImage = "data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==";
    public const string SkinRarityCssPrefix = "sk-rarity-";
    public const string SkinPurchaseRarityCssPrefix = "ska-rarity-";
    public const string SkinPickerRarityCssPrefix = "sp-";
    public const string SkinAnchorPrefix = "skin-";
    public const int SkinSeasonMonthsInYear = 12;
    public const string SkinCatalogInvalidFile = "That file is not a valid skin catalog.";
    public const string SkinCatalogEmpty = "That catalog has no skins.";
    public const string SkinCatalogUrlNotConfigured = "Set the catalog address in the shop settings first.";
    public const string SkinCatalogDownloadFailed = "The catalog could not be downloaded.";
    public const string SkinCatalogNoFile = "Choose the catalog file first.";
    public const string SkinSettingsInvalidUrl = "An address must start with http:// or https://.";
    public const int SkinAuthorSyncIntervalInHours = 24;
    public const int SkinAuthorCheckIntervalInMinutes = 60;
    public const int SkinAuthorSyncStartDelayInSeconds = 90;
    public const int SkinAuthorsPerRun = 300;
    public const string SkinAuthorSyncTempData = "SkinAuthorSync";
    public const string SkinAuthorSyncNoAnswer = "Steam did not answer. Try again in a few minutes.";
}