namespace L4D2PlayStats.Hub.Core.Modules.Settings.Models;

public class SiteSettings
{
    public string Name { get; set; } = AppConsts.DefaultSiteName;
    public string TimeZone { get; set; } = AppConsts.DefaultTimeZone;
    public string DefaultCulture { get; set; } = AppConsts.DefaultCulture;
    public string? PublicUrl { get; set; }
    public string? SteamApiKey { get; set; }
}
