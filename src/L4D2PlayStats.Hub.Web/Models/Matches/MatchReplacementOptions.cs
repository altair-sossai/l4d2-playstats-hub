using L4D2PlayStats.Hub.Core.Modules.Matches.Models;

namespace L4D2PlayStats.Hub.Web.Models.Matches;

public record MatchReplacementMap(int FileId, int Number, string MapName, IReadOnlyList<string> Players);

public record MatchReplacementPlayer(string CommunityId, string Name);

// What the "Replace a player" window offers: every map of the match with who played it, as survivor or infected,
// and everyone who played at least one map. A player who left before the last map is not in the team tables, so
// the list comes from the maps and not from the teams.
public class MatchReplacementOptions
{
    public MatchReplacementOptions(MatchResult match, IReadOnlyList<MatchStatisticsResult> statistics)
    {
        var names = new Dictionary<string, string>();

        Maps = [.. statistics.Select((result, index) => new MatchReplacementMap(result.FileId, index + 1, result.Statistic?.GameRound?.MapName ?? result.FileName, MapPlayers(result)))];

        foreach (var playerName in statistics.SelectMany(result => result.Statistic?.PlayerNames ?? []).Where(playerName => !string.IsNullOrEmpty(playerName.CommunityId) && !string.IsNullOrEmpty(playerName.Name)))
            names.TryAdd(playerName.CommunityId!, playerName.Name!);

        Players = [.. Maps
            .SelectMany(map => map.Players)
            .Distinct()
            .Select(id => new MatchReplacementPlayer(id, Name(match, names, id)))
            .OrderBy(player => player.Name, StringComparer.OrdinalIgnoreCase)];
    }

    public IReadOnlyList<MatchReplacementMap> Maps { get; }
    public IReadOnlyList<MatchReplacementPlayer> Players { get; }

    private static List<string> MapPlayers(MatchStatisticsResult result)
    {
        return [.. (result.Statistic?.Halves ?? [])
            .SelectMany(half => half.Players.Select(player => player.CommunityId).Concat(half.InfectedPlayers.Select(player => player.CommunityId)))
            .OfType<string>()
            .Where(id => id.Length > 0)
            .Distinct()];
    }

    private static string Name(MatchResult match, Dictionary<string, string> names, string id)
    {
        if (long.TryParse(id, out var communityId) && match.PlayerNames.TryGetValue(communityId, out var name) && !string.IsNullOrEmpty(name))
            return name;

        return names.GetValueOrDefault(id) ?? id;
    }
}
