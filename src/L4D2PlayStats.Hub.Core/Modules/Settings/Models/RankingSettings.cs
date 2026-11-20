namespace L4D2PlayStats.Hub.Core.Modules.Settings.Models;

public class RankingSettings
{
    public int Win { get; set; } = AppConsts.DefaultWinExperience;
    public int Loss { get; set; } = AppConsts.DefaultLossExperience;
    public int RageQuit { get; set; } = AppConsts.DefaultRageQuitExperience;
    public int Mvp { get; set; } = AppConsts.DefaultMvpExperience;
    public int MvpCommon { get; set; } = AppConsts.DefaultMvpCommonExperience;
    public int MinimumMatches { get; set; }
    public bool IncludeUnknownCampaigns { get; set; } = true;
    public int MatchGapInHours { get; set; } = AppConsts.MatchClusterGapInHours;
    public int DefaultPunishment { get; set; } = AppConsts.PlayerDefaultPunishment;
    public int HomeVisiblePlayers { get; set; } = AppConsts.RankingVisiblePlayers;
    public int HomeListedPlayers { get; set; } = AppConsts.RankingListedPlayers;
    public int MatchesPerPage { get; set; } = AppConsts.LastMatchesPageSize;
    public int PlayerRecentMatches { get; set; } = AppConsts.PlayerRecentMatches;

    public TimeSpan MatchGap => TimeSpan.FromHours(Math.Clamp(MatchGapInHours, AppConsts.MatchMinGapInHours, AppConsts.MatchMaxGapInHours));

    public void Normalize()
    {
        MinimumMatches = Math.Max(0, MinimumMatches);
        MatchGapInHours = Math.Clamp(MatchGapInHours, AppConsts.MatchMinGapInHours, AppConsts.MatchMaxGapInHours);
        DefaultPunishment = Math.Clamp(DefaultPunishment, 1, AppConsts.MaxScore);
        HomeListedPlayers = Math.Clamp(HomeListedPlayers, 1, AppConsts.RankingMaxListedPlayers);
        HomeVisiblePlayers = Math.Clamp(HomeVisiblePlayers, 1, HomeListedPlayers);
        MatchesPerPage = Math.Clamp(MatchesPerPage, AppConsts.MatchListMinSize, AppConsts.MatchListMaxSize);
        PlayerRecentMatches = Math.Clamp(PlayerRecentMatches, AppConsts.MatchListMinSize, AppConsts.MatchListMaxSize);
    }
}
