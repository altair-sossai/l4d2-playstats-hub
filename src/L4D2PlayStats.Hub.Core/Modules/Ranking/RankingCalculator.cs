using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Settings;

namespace L4D2PlayStats.Hub.Core.Modules.Ranking;

public record RankingMatch(int Id, string? Campaign, IReadOnlyList<MatchPlayer> Players);

public static class RankingCalculator
{
    public static List<RankingPlayer> Calculate(IEnumerable<RankingMatch> matchesOldestFirst, IReadOnlyDictionary<long, int> punishments, RankingSettings settings)
    {
        var players = new Dictionary<long, RankingPlayer>();
        var previousExperience = new Dictionary<long, decimal>();

        foreach (var match in matchesOldestFirst)
        {
            var rows = match.Players.OrderBy(row => row.Order).ToList();
            var experience = new Dictionary<long, MatchExperience>();
            var winners = rows.Where(row => row.Win).ToList();
            var losers = rows.Where(row => row.Loss).ToList();

            foreach (var row in winners)
            {
                var player = GetOrAdd(players, row, true);
                var campaign = player.Campaign(match.Campaign);

                player.Wins++;
                campaign.Wins++;
                player.Results.Add(MatchOutcome.Win);

                Experience(experience, row.CommunityId).Win = true;
            }

            foreach (var row in losers)
            {
                var player = GetOrAdd(players, row, true);
                var campaign = player.Campaign(match.Campaign);

                player.Loss++;
                campaign.Loss++;
                player.Results.Add(MatchOutcome.Loss);

                Experience(experience, row.CommunityId).Loss = true;
            }

            foreach (var row in rows.Where(row => row.Draws > 0))
            {
                var player = GetOrAdd(players, row, false);
                var campaign = player.Campaign(match.Campaign);

                player.Draws += row.Draws;
                campaign.Draws += row.Draws;

                for (var i = 0; i < row.Draws; i++)
                    player.Results.Add(MatchOutcome.Draw);
            }

            BuildRelations(players, winners, losers);

            foreach (var row in rows.Where(row => row.RageQuits > 0))
            {
                var player = GetOrAdd(players, row, false);
                var campaign = player.Campaign(match.Campaign);

                player.Loss += row.RageQuits;
                player.RageQuit += row.RageQuits;
                campaign.Loss += row.RageQuits;
                campaign.RageQuit += row.RageQuits;

                for (var i = 0; i < row.RageQuits; i++)
                    player.Results.Add(MatchOutcome.Loss);

                Experience(experience, row.CommunityId).RageQuit = true;
            }

            previousExperience.Clear();

            foreach (var row in rows.Where(row => row.Participant && players.ContainsKey(row.CommunityId)))
                previousExperience[row.CommunityId] = players[row.CommunityId].Experience;

            foreach (var row in rows.Where(row => row.TeamA).Concat(rows.Where(row => row.TeamB)))
            {
                var player = GetOrAdd(players, row, false);
                var calculation = Experience(experience, row.CommunityId);

                player.Add(row);
                calculation.Mvps += row.MvpSiDamage;
                calculation.MvpsCommon += row.MvpCommon;
            }

            foreach (var (communityId, calculation) in experience)
            {
                if (!players.TryGetValue(communityId, out var player))
                    continue;

                player.Experience += calculation.Total(settings);
            }
        }

        foreach (var (communityId, experience) in previousExperience)
        {
            if (!players.TryGetValue(communityId, out var player))
                continue;

            player.PreviousExperience = experience;
        }

        foreach (var (communityId, punishment) in punishments)
        {
            if (!players.TryGetValue(communityId, out var player))
                continue;

            player.Punishment = punishment;
            player.Experience -= punishment;
            player.PreviousExperience -= punishment;
        }

        return Rank(players.Values, settings);
    }

    private static List<RankingPlayer> Rank(IEnumerable<RankingPlayer> players, RankingSettings settings)
    {
        var ranked = players
            .Where(player => player.Games >= settings.MinimumMatches)
            .OrderByDescending(player => player.Experience)
            .ThenByDescending(player => player.Wins)
            .ThenBy(player => player.Loss)
            .ThenByDescending(player => player.MvpSiDamage)
            .ThenByDescending(player => player.MvpCommon)
            .ToList();

        var position = 1;

        foreach (var player in ranked)
            player.Position = position++;

        return ranked;
    }

    private static RankingPlayer GetOrAdd(Dictionary<long, RankingPlayer> players, MatchPlayer row, bool updateName)
    {
        if (players.TryGetValue(row.CommunityId, out var player))
        {
            if (updateName)
                player.Name = row.Name;

            return player;
        }

        player = new RankingPlayer(row.CommunityId) { Name = row.Name };
        players.Add(row.CommunityId, player);

        return player;
    }

    private static MatchExperience Experience(Dictionary<long, MatchExperience> experience, long communityId)
    {
        if (experience.TryGetValue(communityId, out var calculation))
            return calculation;

        calculation = new MatchExperience();
        experience.Add(communityId, calculation);

        return calculation;
    }

    private static void BuildRelations(Dictionary<long, RankingPlayer> players, List<MatchPlayer> winners, List<MatchPlayer> losers)
    {
        AccumulateTogether(players, winners, true);
        AccumulateTogether(players, losers, false);
        AccumulateAgainst(players, winners, losers, true);
        AccumulateAgainst(players, losers, winners, false);
    }

    private static void AccumulateTogether(Dictionary<long, RankingPlayer> players, List<MatchPlayer> teammates, bool won)
    {
        foreach (var teammate in teammates)
        {
            if (!players.TryGetValue(teammate.CommunityId, out var player))
                continue;

            foreach (var other in teammates.Where(other => other.CommunityId != teammate.CommunityId))
            {
                var relation = player.Relation(other.CommunityId);

                if (won)
                    relation.TogetherWins++;
                else
                    relation.TogetherLosses++;
            }
        }
    }

    private static void AccumulateAgainst(Dictionary<long, RankingPlayer> players, List<MatchPlayer> side, List<MatchPlayer> opponents, bool won)
    {
        foreach (var current in side)
        {
            if (!players.TryGetValue(current.CommunityId, out var player))
                continue;

            foreach (var opponent in opponents)
            {
                var relation = player.Relation(opponent.CommunityId);

                if (won)
                    relation.AgainstWins++;
                else
                    relation.AgainstLosses++;
            }
        }
    }

    private class MatchExperience
    {
        public bool Win { get; set; }
        public bool Loss { get; set; }
        public bool RageQuit { get; set; }
        public int Mvps { get; set; }
        public int MvpsCommon { get; set; }

        public decimal Total(RankingSettings settings)
        {
            if (RageQuit)
                return settings.RageQuit;

            var total = 0m;

            if (Win)
                total += settings.Win;

            if (Loss)
                total += settings.Loss;

            total += Mvps * settings.Mvp;
            total += MvpsCommon * settings.MvpCommon;

            return total;
        }
    }
}
