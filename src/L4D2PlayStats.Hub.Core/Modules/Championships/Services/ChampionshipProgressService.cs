using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services;

public class ChampionshipProgressService(IChampionshipRandomService random) : IChampionshipProgressService
{
    public void Apply(Championship championship, ChampionshipSeries series, IReadOnlyList<ChampionshipCampaignOption> campaigns)
    {
        var stage = championship.Stages.First(item => item.Id == series.StageId);

        if (series.Bye)
        {
            series.Finished = true;
            series.WinnerTeamId = series.TeamAId ?? series.TeamBId;
            Propagate(championship, stage, series, campaigns);

            return;
        }

        if (stage.IsKnockout && series.HasTeams && !series.Walkover)
            FitGames(championship, stage, series, campaigns);

        var progress = SeriesCalculator.Evaluate(series, stage.IsKnockout);

        series.Finished = progress.Finished;

        if (!series.Walkover)
            series.WinnerTeamId = progress.WinnerTeamId;

        foreach (var game in series.Games.Where(game => !game.IsPlayed && (series.Finished || series.Walkover || !game.IsInProgress)))
            game.Status = series.Finished || series.Walkover ? ChampionshipGameStatus.NotNeeded : ChampionshipGameStatus.Pending;

        if (!series.Finished && series.HasTeams)
            DrawNextDecider(championship, stage, series, campaigns);

        Propagate(championship, stage, series, campaigns);
    }

    public void DrawCampaign(Championship championship, ChampionshipStage stage, ChampionshipSeries series, ChampionshipGame game, IReadOnlyList<ChampionshipCampaignOption> campaigns)
    {
        var pool = CampaignRules.ForDraw(stage, series, game, campaigns);

        if (pool.Count == 0)
            return;

        game.CampaignId = pool[random.Next(pool.Count)].Id;
        game.CampaignSource = ChampionshipCampaignSource.Draw;
        game.PickedByTeamId = null;
    }

    private void FitGames(Championship championship, ChampionshipStage stage, ChampionshipSeries series, IReadOnlyList<ChampionshipCampaignOption> campaigns)
    {
        while (true)
        {
            var progress = SeriesCalculator.Evaluate(series, true);
            var games = series.Games.OrderBy(game => game.Number).ToList();

            if (games.Count < progress.RequiredGames)
            {
                var replay = new ChampionshipGame
                {
                    Number = games.Count == 0 ? 1 : games.Max(game => game.Number) + 1,
                    Replay = true,
                    Status = ChampionshipGameStatus.Pending
                };

                series.Games.Add(replay);

                if (stage.CampaignRule != ChampionshipCampaignRule.Manual)
                    DrawCampaign(championship, stage, series, replay, campaigns);

                continue;
            }

            var extra = games.LastOrDefault();

            if (games.Count > progress.RequiredGames && extra is { Replay: true, IsPlayed: false, IsInProgress: false })
            {
                series.Games.Remove(extra);

                continue;
            }

            return;
        }
    }

    private void DrawNextDecider(Championship championship, ChampionshipStage stage, ChampionshipSeries series, IReadOnlyList<ChampionshipCampaignOption> campaigns)
    {
        if (stage.CampaignRule != ChampionshipCampaignRule.Picks)
            return;

        if (stage.Veto && (series.TeamAVetoCampaignId == null || series.TeamBVetoCampaignId == null))
            return;

        var games = series.Games.OrderBy(game => game.Number).ToList();
        var next = games.FirstOrDefault(game => game.Status == ChampionshipGameStatus.Pending);

        if (next == null || next.CampaignId != null || !GamePlan.IsDecider(next, series.BestOf))
            return;

        if (games.Any(game => game.Number < next.Number && game.CampaignId == null))
            return;

        DrawCampaign(championship, stage, series, next, campaigns);
    }

    private void Propagate(Championship championship, ChampionshipStage stage, ChampionshipSeries series, IReadOnlyList<ChampionshipCampaignOption> campaigns)
    {
        if (!stage.IsKnockout)
            return;

        int? winner = series.Finished ? series.WinnerTeamId : null;
        int? loser = series.Finished ? series.LoserTeamId : null;

        Place(championship, stage, series.NextSeriesId, series.NextSlotA, winner, campaigns);
        Place(championship, stage, series.LoserNextSeriesId, series.LoserNextSlotA, loser, campaigns);
    }

    private void Place(Championship championship, ChampionshipStage stage, int? nextSeriesId, bool slotA, int? teamId, IReadOnlyList<ChampionshipCampaignOption> campaigns)
    {
        if (nextSeriesId == null)
            return;

        var next = stage.Series.FirstOrDefault(item => item.Id == nextSeriesId);

        if (next == null)
            return;

        var current = slotA ? next.TeamAId : next.TeamBId;

        if (current == teamId)
            return;

        if (next.Walkover || next.Games.Any(game => game.IsPlayed || game.IsInProgress))
            throw new ChampionshipException("The next series has already started. Clear its results first.");

        if (slotA)
            next.TeamAId = teamId;
        else
            next.TeamBId = teamId;

        next.TeamAVetoCampaignId = null;
        next.TeamBVetoCampaignId = null;
        next.ScheduledAtUtc = null;
        next.Proposals.Clear();

        foreach (var game in next.Games.Where(game => game.CampaignSource is ChampionshipCampaignSource.Pick or ChampionshipCampaignSource.Draw && stage.CampaignRule == ChampionshipCampaignRule.Picks))
        {
            game.CampaignId = null;
            game.CampaignSource = null;
            game.PickedByTeamId = null;
        }

        Apply(championship, next, campaigns);
    }
}
