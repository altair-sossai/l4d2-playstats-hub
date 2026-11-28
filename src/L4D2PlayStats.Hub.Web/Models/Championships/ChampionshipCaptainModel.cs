using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Web.Helpers;

namespace L4D2PlayStats.Hub.Web.Models.Championships;

public class ChampionshipCaptainModel
{
    public required ChampionshipPageModel Page { get; init; }
    public required ChampionshipTeam Team { get; init; }
    public required bool Captain { get; init; }
    public required List<ChampionshipCaptainTask> Tasks { get; init; }
    public required PagedListModel<long> Free { get; init; }

    public Championship Championship => Page.Championship;

    public string Url => ChampionshipUrls.Captain(Championship.Slug);

    public List<ChampionshipSeries> Series => Page.AllSeries.Where(series => series is { Bye: false } && series.HasTeams && series.Has(Team.Id)).OrderBy(series => series.Finished).ThenBy(series => series.ScheduledAtUtc ?? DateTime.MaxValue).ThenBy(series => series.Round).ToList();

    public List<ChampionshipCaptainTask> TasksOf(ChampionshipSeries series)
    {
        return Tasks.Where(task => task.SeriesId == series.Id).ToList();
    }
}
