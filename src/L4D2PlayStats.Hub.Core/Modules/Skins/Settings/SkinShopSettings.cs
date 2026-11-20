namespace L4D2PlayStats.Hub.Core.Modules.Skins.Settings;

public class SkinShopSettings
{
    public bool RefundAtSeasonEnd { get; set; } = true;
    public string? AssetsUrl { get; set; }
    public string? CatalogUrl { get; set; }
    public string? AuthorContactUrl { get; set; }
    public string? AuthorRequestUrl { get; set; } = AppConsts.SkinAuthorRequestUrl;
}
