using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Championships;

public class ChampionshipStage
{
    public int Id { get; set; }
    public int ChampionshipId { get; set; }
    public int Order { get; set; }
    public string Name { get; set; } = null!;
    public ChampionshipStageType Type { get; set; }
    public int BestOf { get; set; } = 1;
    public int FinalBestOf { get; set; } = 1;
    public int Legs { get; set; } = 1;
    public int GroupCount { get; set; } = 1;
    public int PointsForWin { get; set; } = AppConsts.ChampionshipDefaultPointsForWin;
    public int PointsForDraw { get; set; } = AppConsts.ChampionshipDefaultPointsForDraw;
    public int PointsForLoss { get; set; }
    public string Tiebreakers { get; set; } = string.Empty;
    public int Qualifiers { get; set; }
    public ChampionshipSeeding Seeding { get; set; }
    public bool ThirdPlace { get; set; }
    public ChampionshipCampaignRule CampaignRule { get; set; }
    public bool Veto { get; set; }
    public bool OncePerTeam { get; set; }
    public int MinimumMaps { get; set; }
    public bool Generated { get; set; }
    public List<ChampionshipGroup> Groups { get; set; } = [];
    public List<ChampionshipSeries> Series { get; set; } = [];
    public List<ChampionshipPenalty> Penalties { get; set; } = [];

    public bool IsKnockout => Type == ChampionshipStageType.Knockout;
}
