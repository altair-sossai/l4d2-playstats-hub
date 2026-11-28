using L4D2PlayStats.Hub.Core.Modules.Championships.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

public interface IChampionshipLoadService
{
    Task<Championship?> LoadAsync(int id, bool tracking, CancellationToken cancellationToken = default);
    Task<Championship?> LoadBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Championship?> LoadByTeamAsync(int teamId, CancellationToken cancellationToken = default);
    Task<Championship?> LoadByStageAsync(int stageId, CancellationToken cancellationToken = default);
    Task<Championship?> LoadBySeriesAsync(int seriesId, CancellationToken cancellationToken = default);
    Task<Championship?> LoadByGameAsync(int gameId, CancellationToken cancellationToken = default);
    Task<(Championship Championship, ChampionshipTeam Team)> TeamAsync(int teamId, CancellationToken cancellationToken = default);
    Task<(Championship Championship, ChampionshipStage Stage, ChampionshipSeries Series)> SeriesAsync(int seriesId, CancellationToken cancellationToken = default);
    Task<(Championship Championship, ChampionshipStage Stage, ChampionshipSeries Series, ChampionshipGame Game)> GameAsync(int gameId, CancellationToken cancellationToken = default);
    Task<List<ChampionshipCampaignOption>> CampaignsAsync(CancellationToken cancellationToken = default);
}
