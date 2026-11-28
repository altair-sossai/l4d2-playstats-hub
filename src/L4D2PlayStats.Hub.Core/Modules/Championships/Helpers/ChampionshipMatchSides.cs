using L4D2PlayStats.Hub.Core.Modules.Matches;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;

public static class ChampionshipMatchSides
{
    public static bool? Detect(IEnumerable<MatchPlayer> players, IReadOnlySet<long> teamA, IReadOnlySet<long> teamB, int playersPerGame)
    {
        var list = players.Where(player => player.TeamA || player.TeamB).ToList();
        var needed = Math.Max(1, (playersPerGame + 1) / 2);

        if (Fits(list, teamA, teamB, needed))
            return true;

        if (Fits(list, teamB, teamA, needed))
            return false;

        return null;
    }

    private static bool Fits(List<MatchPlayer> players, IReadOnlySet<long> sideA, IReadOnlySet<long> sideB, int needed)
    {
        var onA = players.Count(player => player.TeamA && sideA.Contains(player.CommunityId));
        var onB = players.Count(player => player.TeamB && sideB.Contains(player.CommunityId));
        var wrong = players.Count(player => player.TeamA && sideB.Contains(player.CommunityId) || player.TeamB && sideA.Contains(player.CommunityId));

        return onA >= needed && onB >= needed && wrong < Math.Min(onA, onB);
    }
}
