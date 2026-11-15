using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;

namespace L4D2PlayStats.Hub.Web.Models;

public class MatchViewModel
{
    public Match Match { get; init; } = null!;
    public IReadOnlyList<RankingPlayer> Ranking { get; init; } = [];
    public bool Administrator { get; init; }

    public IEnumerable<MatchPlayer> Team(char team)
    {
        return Match.Players.Where(p => team == 'A' ? p.TeamA : p.TeamB).OrderByDescending(p => p.SiDamage);
    }

    public IEnumerable<MatchPlayer> Others => Match.Players.Where(p => !p.TeamA && !p.TeamB && p.RageQuits > 0);
}
