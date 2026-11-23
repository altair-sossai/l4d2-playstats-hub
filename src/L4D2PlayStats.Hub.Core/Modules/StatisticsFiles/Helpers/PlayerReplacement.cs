using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using ParsedStatistics = L4D2PlayStats.Statistics;

namespace L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Helpers;

// Puts one Steam account in the place of another in a statistics file, for a substitute who came in for a player:
// the survivor and infected lines of the player who left get the Steam ID of the one who came in, so every number of
// those lines changes owner. [PlayerNames:] is where the site takes the names from and how it puts a survivor on a
// team, so the player who came in is added there when the file does not list them yet.
public static class PlayerReplacement
{
    public static bool Plays(ParsedStatistics statistics, long communityId)
    {
        var id = Id(communityId);

        return statistics.Halves.Any(half => half.Players.Any(player => player.CommunityId == id) || half.InfectedPlayers.Any(player => player.CommunityId == id));
    }

    public static void Apply(ParsedStatistics statistics, long replaced, long replacement, string name)
    {
        var from = Id(replaced);
        var to = Id(replacement);
        var steamId = SteamIds.SteamId(replacement);

        foreach (var half in statistics.Halves)
        {
            foreach (var player in half.Players.Where(player => player.CommunityId == from))
                player.SteamId = steamId;

            foreach (var player in half.InfectedPlayers.Where(player => player.CommunityId == from))
                player.SteamId = steamId;
        }

        if (statistics.PlayerNames.Any(playerName => playerName.CommunityId == to))
            return;

        var index = statistics.PlayerNames.Count == 0 ? 1 : statistics.PlayerNames.Max(playerName => playerName.Index) + 1;

        statistics.PlayerNames.Add(new L4D2PlayStats.PlayerName { Index = index, SteamId = steamId, Name = name });
    }

    // The name goes on a line of its own in the file, so a line break would cut the file.
    public static string? CleanName(string? name)
    {
        var clean = name?.Replace('\r', ' ').Replace('\n', ' ').Trim();

        return string.IsNullOrEmpty(clean) ? null : clean;
    }

    private static string Id(long communityId)
    {
        return communityId.ToString(CultureInfo.InvariantCulture);
    }
}
