using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services;

public class ChampionshipLoadService(HubDbContext dbContext) : IChampionshipLoadService
{
    public async Task<Championship?> LoadAsync(int id, bool tracking, CancellationToken cancellationToken = default)
    {
        var query = Query(tracking).Where(championship => championship.Id == id);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Championship?> LoadBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var id = await dbContext.Championships.AsNoTracking().Where(championship => championship.Slug == slug).Select(championship => (int?)championship.Id).FirstOrDefaultAsync(cancellationToken);

        return id == null ? null : await LoadAsync(id.Value, false, cancellationToken);
    }

    public async Task<Championship?> LoadByTeamAsync(int teamId, CancellationToken cancellationToken = default)
    {
        var id = await dbContext.ChampionshipTeams.AsNoTracking().Where(team => team.Id == teamId).Select(team => (int?)team.ChampionshipId).FirstOrDefaultAsync(cancellationToken);

        return id == null ? null : await LoadAsync(id.Value, true, cancellationToken);
    }

    public async Task<Championship?> LoadByStageAsync(int stageId, CancellationToken cancellationToken = default)
    {
        var id = await dbContext.ChampionshipStages.AsNoTracking().Where(stage => stage.Id == stageId).Select(stage => (int?)stage.ChampionshipId).FirstOrDefaultAsync(cancellationToken);

        return id == null ? null : await LoadAsync(id.Value, true, cancellationToken);
    }

    public async Task<Championship?> LoadBySeriesAsync(int seriesId, CancellationToken cancellationToken = default)
    {
        var id = await dbContext.ChampionshipSeries.AsNoTracking().Where(series => series.Id == seriesId).Select(series => (int?)series.Stage!.ChampionshipId).FirstOrDefaultAsync(cancellationToken);

        return id == null ? null : await LoadAsync(id.Value, true, cancellationToken);
    }

    public async Task<Championship?> LoadByGameAsync(int gameId, CancellationToken cancellationToken = default)
    {
        var seriesId = await dbContext.ChampionshipGames.AsNoTracking().Where(game => game.Id == gameId).Select(game => (int?)game.SeriesId).FirstOrDefaultAsync(cancellationToken);

        return seriesId == null ? null : await LoadBySeriesAsync(seriesId.Value, cancellationToken);
    }

    public async Task<(Championship Championship, ChampionshipTeam Team)> TeamAsync(int teamId, CancellationToken cancellationToken = default)
    {
        var championship = await LoadByTeamAsync(teamId, cancellationToken) ?? throw new ChampionshipException("The team was not found.");

        return (championship, championship.Teams.First(team => team.Id == teamId));
    }

    public async Task<(Championship Championship, ChampionshipStage Stage, ChampionshipSeries Series)> SeriesAsync(int seriesId, CancellationToken cancellationToken = default)
    {
        var championship = await LoadBySeriesAsync(seriesId, cancellationToken) ?? throw new ChampionshipException("The series was not found.");
        var stage = championship.Stages.First(item => item.Series.Any(series => series.Id == seriesId));

        return (championship, stage, stage.Series.First(series => series.Id == seriesId));
    }

    public async Task<(Championship Championship, ChampionshipStage Stage, ChampionshipSeries Series, ChampionshipGame Game)> GameAsync(int gameId, CancellationToken cancellationToken = default)
    {
        var championship = await LoadByGameAsync(gameId, cancellationToken) ?? throw new ChampionshipException("The game was not found.");
        var stage = championship.Stages.First(item => item.Series.Any(series => series.Games.Any(game => game.Id == gameId)));
        var series = stage.Series.First(item => item.Games.Any(game => game.Id == gameId));

        return (championship, stage, series, series.Games.First(game => game.Id == gameId));
    }

    public async Task<List<ChampionshipCampaignOption>> CampaignsAsync(CancellationToken cancellationToken = default)
    {
        var campaigns = await dbContext.Campaigns.AsNoTracking()
            .Select(campaign => new { campaign.Id, campaign.Name, Maps = campaign.Maps.Count })
            .ToListAsync(cancellationToken);

        return campaigns.Select(campaign => new ChampionshipCampaignOption(campaign.Id, campaign.Name, campaign.Maps)).OrderBy(campaign => campaign.Name).ToList();
    }

    private IQueryable<Championship> Query(bool tracking)
    {
        var query = dbContext.Championships
            .Include(championship => championship.Participants)
            .Include(championship => championship.Teams).ThenInclude(team => team.Members)
            .Include(championship => championship.DraftPicks)
            .Include(championship => championship.Stages).ThenInclude(stage => stage.Groups).ThenInclude(group => group.Teams)
            .Include(championship => championship.Stages).ThenInclude(stage => stage.Series).ThenInclude(series => series.Games)
            .Include(championship => championship.Stages).ThenInclude(stage => stage.Series).ThenInclude(series => series.Proposals)
            .Include(championship => championship.Stages).ThenInclude(stage => stage.Penalties)
            .AsSplitQuery();

        return tracking ? query : query.AsNoTracking();
    }
}
