namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const string PageCachePolicy = "Pages";
    public const string PageCacheTag = "pages";
    public const string CommunityCacheTag = "community";
    public const int PageCacheInSeconds = 60;
    public const string PageCacheAllQueryKeys = "*";
    public const string PageCacheCultureKey = "culture";
    public const string PageCacheThemeKey = "theme";
    public const string PageCacheInGameKey = "ingame";
    public const string PageCacheDataKey = "data";
    public static readonly string[] PageCacheRefusedCacheControls = ["no-store", "no-cache", "private"];
}
