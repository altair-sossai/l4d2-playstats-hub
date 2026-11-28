using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Enums;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Models;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services;

public class ChampionshipNotificationService(INotificationService notifications) : IChampionshipNotificationService
{
    public void JoinAsked(Championship championship, long communityId)
    {
        notifications.SendAdministrators(Message("{0} asked for a place in {1}.", AdminUrl(championship, "participants"), communityId, championship.Name));
    }

    public void JoinCancelled(Championship championship, long communityId)
    {
        notifications.SendAdministrators(Message("{0} cancelled the request to join {1}.", AdminUrl(championship, "participants"), communityId, championship.Name));
    }

    public void JoinAnswered(Championship championship, long communityId, bool approved)
    {
        notifications.Send(communityId, approved
            ? Message("Your request to join {0} was approved: you are a participant now.", Page(championship), championship.Name)
            : Message("Your request to join {0} was refused.", Page(championship), championship.Name));
    }

    public void StatusChanged(Championship championship, ChampionshipStatus previous)
    {
        if (championship.Status == previous)
            return;

        if (previous == ChampionshipStatus.Draft && championship.IsPublic && !championship.IsOver)
        {
            notifications.SendEveryone(championship is { JoinRequests: true, Status: ChampionshipStatus.Participants }
                ? Message("There is a new championship you can join: {0}.", Page(championship), championship.Name)
                : Message("A new championship was published: {0}.", Page(championship), championship.Name));

            return;
        }

        var text = championship.Status switch
        {
            ChampionshipStatus.Participants when championship.JoinRequests => "The registrations of {0} are open.",
            ChampionshipStatus.Running => "{0} has started.",
            ChampionshipStatus.Finished => "{0} has finished.",
            ChampionshipStatus.Cancelled => "{0} was cancelled.",
            _ => null
        };

        if (text == null)
            return;

        if (championship.Status == ChampionshipStatus.Participants)
            notifications.SendEveryone(Message(text, Page(championship), championship.Name));
        else
            notifications.Send(Players(championship), Message(text, Page(championship), championship.Name));
    }

    public void ParticipantAdded(Championship championship, long communityId)
    {
        notifications.Send(communityId, Message("You are a participant of {0}.", Page(championship), championship.Name));
    }

    public void ParticipantRemoved(Championship championship, long communityId)
    {
        notifications.Send(communityId, Message("You are no longer a participant of {0}.", Page(championship), championship.Name));
    }

    public void MemberAdded(Championship championship, ChampionshipTeam team, long communityId)
    {
        notifications.Send(communityId, Message("You are in the team {0} of {1}.", $"{Page(championship)}/teams", team.Name, championship.Name));
    }

    public void LeaderChosen(Championship championship, ChampionshipTeam team, long communityId, bool captain)
    {
        notifications.Send(communityId, captain
            ? Message("You are the captain of {0} in {1}. Open the captain area to manage the team.", Captain(championship), team.Name, championship.Name)
            : Message("You are the vice-captain of {0} in {1}. Open the captain area to manage the team.", Captain(championship), team.Name, championship.Name));
    }

    public void DraftOrdered(Championship championship)
    {
        notifications.Send(championship.Teams.Select(team => team.CaptainCommunityId).OfType<long>(), Message("The order of the draft of {0} was drawn.", $"{Page(championship)}/draft", championship.Name));
        Turn(championship);
    }

    public void Picked(Championship championship, ChampionshipTeam team, long communityId)
    {
        notifications.Send(communityId, Message("{0} chose you in the draft of {1}.", $"{Page(championship)}/teams", team.Name, championship.Name));
        Turn(championship);
    }

    public void IdentitySent(Championship championship, ChampionshipTeam team)
    {
        notifications.SendAdministrators(Message("The team {0} sent a new name, tag or flag for approval in {1}.", AdminUrl(championship, "teams"), team.Name, championship.Name));
    }

    public void IdentityAnswered(Championship championship, ChampionshipTeam team, bool approved)
    {
        notifications.Send(Leaders(team), approved
            ? Message("The new name, tag or flag of {0} was approved in {1}.", Captain(championship), team.Name, championship.Name)
            : Message("The new name, tag or flag of {0} was refused in {1}.", Captain(championship), team.Name, championship.Name));
    }

    public void StageBuilt(Championship championship, ChampionshipStage stage)
    {
        var teams = stage.Series.SelectMany(series => new[] { series.TeamAId, series.TeamBId }).OfType<int>()
            .Concat(stage.Groups.SelectMany(group => group.Teams).Select(entry => entry.TeamId))
            .ToHashSet();

        notifications.Send(championship.Teams.Where(team => teams.Contains(team.Id)).SelectMany(team => team.Players()), Message("The games of {0} in {1} are ready.", $"{Page(championship)}/games", stage.Name, championship.Name));
    }

    public void DateProposed(Championship championship, ChampionshipSeries series, int teamId)
    {
        var other = Team(championship, teamId == series.TeamAId ? series.TeamBId : series.TeamAId);

        if (other != null)
            notifications.Send(Leaders(other), Message("{0} proposed a date for the series against your team in {1}.", Captain(championship), TeamName(championship, teamId), championship.Name));
    }

