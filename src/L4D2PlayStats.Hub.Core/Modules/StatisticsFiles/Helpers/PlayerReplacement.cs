using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Models;
using ParsedStatistics = L4D2PlayStats.Statistics;
using SteamUser = L4D2PlayStats.Steam.SteamUser;

namespace L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Helpers;

// Puts one Steam account in the place of another on one line of a statistics file: a survivor or an infected of one
// round half, for a substitute who played it. Every number of that line changes owner. [PlayerNames:] is where the site
// takes the names from and how it puts a survivor on a team, so the new player is added there when the file does not
// list them yet.
public static class PlayerReplacement
{
    public static PlayerReplacementError? Apply(ParsedStatistics statistics, int half, bool survivor, long replaced, long replacement, string name)
    {
        if (half < 0 || half >= statistics.Halves.Count)
            return PlayerReplacementError.NotInHalf;

        var from = Id(replaced);
        var to = Id(replacement);
        var players = survivor
            ? statistics.Halves[half].Players.Cast<SteamUser>().ToList()
            : statistics.Halves[half].InfectedPlayers.Cast<SteamUser>().ToList();

        var line = players.FirstOrDefault(player => player.CommunityId == from);
        if (line == null)
            return PlayerReplacementError.NotInHalf;

        if (players.Any(player => player.CommunityId == to))
            return PlayerReplacementError.AlreadyInHalf;

        var team = Team(statistics, half, survivor);
        if (Teams(statistics, replacement).Any(other => other != team))
            return PlayerReplacementError.OtherTeam;

        var steamId = SteamIds.SteamId(replacement);

        line.SteamId = steamId;

        if (statistics.PlayerNames.All(playerName => playerName.CommunityId != to))
        {
            var index = statistics.PlayerNames.Count == 0 ? 1 : statistics.PlayerNames.Max(playerName => playerName.Index) + 1;

            statistics.PlayerNames.Add(new L4D2PlayStats.PlayerName { Index = index, SteamId = steamId, Name = name });
        }

        return null;
    }

    // The name goes on a line of its own in the file, so a line break would cut the file.
    public static string? CleanName(string? name)
    {
        var clean = name?.Replace('\r', ' ').Replace('\n', ' ').Trim();

        return string.IsNullOrEmpty(clean) ? null : clean;
    }

    // The survivors of a round half are its team; the infected are the other one.
    private static char? Team(ParsedStatistics statistics, int half, bool survivor)
    {
        var team = statistics.Halves[half].RoundHalf?.Team ?? statistics.Halves[half].Progress?.Team;

        return survivor ? team : Opposite(team);
    }

    private static IEnumerable<char?> Teams(ParsedStatistics statistics, long communityId)
    {
        var id = Id(communityId);

        for (var half = 0; half < statistics.Halves.Count; half++)
        {
            if (statistics.Halves[half].Players.Any(player => player.CommunityId == id))
                yield return Team(statistics, half, true);

            if (statistics.Halves[half].InfectedPlayers.Any(player => player.CommunityId == id))
                yield return Team(statistics, half, false);
        }
    }

    private static char? Opposite(char? team)
    {
        return team switch
        {
            AppConsts.TeamACode => AppConsts.TeamBCode,
            AppConsts.TeamBCode => AppConsts.TeamACode,
            _ => null
        };
    }

    private static string Id(long communityId)
    {
        return communityId.ToString(CultureInfo.InvariantCulture);
    }
}
