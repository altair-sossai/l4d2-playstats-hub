using L4D2PlayStats.Hub.Core.Modules.GameModes.Models;
using L4D2PlayStats.Hub.Core.Modules.Matches.Models;
using ParsedStatistics = L4D2PlayStats.Statistics;

namespace L4D2PlayStats.Hub.Core.Modules.Matches.Helpers;

public static class MatchFactory
{
    public static Match Create(MatchDraft draft, int gameServerId, int seasonId, GameModeRuleSet ruleSet)
    {
        var newest = draft.Newest.Statistics!;
        var oldest = draft.Oldest.Statistics!;
        var rows = new MatchRows();

        AddResults(rows, newest, oldest);
        AddTeams(rows, draft, newest);
        AddParticipants(rows, draft);

        return new Match
        {
            GameServerId = gameServerId,
            SeasonId = seasonId,
            Campaign = draft.Campaign,
            ConfigurationName = draft.ConfigurationName,
            TeamSize = draft.TeamSize,
            FirstFileDate = draft.Oldest.File.FileDate,
            LastFileDate = draft.Newest.File.FileDate,
            StartedAtUtc = oldest.MapStart,
            EndedAtUtc = newest.MapEnd,
            TeamAScore = newest.Scoring?.TeamA?.Score ?? 0,
            TeamBScore = newest.Scoring?.TeamB?.Score ?? 0,
            MapCount = draft.Files.Count,
            CountsForRanking = ruleSet.CountsForRanking(draft.ConfigurationName, draft.TeamSize, draft.Files.Count),
            Players = rows.ToList()
        };
    }

    private static void AddResults(MatchRows rows, ParsedStatistics newest, ParsedStatistics oldest)
    {
        var firstRoundPlayers = oldest.Halves.SelectMany(half => half.Players).ToList();
        var lastRoundPlayers = newest.Halves.SelectMany(half => half.Players).ToList();
        var firstRoundIds = firstRoundPlayers.Select(p => p.CommunityId).ToHashSet();
        var teamA = newest.Scoring?.TeamA;
        var teamB = newest.Scoring?.TeamB;

        if (teamA == null || teamB == null)
            return;

        if (teamA.Score != teamB.Score)
        {
            var winners = teamA.Score > teamB.Score ? newest.TeamA : newest.TeamB;
            var losers = teamA.Score > teamB.Score ? newest.TeamB : newest.TeamA;

            foreach (var playerName in winners.Where(w => firstRoundIds.Contains(w.CommunityId)))
            {
                var row = rows.GetOrAdd(playerName.CommunityId, playerName.Name);
                row?.Win = true;
            }

            foreach (var playerName in losers.Where(w => firstRoundIds.Contains(w.CommunityId)))
            {
                var row = rows.GetOrAdd(playerName.CommunityId, playerName.Name);
                row?.Loss = true;
            }
        }
        else
        {
            foreach (var player in firstRoundPlayers.Where(first => lastRoundPlayers.Any(last => last.CommunityId == first.CommunityId)))
            {
                var row = rows.GetOrAdd(player.CommunityId, player.PlayerName);
                if (row != null)
                    row.Draws++;
            }
        }

        foreach (var player in firstRoundPlayers.Where(first => lastRoundPlayers.All(last => last.CommunityId != first.CommunityId)))
        {
            var row = rows.GetOrAdd(player.CommunityId, player.PlayerName);
            if (row != null)
                row.RageQuits++;
        }
    }

