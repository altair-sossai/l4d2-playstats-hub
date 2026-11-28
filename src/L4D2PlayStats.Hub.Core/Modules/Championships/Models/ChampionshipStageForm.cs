using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Models;

public class ChampionshipStageForm
{
    public string Name { get; set; } = string.Empty;
    public ChampionshipStageType Type { get; set; }
    public int BestOf { get; set; } = 1;
    public int FinalBestOf { get; set; } = 1;
    public int Legs { get; set; } = 1;
    public int GroupCount { get; set; } = 1;
    public int PointsForWin { get; set; } = AppConsts.ChampionshipDefaultPointsForWin;
    public int PointsForDraw { get; set; } = AppConsts.ChampionshipDefaultPointsForDraw;
    public int PointsForLoss { get; set; }
    public List<ChampionshipTiebreaker> Tiebreakers { get; set; } = [];
    public int Qualifiers { get; set; }
    public ChampionshipSeeding Seeding { get; set; }
    public bool ThirdPlace { get; set; }
    public ChampionshipCampaignRule CampaignRule { get; set; }
    public bool Veto { get; set; }
    public bool OncePerTeam { get; set; }
    public int MinimumMaps { get; set; }
}
