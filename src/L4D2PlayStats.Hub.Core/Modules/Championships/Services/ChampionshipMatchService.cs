using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services;

public class ChampionshipMatchService(
    HubDbContext dbContext,
    IClock clock,
    ISettingsService settingsService,
    IChampionshipLoadService loader,
    IChampionshipProgressService progress,
    IChampionshipLogService log,
    ILogger<ChampionshipMatchService> logger) : IChampionshipMatchService
{
    public async Task SyncAsync(IReadOnlyCollection<int> matchIds, CancellationToken cancellationToken = default)
    {
        if (!await EnabledAsync(cancellationToken))
            return;

        await ReleaseOrphansAsync(cancellationToken);

        foreach (var matchId in matchIds.Distinct())
        {
            try
            {
                await SyncAsync(matchId, cancellationToken);
            }
            catch (ChampionshipException exception)
            {
                dbContext.ChangeTracker.Clear();
                logger.LogWarning("Could not sync the match {Match} with the championships: {Reason}", matchId, exception.Message);
            }
        }
    }

    public async Task<int> CloseIdleAsync(CancellationToken cancellationToken = default)
    {
        if (!await EnabledAsync(cancellationToken))
            return 0;

        var now = clock.UtcNow;
        var games = await dbContext.ChampionshipGames.AsNoTracking()
            .Where(game => game.Status == ChampionshipGameStatus.InProgress)
            .Join(dbContext.ChampionshipSeries, game => game.SeriesId, series => series.Id, (game, series) => new { game.Id, game.LastStatisticsAtUtc, series.Stage!.ChampionshipId })
            .Join(dbContext.Championships, item => item.ChampionshipId, championship => championship.Id, (item, championship) => new { item.Id, item.LastStatisticsAtUtc, championship.AutoCloseMinutes })
            .ToListAsync(cancellationToken);

        var closed = 0;

        foreach (var game in games.Where(game => game.LastStatisticsAtUtc == null || game.LastStatisticsAtUtc.Value.AddMinutes(game.AutoCloseMinutes) <= now))
        {
            try
            {
                var (championship, stage, series, item) = await LoadGameAsync(game.Id, cancellationToken);

                await FinishAsync(championship, stage, series, item, null, ChampionshipGameEnd.Idle, game.AutoCloseMinutes, cancellationToken);
                closed++;
            }
            catch (ChampionshipException exception)
            {
                dbContext.ChangeTracker.Clear();
                logger.LogWarning("Could not close the championship game {Game}: {Reason}", game.Id, exception.Message);
            }
        }

        return closed;
    }

    public async Task CloseAsync(int gameId, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, stage, series, game) = await LoadGameAsync(gameId, cancellationToken);

        if (!game.IsInProgress)
            throw new ChampionshipException("The game is not in progress.");

        await FinishAsync(championship, stage, series, game, by, ChampionshipGameEnd.Administrator, 0, cancellationToken);
    }

    private async Task SyncAsync(int matchId, CancellationToken cancellationToken)
    {
        var match = await dbContext.Matches.AsNoTracking().Include(item => item.Players).FirstOrDefaultAsync(item => item.Id == matchId, cancellationToken);

        if (match == null)
            return;

        var linked = await dbContext.ChampionshipGames.AsNoTracking().Where(game => game.MatchId == matchId).Select(game => (int?)game.Id).FirstOrDefaultAsync(cancellationToken);

        if (linked != null)
        {
            await UpdateAsync(linked.Value, match, cancellationToken);

            return;
        }

        var gameId = await FindAsync(match, cancellationToken);

        if (gameId != null)
            await LinkAsync(gameId.Value, match, cancellationToken);
    }

    private async Task<int?> FindAsync(Match match, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(match.Campaign))
            return null;

        var campaigns = await loader.CampaignsAsync(cancellationToken);
        var campaignIds = campaigns.Where(campaign => string.Equals(campaign.Name, match.Campaign, StringComparison.OrdinalIgnoreCase)).Select(campaign => campaign.Id).ToList();

        if (campaignIds.Count == 0)
            return null;

        var started = match.StartedAtUtc ?? match.FirstFileDate;
        var file = match.FirstFileDate;
        var hours = AppConsts.ChampionshipMaxAutoLinkHours;
        var (startedFrom, startedTo, fileFrom, fileTo) = (started.AddHours(-hours), started.AddHours(hours), file.AddHours(-hours), file.AddHours(hours));

        var candidates = await dbContext.ChampionshipGames.AsNoTracking()
            .Where(game => game.Status == ChampionshipGameStatus.Pending && game.MatchId == null && game.CampaignId != null && campaignIds.Contains(game.CampaignId.Value) && game.UnlinkedMatchId != match.Id)
            .Join(dbContext.ChampionshipSeries, game => game.SeriesId, series => series.Id, (game, series) => new { Game = game, Series = series })
            .Where(item => !item.Series.Finished && !item.Series.Walkover && item.Series.TeamAId != null && item.Series.TeamBId != null && item.Series.ScheduledAtUtc != null)
            .Where(item => item.Series.ScheduledAtUtc >= startedFrom && item.Series.ScheduledAtUtc <= startedTo || item.Series.ScheduledAtUtc >= fileFrom && item.Series.ScheduledAtUtc <= fileTo)
            .Join(dbContext.Championships, item => item.Series.Stage!.ChampionshipId, championship => championship.Id, (item, championship) => new { GameId = item.Game.Id, item.Game.Number, item.Series.ScheduledAtUtc, item.Series.TeamAId, item.Series.TeamBId, ChampionshipId = championship.Id, championship.Status, championship.AutoLinkMatches, championship.AutoLinkHours, championship.PlayersPerGame })
            .Where(item => item.Status == ChampionshipStatus.Running && item.AutoLinkMatches)
            .ToListAsync(cancellationToken);

        var found = new List<(int GameId, int Number, DateTime Scheduled)>();

        foreach (var candidate in candidates.Where(item => Distance(item.ScheduledAtUtc!.Value, match).TotalHours <= item.AutoLinkHours))
        {
            var members = await dbContext.ChampionshipTeamMembers.AsNoTracking()
                .Where(member => member.TeamId == candidate.TeamAId || member.TeamId == candidate.TeamBId)
                .Select(member => new { member.TeamId, member.CommunityId })
                .ToListAsync(cancellationToken);

            var teamA = members.Where(member => member.TeamId == candidate.TeamAId).Select(member => member.CommunityId).ToHashSet();
            var teamB = members.Where(member => member.TeamId == candidate.TeamBId).Select(member => member.CommunityId).ToHashSet();

            if (ChampionshipMatchSides.Detect(match.Players, teamA, teamB, candidate.PlayersPerGame) != null)
                found.Add((candidate.GameId, candidate.Number, candidate.ScheduledAtUtc!.Value));
        }

        return found
            .OrderBy(item => Distance(item.Scheduled, match))
            .ThenBy(item => item.Number)
            .Select(item => (int?)item.GameId)
            .FirstOrDefault();
    }

    private static TimeSpan Distance(DateTime scheduled, Match match)
    {
        var file = (scheduled - match.FirstFileDate).Duration();

        return match.StartedAtUtc == null ? file : TimeSpan.FromTicks(Math.Min(file.Ticks, (scheduled - match.StartedAtUtc.Value).Duration().Ticks));
    }

    private async Task LinkAsync(int gameId, Match match, CancellationToken cancellationToken)
    {
        var (championship, stage, series, game) = await LoadGameAsync(gameId, cancellationToken);
        var side = Side(championship, series, match) ?? throw new ChampionshipException("None of the players of the two teams played this match.");
        var campaigns = await loader.CampaignsAsync(cancellationToken);

        game.MatchId = match.Id;
        game.MatchTeamAIsTeamA = side;
        game.ResultSource = ChampionshipResultSource.Match;
        game.Status = ChampionshipGameStatus.InProgress;
        game.AutoLinked = true;
        Score(game, match);

        log.Add(championship.Id, null, "Linked the match #{0} to game {1} of {2} by itself", match.Id, game.Number, ChampionshipSeriesService.Title(championship, stage, series));

        if (Complete(game, match, campaigns))
        {
            await FinishAsync(championship, stage, series, game, null, ChampionshipGameEnd.Maps, 0, cancellationToken);

            return;
        }

        championship.UpdatedAtUtc = clock.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpdateAsync(int gameId, Match match, CancellationToken cancellationToken)
    {
        var (championship, stage, series, game) = await LoadGameAsync(gameId, cancellationToken);
        var campaigns = await loader.CampaignsAsync(cancellationToken);
        var before = (game.TeamAScore, game.TeamBScore);

        Score(game, match);

        if (game.IsInProgress)
        {
            if (Complete(game, match, campaigns))
            {
                await FinishAsync(championship, stage, series, game, null, ChampionshipGameEnd.Maps, 0, cancellationToken);

                return;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return;
        }

        if (!game.IsPlayed || before == (game.TeamAScore, game.TeamBScore))
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            return;
        }

        progress.Apply(championship, series, campaigns);
        championship.UpdatedAtUtc = clock.UtcNow;
        log.Add(championship.Id, null, "Updated the result of game {0} of {1} from the match #{2}", game.Number, ChampionshipSeriesService.Title(championship, stage, series), match.Id);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task FinishAsync(Championship championship, ChampionshipStage stage, ChampionshipSeries series, ChampionshipGame game, long? by, ChampionshipGameEnd end, int minutes, CancellationToken cancellationToken)
    {
        var title = ChampionshipSeriesService.Title(championship, stage, series);

        game.Status = ChampionshipGameStatus.Played;
        progress.Apply(championship, series, await loader.CampaignsAsync(cancellationToken));
        championship.UpdatedAtUtc = clock.UtcNow;

        switch (end)
        {
            case ChampionshipGameEnd.Idle:
                log.Add(championship.Id, by, "Game {0} of {1} ended: no statistics for {2} minutes", game.Number, title, minutes);
                break;
            case ChampionshipGameEnd.Maps:
                log.Add(championship.Id, by, "Game {0} of {1} ended: every map was played", game.Number, title);
                break;
            default:
                log.Add(championship.Id, by, "Ended game {0} of {1}", game.Number, title);
                break;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ReleaseOrphansAsync(CancellationToken cancellationToken)
    {
        var orphans = await dbContext.ChampionshipGames.AsNoTracking()
            .Where(game => game.Status == ChampionshipGameStatus.InProgress && game.MatchId == null)
            .Select(game => game.Id)
            .ToListAsync(cancellationToken);

        foreach (var orphan in orphans)
        {
            var (championship, _, series, game) = await LoadGameAsync(orphan, cancellationToken);

            game.Status = ChampionshipGameStatus.Pending;
            game.ResultSource = null;
            game.AutoLinked = false;
            game.TeamAScore = 0;
            game.TeamBScore = 0;
            game.PlayedAtUtc = null;
            game.LastStatisticsAtUtc = null;
            progress.Apply(championship, series, await loader.CampaignsAsync(cancellationToken));
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private void Score(ChampionshipGame game, Match match)
    {
        game.TeamAScore = game.MatchTeamAIsTeamA ? match.TeamAScore : match.TeamBScore;
        game.TeamBScore = game.MatchTeamAIsTeamA ? match.TeamBScore : match.TeamAScore;
        game.PlayedAtUtc = match.EndedAtUtc ?? match.LastFileDate;
        game.LastStatisticsAtUtc = clock.UtcNow;
    }

    private static bool Complete(ChampionshipGame game, Match match, IReadOnlyList<ChampionshipCampaignOption> campaigns)
    {
        var maps = campaigns.FirstOrDefault(campaign => campaign.Id == game.CampaignId)?.Maps ?? 0;

        return maps > 0 && match.MapCount >= maps;
    }

    private static bool? Side(Championship championship, ChampionshipSeries series, Match match)
    {
        var teamA = championship.Teams.First(team => team.Id == series.TeamAId).Members.Select(member => member.CommunityId).ToHashSet();
        var teamB = championship.Teams.First(team => team.Id == series.TeamBId).Members.Select(member => member.CommunityId).ToHashSet();

        return ChampionshipMatchSides.Detect(match.Players, teamA, teamB, championship.PlayersPerGame);
    }

    private async Task<bool> EnabledAsync(CancellationToken cancellationToken)
    {
        return (await settingsService.GetAsync<ModuleSettings>(cancellationToken)).IsEnabled(HubModule.Championships);
    }

    private async Task<(Championship, ChampionshipStage, ChampionshipSeries, ChampionshipGame)> LoadGameAsync(int gameId, CancellationToken cancellationToken)
    {
        var championship = await loader.LoadByGameAsync(gameId, cancellationToken) ?? throw new ChampionshipException("The game was not found.");
        var stage = championship.Stages.First(item => item.Series.Any(series => series.Games.Any(game => game.Id == gameId)));
        var series = stage.Series.First(item => item.Games.Any(game => game.Id == gameId));

        return (championship, stage, series, series.Games.First(game => game.Id == gameId));
    }
}
