using L4D2PlayStats.Hub.Core.Modules.Championships.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;

public static class RoundRobinScheduler
{
    public static List<RoundRobinPairing> Rounds(IReadOnlyList<int> teams, int legs)
    {
        var pairings = new List<RoundRobinPairing>();

        if (teams.Count < 2)
            return pairings;

        var slots = teams.Select(team => (int?)team).ToList();

        if (slots.Count % 2 == 1)
            slots.Add(null);

        var count = slots.Count;
        var roundsPerLeg = count - 1;

        for (var round = 0; round < roundsPerLeg; round++)
        {
            for (var index = 0; index < count / 2; index++)
            {
                var home = slots[index];
                var away = slots[count - 1 - index];

                if (home == null || away == null)
                    continue;

                var swap = index == 0 && round % 2 == 1;

                pairings.Add(new RoundRobinPairing(round + 1, swap ? away.Value : home.Value, swap ? home.Value : away.Value));
            }

            var last = slots[count - 1];

            slots.RemoveAt(count - 1);
            slots.Insert(1, last);
        }

        var firstLeg = pairings.ToList();

        for (var leg = 1; leg < Math.Max(1, legs); leg++)
            pairings.AddRange(firstLeg.Select(pairing => leg % 2 == 1
                ? new RoundRobinPairing(pairing.Round + leg * roundsPerLeg, pairing.TeamB, pairing.TeamA)
                : pairing with { Round = pairing.Round + leg * roundsPerLeg }));

        return pairings;
    }
}
