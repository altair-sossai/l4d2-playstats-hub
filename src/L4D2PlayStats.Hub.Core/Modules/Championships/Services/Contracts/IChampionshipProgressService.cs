using L4D2PlayStats.Hub.Core.Modules.Championships.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

public interface IChampionshipProgressService
{
    void Apply(Championship championship, ChampionshipSeries series, IReadOnlyList<ChampionshipCampaignOption> campaigns);
    void DrawCampaign(Championship championship, ChampionshipStage stage, ChampionshipSeries series, ChampionshipGame game, IReadOnlyList<ChampionshipCampaignOption> campaigns);
}
