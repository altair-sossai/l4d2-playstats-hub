using L4D2PlayStats.Hub.Core.Modules.Seasons;
using L4D2PlayStats.Hub.Web.Helpers;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerPeriodModel
{
    public Season? Season { get; init; }
    public int? Year { get; init; }
    public required Season CurrentSeason { get; init; }
    public required IReadOnlyList<PlayerPeriodOption> Options { get; init; }
    public bool IsCurrent => Season != null && Season.Id == CurrentSeason.Id;
    public bool HasChoices => Options.Count > 1;

    public string? Query
    {
        get
        {
            if (Year != null)
                return PlayerUrls.YearQuery(Year.Value);

            return Season == null || IsCurrent ? null : PlayerUrls.SeasonQuery(Season.Id);
        }
    }
}
