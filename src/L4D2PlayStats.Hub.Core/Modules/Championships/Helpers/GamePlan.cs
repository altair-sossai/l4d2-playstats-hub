using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;

public static class GamePlan
{
    public static bool IsDecider(ChampionshipGame game, int bestOf)
    {
        if (game.Replay || bestOf <= 1)
            return true;

        return bestOf % 2 == 1 && game.Number == bestOf;
    }

    public static bool? PickedByTeamA(ChampionshipGame game, int bestOf)
    {
        if (IsDecider(game, bestOf))
            return null;

        return game.Number % 2 == 1;
    }

    public static bool TeamPicks(ChampionshipStage stage, ChampionshipGame game, int bestOf)
    {
        return stage.CampaignRule == ChampionshipCampaignRule.Picks && PickedByTeamA(game, bestOf) != null;
    }
}
