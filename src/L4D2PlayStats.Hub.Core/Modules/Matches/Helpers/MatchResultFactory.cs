using L4D2PlayStats.Hub.Core.Modules.Matches.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using ParsedStatistics = L4D2PlayStats.Statistics;

namespace L4D2PlayStats.Hub.Core.Modules.Matches.Helpers;

public static class MatchResultFactory
{
    public static MatchResult Create(Match match, IReadOnlyList<MatchMapResult> maps, ParsedStatistics? newest, IDisplayNameStore displayNames)
    {
        var rows = match.Players
            .GroupBy(player => player.CommunityId)
            .ToDictionary(group => group.Key, group => group.First());

        var teamA = new MatchTeamResult(AppConsts.TeamACode, match.TeamAScore);
        var teamB = new MatchTeamResult(AppConsts.TeamBCode, match.TeamBScore);

        if (!AddPlayers(teamA, newest, rows, displayNames))
            AddStoredPlayers(teamA, match.Players.Where(player => player.TeamA), displayNames);

        if (!AddPlayers(teamB, newest, rows, displayNames))
            AddStoredPlayers(teamB, match.Players.Where(player => player.TeamB), displayNames);

        return new MatchResult
        {
            Id = match.Id,
            GameServerId = match.GameServerId,
            GameServerName = match.GameServer?.Name,
            SeasonId = match.SeasonId,
            ConfigurationName = match.ConfigurationName,
            Competitive = match.CountsForRanking,
            TeamSize = match.TeamSize,
            FirstFileDate = match.FirstFileDate,
            LastFileDate = match.LastFileDate,
            MatchStart = match.StartedAtUtc ?? match.FirstFileDate,
            MatchStartIsUtc = match.StartedAtUtc != null,
            MatchEnd = match.EndedAtUtc,
            Campaign = match.Campaign,
            Teams = [teamA, teamB],
            Maps = maps,
            PlayerNames = PlayerNames(match, [teamA, teamB], displayNames)
        };
    }

    private static Dictionary<long, string> PlayerNames(Match match, MatchTeamResult[] teams, IDisplayNameStore displayNames)
    {
        var names = new Dictionary<long, string>();

        foreach (var player in teams.SelectMany(team => team.Players).Where(player => !string.IsNullOrEmpty(player.Name)))
            names.TryAdd(player.CommunityId, player.Name!);

        foreach (var player in match.Players)
        {
            var name = displayNames.Apply(player.CommunityId, player.Name);

            if (!string.IsNullOrEmpty(name))
                names.TryAdd(player.CommunityId, name);
        }

        return names;
    }

    private static bool AddPlayers(MatchTeamResult team, ParsedStatistics? newest, Dictionary<long, MatchPlayer> rows, IDisplayNameStore displayNames)
    {
        var half = newest?.Halves.FirstOrDefault(h => h.RoundHalf?.Team == team.Code);
        if (newest == null || half == null)
            return false;

        var survivors = half.Players.Select(player => player.CommunityId).ToHashSet();

        foreach (var playerName in newest.PlayerNames.Where(playerName => survivors.Contains(playerName.CommunityId)))
        {
            if (!long.TryParse(playerName.CommunityId, out var communityId))
                continue;

            var row = rows.GetValueOrDefault(communityId);

            team.AddPlayer(communityId, displayNames.Apply(communityId, playerName.Name ?? row?.Name), row);
        }

        return true;
    }

    private static void AddStoredPlayers(MatchTeamResult team, IEnumerable<MatchPlayer> players, IDisplayNameStore displayNames)
    {
        foreach (var player in players.OrderBy(player => player.Order))
            team.AddPlayer(player.CommunityId, displayNames.Apply(player.CommunityId, player.Name), player);
    }
}
