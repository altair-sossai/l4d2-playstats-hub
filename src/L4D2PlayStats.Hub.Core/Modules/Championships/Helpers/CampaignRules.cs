using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;

public static class CampaignRules
{
    public static List<ChampionshipCampaignOption> Eligible(ChampionshipStage stage, IEnumerable<ChampionshipCampaignOption> campaigns)
    {
        return campaigns.Where(campaign => stage.MinimumMaps <= 0 || campaign.Maps >= stage.MinimumMaps).OrderBy(campaign => campaign.Name).ToList();
    }

    public static List<ChampionshipCampaignOption> ForPick(ChampionshipStage stage, ChampionshipSeries series, ChampionshipGame game, int teamId, IEnumerable<ChampionshipCampaignOption> campaigns, IEnumerable<ChampionshipGame> championshipGames)
    {
        var used = UsedInSeries(series, game);

        if (stage.OncePerTeam)
            used.UnionWith(championshipGames
                .Where(other => other.Id != game.Id && other.CampaignSource == ChampionshipCampaignSource.Pick && other.PickedByTeamId == teamId && other.CampaignId != null)
                .Select(other => other.CampaignId!.Value));

        return Eligible(stage, campaigns).Where(campaign => !used.Contains(campaign.Id)).ToList();
    }

    public static List<ChampionshipCampaignOption> ForVeto(ChampionshipStage stage, ChampionshipSeries series, bool teamA, IEnumerable<ChampionshipCampaignOption> campaigns)
    {
        var used = series.Games.Where(game => game.CampaignId != null).Select(game => game.CampaignId!.Value).ToHashSet();
        var other = teamA ? series.TeamBVetoCampaignId : series.TeamAVetoCampaignId;

        if (other != null)
            used.Add(other.Value);

        return Eligible(stage, campaigns).Where(campaign => !used.Contains(campaign.Id)).ToList();
    }

    public static List<ChampionshipCampaignOption> ForDraw(ChampionshipStage stage, ChampionshipSeries series, ChampionshipGame game, IReadOnlyList<ChampionshipCampaignOption> campaigns)
    {
        var used = UsedInSeries(series, game);
        var eligible = Eligible(stage, campaigns);
        var pool = eligible.Where(campaign => !used.Contains(campaign.Id)).ToList();

        if (pool.Count == 0)
            pool = eligible.Where(campaign => series.Games.All(other => other.Id == game.Id || other.CampaignId != campaign.Id)).ToList();

        if (pool.Count == 0)
            pool = eligible.Count > 0 ? eligible : campaigns.ToList();

        return pool;
    }

    private static HashSet<int> UsedInSeries(ChampionshipSeries series, ChampionshipGame game)
    {
        var used = series.Games.Where(other => !ReferenceEquals(other, game) && (game.Id == 0 || other.Id != game.Id) && other.CampaignId != null).Select(other => other.CampaignId!.Value).ToHashSet();

        if (series.TeamAVetoCampaignId != null)
            used.Add(series.TeamAVetoCampaignId.Value);

        if (series.TeamBVetoCampaignId != null)
            used.Add(series.TeamBVetoCampaignId.Value);

        return used;
    }
}
