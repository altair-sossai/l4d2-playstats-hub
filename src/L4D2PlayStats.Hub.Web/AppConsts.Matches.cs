namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const string MatchesUrl = "/matches";
    public const string MatchSeasonQuery = "season";
    public const string MatchYearQuery = "year";
    public const string MatchServerQuery = "server";
    public const int MatchChartMapNameLength = 16;
    public const string MatchElapsedFormat = @"hh\:mm\:ss";
    public const string MatchChartSuccessColor = "rgba(40, 167, 69, 1)";
    public const string MatchChartDangerColor = "rgba(220, 53, 69, 1)";
    public const string MatchChartTransparentColor = "rgba(0, 0, 0, 0)";
    public const string MatchChartLightFontColor = "#222222";
    public const string MatchChartDarkFontColor = "#c8c8c8";
    public const string CampaignImagesFolder = "imgs/campaigns";
    public const string CampaignImageExtension = ".jpg";
    public const string NoCampaignImage = "no-image";
    public const int MinScore = 0;
    public const decimal AvgClearTimeDivisor = 1000m;
    public const string WeaponShotsPrefix = "Shots";

    public const string MatchesApiRoute = "api/matches";
    public const string ReplacedPlayerRequiredMessage = "Choose the player who left.";
    public const string ReplacementPlayerRequiredMessage = "Choose the player who came in.";
    public const string ReplacementMapsRequiredMessage = "Choose at least one map.";
    public const string ReplacementSamePlayerMessage = "The player who left and the player who came in must be different.";
    public const string ReplacementMatchNotFoundMessage = "The match was not found. Reload the page and try again.";
    public const string ReplacementMapNotInMatchMessage = "The map {0} is no longer part of this match. Reload the page and try again.";
    public const string ReplacementUnreadableMapMessage = "The statistics file of the map {0} could not be read.";
    public const string ReplacementNotInMapMessage = "The player who left did not play the map {0}.";
    public const string ReplacementAlreadyInMapMessage = "The player who came in already played the map {0}, so both would be in the same map.";
}
