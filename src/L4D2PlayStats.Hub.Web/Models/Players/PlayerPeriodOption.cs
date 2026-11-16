using L4D2PlayStats.Hub.Core.Modules.Seasons;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerPeriodOption
{
    public Season? Season { get; init; }
    public int? Year { get; init; }
    public string? Query { get; init; }
}
