namespace L4D2PlayStats.Hub.Web.Models;

public class DashboardViewModel
{
    public IReadOnlyList<ServerStatusViewModel> Servers { get; init; } = [];
    public int Files { get; init; }
    public int Matches { get; init; }
    public int RankedMatches { get; init; }
    public int Players { get; init; }
    public int Seasons { get; init; }
    public int UnmatchedConfigurations { get; init; }
}