using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services;

public class ChampionshipSeriesService(
    HubDbContext dbContext,
    IClock clock,
    IChampionshipLoadService loader,
    IChampionshipProgressService progress,
    IChampionshipLogService log,
    IChampionshipNotificationService notify) : IChampionshipSeriesService
{
    public async Task<List<ChampionshipCandidateMatch>> CandidatesAsync(int seriesId, CancellationToken cancellationToken = default)
    {
        var (championship, _, series) = await loader.SeriesAsync(seriesId, cancellationToken);

        if (!series.HasTeams)
            return [];

        var teamA = championship.Teams.First(team => team.Id == series.TeamAId!.Value).Players();
        var teamB = championship.Teams.First(team => team.Id == series.TeamBId!.Value).Players();
        var everyone = teamA.Concat(teamB).ToList();
        var since = clock.UtcNow.AddDays(-AppConsts.ChampionshipCandidateMatchesInDays);
        var rows = await dbContext.MatchPlayers.AsNoTracking()
            .Where(player => everyone.Contains(player.CommunityId) && (player.TeamA || player.TeamB) && player.Match!.LastFileDate >= since)
            .Select(player => new { player.MatchId, player.CommunityId, player.TeamA })
            .ToListAsync(cancellationToken);

        var matchIds = rows
            .GroupBy(row => row.MatchId)
            .Where(group => group.Any(row => teamA.Contains(row.CommunityId)) && group.Any(row => teamB.Contains(row.CommunityId)))
            .Select(group => group.Key)
            .OrderByDescending(id => id)
            .Take(AppConsts.ChampionshipCandidateMatches)
            .ToList();

        var matches = await dbContext.Matches.AsNoTracking().Where(match => matchIds.Contains(match.Id)).ToListAsync(cancellationToken);
        var linked = await dbContext.ChampionshipGames.AsNoTracking().Where(game => game.MatchId != null && matchIds.Contains(game.MatchId.Value)).Select(game => game.MatchId!.Value).ToListAsync(cancellationToken);

        return matches
            .Select(match =>
            {
                var players = rows.Where(row => row.MatchId == match.Id).ToList();
                var straight = players.Count(row => row.TeamA && teamA.Contains(row.CommunityId) || !row.TeamA && teamB.Contains(row.CommunityId));
                var crossed = players.Count(row => row.TeamA && teamB.Contains(row.CommunityId) || !row.TeamA && teamA.Contains(row.CommunityId));
                var side = straight >= crossed;

                return new ChampionshipCandidateMatch(match.Id, match.EndedAtUtc ?? match.LastFileDate, match.Campaign, match.MapCount,
                    side ? match.TeamAScore : match.TeamBScore, side ? match.TeamBScore : match.TeamAScore, side,
                    players.Count(row => teamA.Contains(row.CommunityId)), players.Count(row => teamB.Contains(row.CommunityId)), linked.Contains(match.Id));
            })
            .OrderByDescending(candidate => candidate.PlayedAtUtc)
            .ToList();
    }

    public async Task LinkMatchAsync(int gameId, int matchId, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, stage, series, game) = await loader.GameAsync(gameId, cancellationToken);

        if (!series.HasTeams)
            throw new ChampionshipException("The series does not have both teams yet.");

        if (await dbContext.ChampionshipGames.AnyAsync(other => other.MatchId == matchId && other.Id != gameId, cancellationToken))
            throw new ChampionshipException("This match is already linked to another game.");

        var match = await dbContext.Matches.AsNoTracking().Include(item => item.Players).FirstOrDefaultAsync(item => item.Id == matchId, cancellationToken) ?? throw new ChampionshipException("The match was not found.");
        var teamA = championship.Teams.First(team => team.Id == series.TeamAId!.Value).Players();
        var teamB = championship.Teams.First(team => team.Id == series.TeamBId!.Value).Players();
        var straight = match.Players.Count(player => player.TeamA && teamA.Contains(player.CommunityId) || player.TeamB && teamB.Contains(player.CommunityId));
        var crossed = match.Players.Count(player => player.TeamA && teamB.Contains(player.CommunityId) || player.TeamB && teamA.Contains(player.CommunityId));

        if (straight == 0 && crossed == 0)
            throw new ChampionshipException("None of the players of the two teams played this match.");

        var side = straight >= crossed;

        game.MatchId = match.Id;
        game.MatchTeamAIsTeamA = side;
        game.TeamAScore = side ? match.TeamAScore : match.TeamBScore;
        game.TeamBScore = side ? match.TeamBScore : match.TeamAScore;
        game.PlayedAtUtc = match.EndedAtUtc ?? match.LastFileDate;
        game.Status = ChampionshipGameStatus.Played;
        game.ResultSource = ChampionshipResultSource.Match;
        game.AutoLinked = false;
        game.LastStatisticsAtUtc = null;

        if (game.CampaignId == null && !string.IsNullOrWhiteSpace(match.Campaign))
        {
            var campaign = (await loader.CampaignsAsync(cancellationToken)).FirstOrDefault(item => string.Equals(item.Name, match.Campaign, StringComparison.OrdinalIgnoreCase));

            if (campaign != null)
            {
                game.CampaignId = campaign.Id;
                game.CampaignSource = ChampionshipCampaignSource.Admin;
            }
        }

        var wasFinished = series.Finished;

        await ApplyAsync(championship, series, cancellationToken);
        log.Add(championship.Id, by, "Linked the match #{0} to game {1} of {2}", match.Id, game.Number, Title(championship, stage, series));
        notify.GameFinished(championship, series, game, wasFinished);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetManualResultAsync(int gameId, int teamAScore, int teamBScore, DateTime? playedAtUtc, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, stage, series, game) = await loader.GameAsync(gameId, cancellationToken);

        if (!series.HasTeams)
            throw new ChampionshipException("The series does not have both teams yet.");

        if (teamAScore < 0 || teamBScore < 0 || teamAScore > AppConsts.MaxScore || teamBScore > AppConsts.MaxScore)
            throw new ChampionshipException("Enter a valid score.");

        game.MatchId = null;
        game.MatchTeamAIsTeamA = true;
        game.TeamAScore = teamAScore;
        game.TeamBScore = teamBScore;
        game.PlayedAtUtc = playedAtUtc ?? clock.UtcNow;
        game.Status = ChampionshipGameStatus.Played;
        game.ResultSource = ChampionshipResultSource.Manual;
        game.AutoLinked = false;
        game.LastStatisticsAtUtc = null;

        var wasFinished = series.Finished;

        await ApplyAsync(championship, series, cancellationToken);
        log.Add(championship.Id, by, "Typed the result {0} x {1} in game {2} of {3}", teamAScore, teamBScore, game.Number, Title(championship, stage, series));
        notify.GameFinished(championship, series, game, wasFinished);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetPenaltiesAsync(int gameId, int teamAPenalty, int teamBPenalty, string? notes, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, stage, series, game) = await loader.GameAsync(gameId, cancellationToken);

        if (teamAPenalty < 0 || teamBPenalty < 0 || teamAPenalty > AppConsts.ChampionshipMaxPenalty || teamBPenalty > AppConsts.ChampionshipMaxPenalty)
            throw new ChampionshipException("Enter the points of the penalty.");


        game.TeamAPenalty = teamAPenalty;
        game.TeamBPenalty = teamBPenalty;
        game.Notes = Text.Clip(notes, AppConsts.ChampionshipNotesMaxLength);

        await ApplyAsync(championship, series, cancellationToken);
        log.Add(championship.Id, by, "Saved the penalties of game {0} of {1}", game.Number, Title(championship, stage, series));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearResultAsync(int gameId, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, stage, series, game) = await loader.GameAsync(gameId, cancellationToken);

        game.UnlinkedMatchId = game.MatchId ?? game.UnlinkedMatchId;
        game.MatchId = null;
        game.AutoLinked = false;
        game.LastStatisticsAtUtc = null;
        game.MatchTeamAIsTeamA = true;
        game.TeamAScore = 0;
        game.TeamBScore = 0;
        game.TeamAPenalty = 0;
        game.TeamBPenalty = 0;
        game.PlayedAtUtc = null;
        game.ResultSource = null;
        game.Status = ChampionshipGameStatus.Pending;

        await ApplyAsync(championship, series, cancellationToken);
        log.Add(championship.Id, by, "Cleared the result of game {0} of {1}", game.Number, Title(championship, stage, series));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetCampaignAsync(int gameId, int? campaignId, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, stage, series, game) = await loader.GameAsync(gameId, cancellationToken);

        if (campaignId != null && (await loader.CampaignsAsync(cancellationToken)).All(campaign => campaign.Id != campaignId))
            throw new ChampionshipException("The campaign was not found.");

        var pickedByTeamA = GamePlan.PickedByTeamA(game, series.BestOf);
        var teamPick = campaignId != null && stage.CampaignRule == ChampionshipCampaignRule.Picks && pickedByTeamA != null && series.HasTeams;

        game.CampaignId = campaignId;
        game.CampaignSource = campaignId == null ? null : teamPick ? ChampionshipCampaignSource.Pick : ChampionshipCampaignSource.Admin;
        game.PickedByTeamId = teamPick ? pickedByTeamA == true ? series.TeamAId : series.TeamBId : null;

        await ApplyAsync(championship, series, cancellationToken);
        log.Add(championship.Id, by, "Set the campaign of game {0} of {1}", game.Number, Title(championship, stage, series));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task PickAsync(int gameId, int campaignId, long communityId, CancellationToken cancellationToken = default)
    {
        var (championship, stage, series, game) = await loader.GameAsync(gameId, cancellationToken);
        var team = Leader(championship, series, communityId);

        Open(championship, series);

        if (stage.CampaignRule != ChampionshipCampaignRule.Picks)
            throw new ChampionshipException("The teams do not choose the campaigns in this stage.");

        var pickedByTeamA = GamePlan.PickedByTeamA(game, series.BestOf);

        if (pickedByTeamA == null || (pickedByTeamA == true ? series.TeamAId : series.TeamBId) != team.Id)
            throw new ChampionshipException("Your team does not choose the campaign of this game.");

        if (game.IsPlayed || game.IsInProgress)
            throw new ChampionshipException("This game was already played.");

        if (stage.Veto && (series.TeamAVetoCampaignId == null || series.TeamBVetoCampaignId == null))
            throw new ChampionshipException("Both teams must veto a campaign first.");

        var allowed = CampaignRules.ForPick(stage, series, game, team.Id, await loader.CampaignsAsync(cancellationToken), Games(championship));

        if (allowed.All(campaign => campaign.Id != campaignId))
            throw new ChampionshipException("Your team cannot choose this campaign.");

        game.CampaignId = campaignId;
        game.CampaignSource = ChampionshipCampaignSource.Pick;
        game.PickedByTeamId = team.Id;

        await ApplyAsync(championship, series, cancellationToken);
        log.Add(championship.Id, communityId, "{0} chose the campaign of game {1} of {2}", team.Name, game.Number, Title(championship, stage, series));
        notify.CampaignPicked(championship, series, game, team.Id);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task VetoAsync(int seriesId, int campaignId, long communityId, CancellationToken cancellationToken = default)
    {
        var (championship, stage, series) = await loader.SeriesAsync(seriesId, cancellationToken);
        var team = Leader(championship, series, communityId);
        var teamA = series.TeamAId == team.Id;

        Open(championship, series);

        if (!stage.Veto)
            throw new ChampionshipException("There are no vetoes in this stage.");

        if (series.Games.Any(game => game.IsPlayed || game.IsInProgress))
            throw new ChampionshipException("The series has already started.");

        if ((teamA ? series.TeamAVetoCampaignId : series.TeamBVetoCampaignId) != null)
            throw new ChampionshipException("Your team has already vetoed a campaign.");

        if (CampaignRules.ForVeto(stage, series, teamA, await loader.CampaignsAsync(cancellationToken)).All(campaign => campaign.Id != campaignId))
            throw new ChampionshipException("Your team cannot veto this campaign.");

        if (teamA)
            series.TeamAVetoCampaignId = campaignId;
        else
            series.TeamBVetoCampaignId = campaignId;

        await ApplyAsync(championship, series, cancellationToken);
        log.Add(championship.Id, communityId, "{0} vetoed a campaign in {1}", team.Name, Title(championship, stage, series));
        notify.Vetoed(championship, series, team.Id);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetVetoAsync(int seriesId, bool teamA, int? campaignId, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, stage, series) = await loader.SeriesAsync(seriesId, cancellationToken);

        if (campaignId != null && series.Games.Any(game => game.CampaignId == campaignId))
            throw new ChampionshipException("This campaign is already chosen for a game of the series.");

        if (teamA)
            series.TeamAVetoCampaignId = campaignId;
        else
            series.TeamBVetoCampaignId = campaignId;

        await ApplyAsync(championship, series, cancellationToken);
        log.Add(championship.Id, by, "Set a veto in {0}", Title(championship, stage, series));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task WalkoverAsync(int seriesId, int? winnerTeamId, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, stage, series) = await loader.SeriesAsync(seriesId, cancellationToken);

        if (winnerTeamId != null && (!series.HasTeams || !series.Has(winnerTeamId.Value)))
            throw new ChampionshipException("Choose one of the two teams of the series.");

        var given = winnerTeamId != null && (!series.Walkover || series.WinnerTeamId != winnerTeamId);

        series.Walkover = winnerTeamId != null;
        series.WinnerTeamId = winnerTeamId;

        await ApplyAsync(championship, series, cancellationToken);
        log.Add(championship.Id, by, winnerTeamId == null ? "Removed the walkover of {0}" : "Gave a walkover in {0}", Title(championship, stage, series));

        if (given)
            notify.Walkover(championship, series);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetScheduleAsync(int seriesId, DateTime? scheduledAtUtc, DateTime? deadlineUtc, string? notes, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, stage, series) = await loader.SeriesAsync(seriesId, cancellationToken);

        var rescheduled = scheduledAtUtc != null && scheduledAtUtc != series.ScheduledAtUtc && series.HasTeams;

        series.ScheduledAtUtc = scheduledAtUtc;
        series.DeadlineUtc = deadlineUtc;
        series.Notes = Text.Clip(notes, AppConsts.ChampionshipNotesMaxLength);
        log.Add(championship.Id, by, "Saved the date of {0}", Title(championship, stage, series));

        if (rescheduled)
            notify.Scheduled(championship, series);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public static string Title(Championship championship, ChampionshipStage stage, ChampionshipSeries series)
    {
        var teamA = championship.Teams.FirstOrDefault(team => team.Id == series.TeamAId)?.Name ?? "?";
        var teamB = championship.Teams.FirstOrDefault(team => team.Id == series.TeamBId)?.Name ?? "?";

        return $"{teamA} x {teamB} ({stage.Name})";
    }

    public static ChampionshipTeam Leader(Championship championship, ChampionshipSeries series, long communityId)
    {
        var team = championship.Teams.FirstOrDefault(item => series.Has(item.Id) && item.IsLeader(communityId));

        return team ?? throw new ChampionshipException("Only the captain or the vice-captain of the teams of this series can do this.");
    }

    public static void Open(Championship championship, ChampionshipSeries series)
    {
        if (championship.Status != ChampionshipStatus.Running)
            throw new ChampionshipException("The championship is not running.");

        if (series.Finished || !series.HasTeams)
            throw new ChampionshipException("The series is not open.");
    }


    private static IEnumerable<ChampionshipGame> Games(Championship championship)
    {
        return championship.Stages.SelectMany(stage => stage.Series).SelectMany(series => series.Games);
    }

    private async Task ApplyAsync(Championship championship, ChampionshipSeries series, CancellationToken cancellationToken)
    {
        progress.Apply(championship, series, await loader.CampaignsAsync(cancellationToken));
        championship.UpdatedAtUtc = clock.UtcNow;
    }


}
