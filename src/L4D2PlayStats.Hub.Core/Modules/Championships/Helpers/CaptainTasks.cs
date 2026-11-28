using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;

public static class CaptainTasks
{
    public static List<ChampionshipCaptainTask> Of(Championship championship, ChampionshipTeam team, DateTime now)
    {
        var tasks = new List<ChampionshipCaptainTask>();

        if (!championship.IsPublic || championship.IsOver)
            return tasks;

        if (ChampionshipDraftService.IsOrdered(championship) && ChampionshipDraftService.OnTheClock(championship)?.Id == team.Id)
            tasks.Add(new ChampionshipCaptainTask(ChampionshipCaptainTaskKind.DraftTurn));

        if (team.RejectionReason != null && !team.HasPendingIdentity)
            tasks.Add(new ChampionshipCaptainTask(ChampionshipCaptainTaskKind.IdentityRejected));

        if (championship.Status != ChampionshipStatus.Running)
            return tasks;

        foreach (var stage in championship.Stages.OrderBy(stage => stage.Order))
        foreach (var series in stage.Series.Where(series => series is { Finished: false, Bye: false } && series.HasTeams && series.Has(team.Id)).OrderBy(series => series.Round).ThenBy(series => series.Position))
        {
            var teamA = series.TeamAId == team.Id;
            var started = series.Games.Any(game => game.IsPlayed || game.IsInProgress);
            var vetoesDone = !stage.Veto || series.TeamAVetoCampaignId != null && series.TeamBVetoCampaignId != null;

            if (stage.Veto && !started && (teamA ? series.TeamAVetoCampaignId : series.TeamBVetoCampaignId) == null)
                tasks.Add(new ChampionshipCaptainTask(ChampionshipCaptainTaskKind.Veto, series.Id));

            if (stage.CampaignRule == ChampionshipCampaignRule.Picks && vetoesDone)
            {
                foreach (var game in series.Games.Where(game => game.Status == ChampionshipGameStatus.Pending && game.CampaignId == null && GamePlan.PickedByTeamA(game, series.BestOf) == teamA).OrderBy(game => game.Number))
                    tasks.Add(new ChampionshipCaptainTask(ChampionshipCaptainTaskKind.Pick, series.Id, game.Number));
            }

            var open = series.Proposals.Where(proposal => !proposal.Confirmed && !proposal.Refused && proposal.DateUtc > now).ToList();

            if (open.Any(proposal => proposal.TeamId != team.Id && (teamA ? proposal.TeamAAnswer : proposal.TeamBAnswer) == ChampionshipAnswer.None))
                tasks.Add(new ChampionshipCaptainTask(ChampionshipCaptainTaskKind.AnswerDate, series.Id));
            else if (series.ScheduledAtUtc == null && !started && open.Count == 0)
                tasks.Add(new ChampionshipCaptainTask(ChampionshipCaptainTaskKind.ProposeDate, series.Id));
        }

        return tasks;
    }
}
