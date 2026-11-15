namespace L4D2PlayStats.Hub.Web;

public static class AppConsts
{
    public const string AdministratorPolicy = "Administrator";
    public const string GameServerItem = "GameServer";
    public const string AuthorizationHeader = "Authorization";

    public const string SetupCookie = "hub_setup";
    public const int SetupCookieInMinutes = 15;
    public const string ThemeCookie = "hub_theme";
    public const int LoginCookieInDays = 30;

    public const string DefaultCulture = "en-US";
    public const string ResourcesFolder = "Resources";
    public static readonly string[] SupportedCultures = ["en-US", "pt-BR", "es-ES"];

    public const string InGameUserAgent = "Valve Client";
    public const int InGameRankingPlayers = 50;
    public const int InGameMatches = 10;

    public const int RankingPageSize = 100;
    public const int MatchesPageSize = 30;
    public const int PlayerRelations = 12;
    public const int ServerStatusRefreshInSeconds = 3;
    public const int SeasonCheckIntervalInMinutes = 30;

    public const string AvatarFolder = "data/avatars";
    public const int AvatarCacheInSeconds = 3600;
    public const int AvatarCheckIntervalInMinutes = 30;
    public const int AvatarMaxAgeInHours = 24;
    public const int AvatarActivePlayerInDays = 180;
    public const int AvatarBatchSize = 100;
    public const int AvatarBatchesPerRun = 5;
    public const int SteamTimeoutInSeconds = 10;
    public const string SteamSummariesUrl = "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/";

    public const string ServerUrlCvar = "playstats_url";
    public const string ServerTokenCvar = "playstats_token";

    public const string DevelopmentSignInSetting = "Development:AllowTestSignIn";
    public const string SteamOpenIdPattern = @"openid/id/(\d+)$";

    public const int LiveRefreshInSeconds = 3;
    public const int ServerListRefreshInSeconds = 10;
    public const int RemoteCommandLogSize = 100;
    public const int PlayerConnectionCleanupIntervalInHours = 24;
    public const string HtmxRequestHeader = "HX-Request";
    public const string SteamConnectUrl = "steam://connect/";
    public const string LiveTimeFormat = "HH:mm";
    public const string EmptyAvatarUrl = "/imgs/avatar-empty.png";
}