    private static void AddTeams(MatchRows rows, MatchDraft draft, ParsedStatistics newest)
    {
        var halfA = newest.Halves.FirstOrDefault(half => half.RoundHalf?.Team == 'A');
        var halfB = newest.Halves.FirstOrDefault(half => half.RoundHalf?.Team == 'B');

        if (halfA == null || halfB == null)
            return;

        foreach (var playerName in newest.PlayerNames.Where(pn => halfA.Players.Any(p => p.CommunityId == pn.CommunityId)))
        {
            var row = rows.GetOrAdd(playerName.CommunityId, playerName.Name);
            row?.TeamA = true;
        }

        foreach (var playerName in newest.PlayerNames.Where(pn => halfB.Players.Any(p => p.CommunityId == pn.CommunityId)))
        {
            var row = rows.GetOrAdd(playerName.CommunityId, playerName.Name);
            row?.TeamB = true;
        }

        foreach (var row in rows.ToList().Where(r => r.TeamA || r.TeamB))
        foreach (var file in draft.Files)
            Accumulate(row, file.Statistics!);
    }

    private static void Accumulate(MatchPlayer row, ParsedStatistics statistics)
    {
        var communityId = row.CommunityId.ToString();

        foreach (var half in statistics.Halves)
        {
            var mvpSiDamage = half.MvpSiDamage;
            var mvpCommon = half.MvpCommon;
            var lvpFfGiven = half.LvpFfGiven;

            foreach (var player in half.Players.Where(p => p.CommunityId == communityId))
            {
                row.Died += player.Died;
                row.Incaps += player.Incaps;
                row.DmgTaken += player.DmgTaken;
                row.Common += player.Common;
                row.SiKilled += player.SiKilled;
                row.SiDamage += player.SiDamage;
                row.TankDamage += player.TankDamage;
                row.RockEats += player.RockEats;
                row.WitchDamage += player.WitchDamage;
                row.Skeets += player.Skeets + player.SkeetsHurt + player.SkeetsMelee;
                row.SkeetsMelee += player.SkeetsMelee;
                row.Levels += player.Levels + player.LevelsHurt;
                row.Crowns += player.Crowns + player.CrownsHurt;
                row.FfGiven += player.FfGiven;

                if (mvpSiDamage != null && player.CommunityId == mvpSiDamage.CommunityId)
                    row.MvpSiDamage++;

                if (mvpCommon != null && player.CommunityId == mvpCommon.CommunityId)
                    row.MvpCommon++;

                if (lvpFfGiven != null && player.CommunityId == lvpFfGiven.CommunityId)
                    row.LvpFfGiven++;
            }

            foreach (var infectedPlayer in half.InfectedPlayers.Where(p => p.CommunityId == communityId))
            {
                row.DmgTotal += infectedPlayer.DmgTotal;
                row.DmgTank += infectedPlayer.DmgTank;
                row.DmgSpit += infectedPlayer.DmgSpit;
                row.HunterDpDmg += infectedPlayer.HunterDpDmg;
            }
        }
    }

    private static void AddParticipants(MatchRows rows, MatchDraft draft)
    {
        foreach (var half in draft.Files.SelectMany(file => file.Statistics!.Halves))
        {
            foreach (var player in half.Players)
            {
                var row = rows.GetOrAdd(player.CommunityId, player.PlayerName);
                row?.Participant = true;
            }

            foreach (var infectedPlayer in half.InfectedPlayers)
            {
                var row = rows.GetOrAdd(infectedPlayer.CommunityId, infectedPlayer.PlayerName);
                row?.Participant = true;
            }
        }
    }

    private class MatchRows
    {
        private readonly Dictionary<long, MatchPlayer> _index = [];
        private readonly List<MatchPlayer> _rows = [];

        public MatchPlayer? GetOrAdd(string? communityId, string? name)
        {
            if (string.IsNullOrEmpty(communityId) || !long.TryParse(communityId, out var id))
                return null;

            if (_index.TryGetValue(id, out var row))
            {
                if (string.IsNullOrEmpty(row.Name))
                    row.Name = name;

                return row;
            }

            row = new MatchPlayer { CommunityId = id, Name = name, Order = _rows.Count };

            _index.Add(id, row);
            _rows.Add(row);

            return row;
        }

        public List<MatchPlayer> ToList()
        {
            return [.. _rows];
        }
    }
}