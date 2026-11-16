namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const string AdministratorPolicy = "Administrator";
    public const string GameServerItem = "GameServer";
    public const string AuthorizationHeader = "Authorization";

    public const string SetupCookie = "hub_setup";
    public const int SetupCookieInMinutes = 15;
    public const string ThemeCookie = "hub_theme";
    public const string LightTheme = "light";
    public const string DefaultTheme = "default";
    public const string DefaultOpenGraphType = "website";
    public const string ProjectName = "L4D2 PlayStats Hub";
    public const string ProjectUrl = "https://github.com/altair-sossai/l4d2-playstats-hub";
    public const char FooterLinkSeparator = '|';
    public const string BrandingFolder = "branding";
    public const string LogoFile = "logo";
    public const string LogoUrl = "/site/logo";
    public const int LogoCacheInSeconds = 3600;
    public const int LogoMaxBytes = 1048576;
    public const int DefaultTeamSize = 4;
    public const int RankingVisiblePlayers = 50;
    public const int PaginationWindow = 2;
    public const int PlayerPickerPreviousNames = 5;
    public const int RankingListedPlayers = 100;
    public const string AllTimeHistoryId = "all-time";
    public const int LongPlayerNameLength = 15;
    public const int LongPlayerNameVisibleLength = 10;
    public const int PlayerNameVisibleLength = 30;
    public const int HistoryPodiumPlayers = 6;
    public const int LoginCookieInDays = 30;

    public const string DefaultCulture = "en-US";
    public const string ResourcesFolder = "Resources";
    public static readonly string[] SupportedCultures = ["en-US", "pt-BR", "es-ES"];

    public static readonly IReadOnlyDictionary<string, string> LogoContentTypes = new Dictionary<string, string>
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".webp"] = "image/webp",
        [".gif"] = "image/gif"
    };
    public const int InGameRankingPlayers = 50;
    public const int InGameMatches = 10;

    public const int RankingPageSize = 100;
    public const int ServerStatusRefreshInSeconds = 3;
    public const int SeasonCheckIntervalInMinutes = 30;

    public const string DataDirectorySetting = "Hub:DataDirectory";
    public const string DefaultDataDirectory = "data";
    public const string KeysFolder = "keys";
    public const string SetupCodeFile = "setup-code.txt";
    public const string ApplicationName = "L4D2PlayStatsHub";

    public const string AvatarFolder = "avatars";
    public const int AvatarCacheInSeconds = 3600;
    public const int AvatarCheckIntervalInMinutes = 30;
    public const int AvatarMaxAgeInHours = 24;
    public const int AvatarActivePlayerInDays = 180;
    public const int AvatarBatchSize = 100;
    public const int AvatarBatchesPerRun = 5;
    public const int AvatarRetryInHours = 6;
    public const int AvatarKeylessPerRun = 120;
    public const int AvatarKeylessParallel = 4;
    public const int AvatarKeylessDelayInMilliseconds = 400;
    public const int AvatarKeylessPauseInMinutes = 10;
    public const string AvatarContentType = "image/jpeg";
    public const string NoStore = "no-store";
    public const string SteamProfileXmlUrl = "https://steamcommunity.com/profiles/{0}/?xml=1";
    public const string SteamProfileAvatarPattern = @"<avatarFull><!\[CDATA\[(https://[^\]]+)\]\]></avatarFull>";
    public const int SteamTimeoutInSeconds = 10;
    public const string SteamSummariesUrl = "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/";

    public const string ServerUrlCvar = "playstats_url";
    public const string ServerTokenCvar = "playstats_token";

    public const string NpgsqlLegacyTimestampSwitch = "Npgsql.EnableLegacyTimestampBehavior";
    public const string SqliteMemoryDataSource = ":memory:";

    public const string AdminCommand = "admin";
    public const string AdminListAction = "list";
    public const string AdminAddAction = "add";

    public const string DevelopmentSignInSetting = "Development:AllowTestSignIn";
    public const string SteamOpenIdPattern = @"openid/id/(\d+)$";

    public const string HtmxRequestHeader = "HX-Request";
    public const string CommunityBallotElement = "community-ballot";
    public const string CommunityVotePage = "/community/vote";
    public const int RemoteCommandLogSize = 100;
    public const int PlayerConnectionCleanupIntervalInHours = 24;
    public const string SteamConnectUrl = "steam://connect/";
    public const string LiveTimeFormat = "HH:mm";
    public const string EmptyAvatarUrl = "/imgs/avatar-empty.png";
    public const string SkinShopUrl = "/skins";
    public const string SkinAdminUrl = "/admin/skins";
    public const string SkinPricesUrl = "/admin/skins/prices";
    public const int SkinSeasonRefundIntervalInMinutes = 10;
    public const int SkinSeasonRefundStartDelayInSeconds = 30;
    public const string SkinImportTempData = "SkinImport";
    public const char SkinTempDataSeparator = ',';
}
