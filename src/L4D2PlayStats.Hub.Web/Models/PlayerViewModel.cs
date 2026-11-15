using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Patents.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Core.Modules.Seasons;

namespace L4D2PlayStats.Hub.Web.Models;

public class PlayerViewModel
{
    public long CommunityId { get; init; }
    public string Name { get; init; } = null!;
    public Season Season { get; init; } = null!;
    public IReadOnlyList<Season> Seasons { get; init; } = [];
    public RankingPlayer? Ranked { get; init; }
    public PatentProgress? Progress { get; init; }
    public RankingPlayer? AllTime { get; init; }
    public IReadOnlyList<Match> Matches { get; init; } = [];
    public IReadOnlyDictionary<long, string> Names { get; init; } = new Dictionary<long, string>();
    public int Stars { get; init; }
    public bool Administrator { get; init; }
}
