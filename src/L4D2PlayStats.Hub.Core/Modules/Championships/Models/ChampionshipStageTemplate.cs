using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Models;

public record ChampionshipStageTemplate(string Name, ChampionshipStageType Type, int GroupCount, int Qualifiers, int BestOf, ChampionshipCampaignRule CampaignRule, ChampionshipSeeding Seeding);
