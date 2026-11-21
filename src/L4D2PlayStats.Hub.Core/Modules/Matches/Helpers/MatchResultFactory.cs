using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Matches.Models;
using ParsedStatistics = L4D2PlayStats.Statistics;

namespace L4D2PlayStats.Hub.Core.Modules.Matches.Helpers;

public static class MatchResultFactory
{
    public static MatchResult Create(Match match, IReadOnlyList<MatchMapResult> maps, ParsedStatistics? newest, IForcedNameStore forcedNames)
    {
        var rows = match.Players
            .GroupBy(player => player.CommunityId)
            .ToDictionary(group => group.Key, group => group.First());

        var teamA = new MatchTeamResult(AppConsts.TeamACode, match.TeamAScore);
        var teamB = new MatchTeamResult(AppConsts.TeamBCode, match.TeamBScore);

        if (!AddPlayers(teamA, newest, rows, forcedNames))
            AddStoredPlayers(teamA, match.Players.Where(player => player.TeamA), forcedNames);

        if (!AddPlayers(teamB, newest, rows, forcedNames))
            AddStoredPlayers(teamB, match.Players.Where(player => player.TeamB), forcedNames);

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
            Maps = maps
        };
    }

    private static bool AddPlayers(MatchTeamResult team, ParsedStatistics? newest, Dictionary<long, MatchPlayer> rows, IForcedNameStore forcedNames)
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

            team.AddPlayer(communityId, forcedNames.Find(communityId) ?? playerName.Name ?? row?.Name, row);
        }

        return true;
    }

    private static void AddStoredPlayers(MatchTeamResult team, IEnumerable<MatchPlayer> players, IForcedNameStore forcedNames)
    {
        foreach (var player in players.OrderBy(player => player.Order))
            team.AddPlayer(player.CommunityId, forcedNames.Find(player.CommunityId) ?? player.Name, player);
    }
}
