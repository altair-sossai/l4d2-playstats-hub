namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const string AntiCheatUrl = "/anti-cheat";
    public const string CompetitiveWordStem = "competitiv";
    public const string AntiCheatAdminUrl = "/admin/anti-cheat";
    public const string AntiCheatAdminLegacyUrl = "/anti-cheat-admin";
    public const string AntiCheatVersionPath = "/v1/client/version";
    public const string AntiCheatVersionCacheKey = "AntiCheatVersion";
    public const int AntiCheatVersionCacheInMinutes = 5;
    public const int AntiCheatFailureCacheInSeconds = 30;
    public const int AntiCheatTimeoutInSeconds = 10;
    public const string AntiCheatAccessRoute = "api/anti-cheat-access";
    public const string AntiCheatAccessServerRoute = "api/anti-cheat-access/server";
    public const string AntiCheatAccessApiUrl = "/api/anti-cheat-access";
    public const int AntiCheatAdminPageSize = 20;
    public const string PlayerLookupUrl = "/api/admin/players/lookup";
    public const string PlayerLookupSteamNameCacheKey = "PlayerLookupSteamName_{0}";
    public const int PlayerLookupSteamNameCacheInHours = 4;
    public const int PlayerLookupFailureCacheInMinutes = 1;
}