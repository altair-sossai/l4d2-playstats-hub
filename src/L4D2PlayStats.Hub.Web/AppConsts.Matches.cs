namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const string MatchesUrl = "/matches";
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
}