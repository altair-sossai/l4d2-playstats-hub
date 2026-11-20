namespace L4D2PlayStats.Hub.Core.Modules.Matches.Helpers;

public static class MatchIdentity
{
    public static Dictionary<Match, Match> Pair(IReadOnlyList<Match> built, IReadOnlyList<Match> stored)
    {
        var pairs = new Dictionary<Match, Match>();
        var free = stored.ToList();

        Pair(pairs, built, free, (match, candidate) => candidate.FirstFileDate == match.FirstFileDate);
        Pair(pairs, built, free, (match, candidate) => candidate.LastFileDate == match.LastFileDate);

        return pairs;
    }

    private static void Pair(Dictionary<Match, Match> pairs, IReadOnlyList<Match> built, List<Match> free, Func<Match, Match, bool> same)
    {
        foreach (var match in built.Where(match => !pairs.ContainsKey(match)))
        {
            var kept = free.FirstOrDefault(candidate => SameMode(match, candidate) && same(match, candidate));
            if (kept == null)
                continue;

            pairs.Add(match, kept);
            free.Remove(kept);
        }
    }

    private static bool SameMode(Match match, Match candidate)
    {
        return candidate.ConfigurationName == match.ConfigurationName && candidate.TeamSize == match.TeamSize;
    }
}