using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;

namespace L4D2PlayStats.Hub.Web.Models.Championships;

public class ChampionshipCaptainModel
{
    public required ChampionshipPageModel Page { get; init; }
    public required ChampionshipTeam Team { get; init; }
    public required bool Captain { get; init; }
    public required List<ChampionshipCaptainTask> Tasks { get; init; }

    public Championship Championship => Page.Championship;

    public string Url => $"{AppConsts.ChampionshipsUrl}/{Championship.Slug}/{AppConsts.ChampionshipCaptainPath}";

    public List<ChampionshipSeries> Series => Page.AllSeries.Where(series => series is { Bye: false } && series.HasTeams && series.Has(Team.Id)).OrderBy(series => series.Finished).ThenBy(series => series.ScheduledAtUtc ?? DateTime.MaxValue).ThenBy(series => series.Round).ToList();

    public List<ChampionshipCaptainTask> TasksOf(ChampionshipSeries series)
    {
        return Tasks.Where(task => task.SeriesId == series.Id).ToList();
    }
}
