using L4D2PlayStats.Hub.Core.Modules.Matches;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerMatchTeamModel(char code, int score, IEnumerable<MatchPlayer> players)
{
    public char Code { get; } = code;
    public int Score { get; } = score;

    public IReadOnlyList<MatchPlayer> Players { get; } =
    [
        .. players
            .OrderByDescending(player => player.MvpSiDamage + player.MvpCommon)
            .ThenByDescending(player => player.MvpSiDamage)
            .ThenByDescending(player => player.MvpCommon)
            .ThenBy(player => player.LvpFfGiven)
    ];
}
