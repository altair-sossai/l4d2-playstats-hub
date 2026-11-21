namespace L4D2PlayStats.Hub.Web;

public static partial class AppConsts
{
    public const string SteamBansUrl = "https://api.steampowered.com/ISteamUser/GetPlayerBans/v1/";
    public const string SteamOwnedGamesUrl = "https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/";
    public const string SteamLevelUrl = "https://api.steampowered.com/IPlayerService/GetSteamLevel/v1/";
    public const int Left4Dead2AppId = 550;
    public const int SteamDataBatchSize = 100;
    public const int SteamDataPlayersPerRun = 100;
    public const int SteamDataIntervalInMinutes = 10;
    public const int SteamDataStartDelayInSeconds = 30;
    public const int SteamDataPauseInMinutes = 60;
    public const int MinutesPerHour = 60;
    public const string SteamHoursFormat = "N1";
}
