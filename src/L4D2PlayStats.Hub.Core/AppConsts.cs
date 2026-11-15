namespace L4D2PlayStats.Hub.Core;

public static class AppConsts
{
    public const string DefaultSiteName = "L4D2 PlayStats";
    public const string DefaultTimeZone = "UTC";
    public const string DefaultCulture = "en-US";

    public const string DatabaseSection = "Database";
    public const string SqliteProvider = "Sqlite";
    public const string SqlServerProvider = "SqlServer";
    public const string PostgreSqlProvider = "PostgreSql";
    public const string MySqlProvider = "MySql";
    public const string DefaultConnectionString = "Data Source=hub.db";

    public const int GameServerKeyBytes = 32;
    public const int GameServerPublicIdMaxLength = 40;
    public const char GameServerTokenSeparator = ':';
    public const int ServerOnlineWindowInSeconds = 90;
    public const int ServerLastSeenSaveIntervalInSeconds = 60;
    public const int InvalidKeyWindowInMinutes = 10;

    public const int SetupTokenBytes = 16;

    public const int DefaultSeasonLengthInMonths = 2;
    public const int MaxSeasonLengthInMonths = 24;

    public const int UploadRetryWindowInHours = 12;
    public const int MatchClusterGapInHours = 6;
    public const int UnknownCampaignMapGapInHours = 2;
    public const int MaxScore = 10000;

    public const int DefaultWinExperience = 85;
    public const int DefaultLossExperience = -80;
    public const int DefaultRageQuitExperience = -150;
    public const int DefaultMvpExperience = 8;
    public const int DefaultMvpCommonExperience = 6;

    public const int DefaultMinimumMaps = 2;
    public const int DefaultTeamSize = 4;
    public const string AnyConfigurationPattern = "*";

    public const int MixPlayers = 8;
    public const int MostUsedNames = 3;
    public const string UnknownPlayerName = "Unknown";

    public const int PatentMinLevel = 1;
    public const int PatentMaxLevel = 15;
    public const int PatentMinExperience = 0;
    public const int PatentMaxExperience = 2500;

    public const int SettingsCacheInMinutes = 60;
    public const int RankingCacheInMinutes = 60;

    public const long SteamCommunityIdBase = 76561197960265728;
    public const string SteamProfileUrl = "https://steamcommunity.com/profiles/";
}
