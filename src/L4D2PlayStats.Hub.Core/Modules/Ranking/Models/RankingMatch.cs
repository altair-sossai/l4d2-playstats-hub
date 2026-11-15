using L4D2PlayStats.Hub.Core.Modules.Matches;

namespace L4D2PlayStats.Hub.Core.Modules.Ranking.Models;

public record RankingMatch(int Id, string? Campaign, IReadOnlyList<MatchPlayer> Players);
