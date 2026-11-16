using L4D2PlayStats.Hub.Core.Modules.Patents;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;

namespace L4D2PlayStats.Hub.Web.Models.Ranking;

public class PatentModel
{
    public RankingSettings Config { get; init; } = null!;
    public int SeasonLengthInMonths { get; init; }
    public IReadOnlyList<Patent> Patents { get; init; } = [];
}
