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

    public const int DefaultCommunityMinimumMatchesToVote = 10;
    public const int DefaultCommunityVotesPerDay = 20;
    public const int DefaultCommunityMinimumMatchesToBeCandidate = 1;
    public const int DefaultCommunityMinimumComparisonsToBeListed = 5;
    public const int DefaultCommunityKFactor = 32;
    public const int DefaultCommunityInitialRating = 1500;
    public const int CommunityMinVotesPerDay = 1;
    public const int CommunityMinKFactor = 1;
    public const int CommunityMaxKFactor = 200;
    public const double CommunityEloScale = 400;
    public const double CommunityEloBase = 10;
    public const double CommunityWinScore = 1;
    public const double CommunityDrawScore = 0.5;
    public const double CommunityLossScore = 0;
    public const int CommunityAnchorPoolSize = 5;
    public const int CommunityOpponentPoolSize = 5;
    public const int CommunityPairSides = 2;
    public const int CommunityVoterListSize = 100;

    public const int LiveDelayInSeconds = 15;
    public const int LiveConfigurationExpirationInMinutes = 120;
    public const int LiveRoundExpirationInMinutes = 120;
    public const int LiveScoreboardExpirationInMinutes = 10;
    public const int LivePlayersExpirationInMinutes = 120;
    public const int LiveFeedRetentionInMinutes = 60;
    public const int LiveFeedMaxItems = 250;
    public const decimal LiveLatencyLowLimit = 0.1m;
    public const decimal LiveLatencyMediumLimit = 0.2m;
    public const int LiveSurvivorMaxHealth = 100;
    public const int LiveIncapacitatedMaxHealth = 300;
    public const int LiveHealthRedLimit = 25;
    public const int LiveHealthOrangeLimit = 40;
    public const int LivePlayerNameMaxLength = 100;
    public static readonly string[] LiveChatBindPhrases = ["gogogo", "go go go", "back back", "back back back", "tank tank", "tank tank tank", "hit hit", "hit hit hit", "r"];

    public const int SiteChatRetentionInMinutes = 60;
    public const int SiteChatGlobalCooldownInSeconds = 15;
    public const int SiteChatUserCooldownInSeconds = 30;
    public const int SiteChatMaxMessageLength = 200;
    public const int SiteChatRecentSenders = 30;
    public const int ChatBanReasonMaxLength = 300;

    public const string ServerCommandPattern = @"^(!|/|sm_)(\w+)( ([^;\r\n]+))?$";
    public const string ServerCommandPrefix = "sm_";
    public const int ServerCommandMaxLength = 256;
    public const int RemoteCommandExpirationInMinutes = 5;

    public const int PlayerConnectionRetentionInMonths = 3;
    public const int PlayerConnectionCleanupBatchSize = 100;
    public const int IpAddressMaxLength = 45;
    public const int SkinIdMaxLength = 64;
    public const int SkinWeaponMaxLength = 64;
    public const int SkinWorkshopIdMaxLength = 32;
    public const int SkinNameMaxLength = 100;
    public const int SkinTitleMaxLength = 300;
    public const int SkinLanguageMaxLength = 10;
    public const int SkinSourceMaxLength = 20;
    public const int SkinMinPrice = 1;
    public const int SkinMaxPrice = 100000;
    public const int SkinShotCount = 6;
    public const string SkinDefaultLanguage = "en";
    public const string SkinCommand = "!skin";
    public const string SkinThumbnailsFolder = "skins";
    public const string SkinThumbnailExtension = ".jpg";
    public const string SkinShotsFolder = "skin-shots";
    public const string SkinShotExtension = ".webp";
    public const string SkinShotThumbnailSuffix = "-thumb";
    public const string SkinWorkshopUrl = "https://steamcommunity.com/sharedfiles/filedetails/?id=";
    public const string SkinCatalogHttpClient = "SkinCatalog";
    public const int SkinCatalogTimeoutInSeconds = 30;
    public const int SkinCatalogMaxBytes = 5 * 1024 * 1024;
    public const int SkinShopLockTimeoutInSeconds = 5;
    public const int SkinSeasonClockStepInMinutes = 30;
    public const string SkinSourceShop = "shop";
    public const string SkinSourceGift = "gift";
    public const string SkinSourceSite = "site";
    public const string SkinSourceGame = "game";

    public const char TeamACode = 'A';
    public const char TeamBCode = 'B';
    public const string SkinStarsCacheKey = "skin_stars";
    public const string SkinAuthorRequestUrl = "https://github.com/altair-sossai/l4d2-zone-server-assets/issues/new";
    public const string SkinMeleePrefix = "weapon_melee_";
    public const string SteamIdPattern = @"^STEAM_\d:(\d):(\d+)$";
    public const string Steam3Pattern = @"^\[?U:\d:(\d+)]?$";
    public const string SteamCommunityIdPattern = @"^(\d{16,})$";
    public const string SteamProfilePattern = @"^https?:\/\/steamcommunity\.com\/profiles\/(\d{16,})\/?$";
    public const string SteamProfileUrlPattern = @"^https?:\/\/steamcommunity\.com\/profiles\/(\d{16,})\/?$";
}
