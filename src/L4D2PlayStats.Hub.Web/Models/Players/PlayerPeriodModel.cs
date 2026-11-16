using L4D2PlayStats.Hub.Core.Modules.Seasons;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerPeriodModel
{
    public required Season Season { get; init; }
    public required Season CurrentSeason { get; init; }
    public required IReadOnlyList<Season> Seasons { get; init; }
    public bool IsCurrent => Season.Id == CurrentSeason.Id;
    public int? SeasonId => IsCurrent ? null : Season.Id;
    public bool HasChoices => Seasons.Count > 1;

    public int? IdOf(Season season)
    {
        return season.Id == CurrentSeason.Id ? null : season.Id;
    }
}
