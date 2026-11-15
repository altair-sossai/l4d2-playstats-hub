using L4D2PlayStats.Hub.Web.Enums;
using L4D2PlayStats.Hub.Core.Modules.Seasons;

namespace L4D2PlayStats.Hub.Web.Models;

public class RankingViewModel
{
    public RankingScope Scope { get; init; }
    public Season? Season { get; init; }
    public int? Year { get; init; }
    public bool Current { get; init; }
    public IReadOnlyList<Season> Seasons { get; init; } = [];
    public IReadOnlyList<int> Years { get; init; } = [];
    public IReadOnlyList<RankingRow> Players { get; init; } = [];
    public long? Highlight { get; init; }
}
