using L4D2PlayStats.Hub.Core.Modules.Settings.Attributes;

namespace L4D2PlayStats.Hub.Core.Modules.Settings.Models;

public class SiteSettings
{
    public string Name { get; set; } = AppConsts.DefaultSiteName;
    public string TimeZone { get; set; } = AppConsts.DefaultTimeZone;
    public string DefaultCulture { get; set; } = AppConsts.DefaultCulture;
    public string? PublicUrl { get; set; }
    [Secret]
    public string? SteamApiKey { get; set; }
    public string? GoogleAnalyticsId { get; set; }
    public string? FooterLinks { get; set; }
    public string? MetaKeywords { get; set; }
}
