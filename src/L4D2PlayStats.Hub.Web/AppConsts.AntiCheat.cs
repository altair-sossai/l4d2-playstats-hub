namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const string AntiCheatUrl = "/anti-cheat";
    public const string AntiCheatAdminUrl = "/admin/anti-cheat";
    public const string AntiCheatVersionPath = "/v1/client/version";
    public const string AntiCheatVersionCacheKey = "AntiCheatVersion";
    public const int AntiCheatVersionCacheInMinutes = 5;
    public const int AntiCheatFailureCacheInSeconds = 30;
    public const int AntiCheatTimeoutInSeconds = 10;
    public const double BytesPerMegabyte = 1048576d;
}
