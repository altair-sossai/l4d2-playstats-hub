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
}