    public void DateAnswered(Championship championship, ChampionshipSeries series, ChampionshipDateProposal proposal, bool accepted)
    {
        if (accepted && proposal.Confirmed)
        {
            Scheduled(championship, series);
            return;
        }

        var proposer = Team(championship, proposal.TeamId);

        if (!accepted && proposer != null)
            notifications.Send(Leaders(proposer), Message("{0} refused the date your team proposed in {1}.", Captain(championship), TeamName(championship, proposer.Id == series.TeamAId ? series.TeamBId : series.TeamAId), championship.Name));
    }

    public void Scheduled(Championship championship, ChampionshipSeries series)
    {
        notifications.Send(SeriesPlayers(championship, series), Message("The series {0} × {1} of {2} is scheduled.", Series(championship, series), TeamName(championship, series.TeamAId), TeamName(championship, series.TeamBId), championship.Name));
    }

    public void Vetoed(Championship championship, ChampionshipSeries series, int teamId)
    {
        var other = Team(championship, teamId == series.TeamAId ? series.TeamBId : series.TeamAId);

        if (other != null)
            notifications.Send(Leaders(other), Message("{0} vetoed a campaign in the series against your team in {1}.", Captain(championship), TeamName(championship, teamId), championship.Name));
    }

    public void CampaignPicked(Championship championship, ChampionshipSeries series, ChampionshipGame game, int teamId)
    {
        var other = Team(championship, teamId == series.TeamAId ? series.TeamBId : series.TeamAId);

        if (other != null)
            notifications.Send(Leaders(other), Message("{0} chose the campaign of game {1} against your team in {2}.", Series(championship, series), TeamName(championship, teamId), game.Number, championship.Name));
    }

    public void GameFinished(Championship championship, ChampionshipSeries series, ChampionshipGame game, bool seriesWasFinished)
    {
        var players = SeriesPlayers(championship, series);

        notifications.Send(players, Message("Game {0} of {1} × {2} ended {3} × {4} in {5}.", Series(championship, series), game.Number, TeamName(championship, series.TeamAId), TeamName(championship, series.TeamBId), game.TeamATotal, game.TeamBTotal, championship.Name));

        if (series.Finished && !seriesWasFinished && series.WinnerTeamId != null)
            notifications.Send(players, Message("{0} won the series against {1} in {2}.", Series(championship, series), TeamName(championship, series.WinnerTeamId), TeamName(championship, series.WinnerTeamId == series.TeamAId ? series.TeamBId : series.TeamAId), championship.Name));
    }

    public void Walkover(Championship championship, ChampionshipSeries series)
    {
        if (series.WinnerTeamId == null)
            return;

        notifications.Send(SeriesPlayers(championship, series), Message("{0} won the series against {1} by walkover in {2}.", Series(championship, series), TeamName(championship, series.WinnerTeamId), TeamName(championship, series.WinnerTeamId == series.TeamAId ? series.TeamBId : series.TeamAId), championship.Name));
    }

    private void Turn(Championship championship)
    {
        var team = ChampionshipDraftService.OnTheClock(championship);

        if (team != null)
            notifications.Send(Leaders(team), Message("It is the turn of {0} in the draft of {1}: choose a player in the captain area.", $"{Captain(championship)}?tab=draft", team.Name, championship.Name));
    }

    private static NotificationMessage Message(string text, string url, params object?[] values)
    {
        return new NotificationMessage(NotificationTopic.Championships, text, url, values);
    }

    private static IEnumerable<long> Players(Championship championship)
    {
        return championship.Participants.Select(participant => participant.CommunityId).Concat(championship.Teams.SelectMany(team => team.Players())).Distinct();
    }

    private static IEnumerable<long> Leaders(ChampionshipTeam team)
    {
        return new[] { team.CaptainCommunityId, team.ViceCaptainCommunityId }.OfType<long>();
    }

    private static IEnumerable<long> SeriesPlayers(Championship championship, ChampionshipSeries series)
    {
        return new[] { Team(championship, series.TeamAId), Team(championship, series.TeamBId) }.OfType<ChampionshipTeam>().SelectMany(team => team.Players());
    }

    private static ChampionshipTeam? Team(Championship championship, int? teamId)
    {
        return teamId == null ? null : championship.Teams.FirstOrDefault(team => team.Id == teamId);
    }

    private static string TeamName(Championship championship, int? teamId)
    {
        return Team(championship, teamId)?.Name ?? "?";
    }

    private static string Page(Championship championship)
    {
        return $"{AppConsts.ChampionshipsPath}/{championship.Slug}";
    }

    private static string Captain(Championship championship)
    {
        return $"{Page(championship)}/captain";
    }

    private static string Series(Championship championship, ChampionshipSeries series)
    {
        return $"{Page(championship)}/series/{series.Id.ToString(CultureInfo.InvariantCulture)}";
    }

    private static string AdminUrl(Championship championship, string tab)
    {
        return $"{AppConsts.AdminChampionshipsPath}/{championship.Id.ToString(CultureInfo.InvariantCulture)}?tab={tab}";
    }
}
