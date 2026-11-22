using ParsedStatistics = L4D2PlayStats.Statistics;

namespace L4D2PlayStats.Hub.Core.Modules.Matches.Helpers;

// The statistics file has no field for a tank wipe, so it is deduced from what the plugin writes for each half:
// - the progress line has how many survivors were standing when the round ended, and 0 is a wipe;
// - the round stops the Tank's clock when the Tank dies, or at the end of the round when a Tank is still up,
//   in the same instant it stops the round's clock. So a Tank that was alive when the survivors went down ends
//   in the same second as the round, and a Tank that died before ends earlier.
// The wipe goes to the infected player who did the most damage as Tank in that half. A Tank driven by a bot,
// or one that never hit a survivor, gives no tank wipe.
public static class TankWipes
{
    public static string? Player(ParsedStatistics.Half half)
    {
        var round = half.RoundHalf;

        if (half.Progress is not { Survived: 0 } || round?.StartTimeTank == null || round.EndTime == null || round.StopTimeTank != round.EndTime)
            return null;

        return half.InfectedPlayers
            .Where(player => !string.IsNullOrEmpty(player.CommunityId) && player.DmgTank + player.DmgTankIncap > 0)
            .OrderByDescending(player => player.DmgTank + player.DmgTankIncap)
            .FirstOrDefault()?
            .CommunityId;
    }

    public static Dictionary<long, int> Count(IEnumerable<ParsedStatistics> statistics)
    {
        var counts = new Dictionary<long, int>();

        foreach (var half in statistics.SelectMany(file => file.Halves))
        {
            if (!long.TryParse(Player(half), out var communityId))
                continue;

            counts[communityId] = counts.GetValueOrDefault(communityId) + 1;
        }

        return counts;
    }
}
