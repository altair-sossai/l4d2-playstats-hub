using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;

public static class ChampionshipTiebreakers
{
    public static readonly ChampionshipTiebreaker[] Default = [ChampionshipTiebreaker.Difference, ChampionshipTiebreaker.ScoreFor, ChampionshipTiebreaker.HeadToHead];

    public static List<ChampionshipTiebreaker> Parse(string? value)
    {
        return (value ?? string.Empty)
            .Split(AppConsts.ChampionshipTiebreakerSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => Enum.TryParse<ChampionshipTiebreaker>(part, out var tiebreaker) ? (ChampionshipTiebreaker?)tiebreaker : null)
            .Where(tiebreaker => tiebreaker != null)
            .Select(tiebreaker => tiebreaker!.Value)
            .Distinct()
            .ToList();
    }

    public static string Format(IEnumerable<ChampionshipTiebreaker> tiebreakers)
    {
        return string.Join(AppConsts.ChampionshipTiebreakerSeparator, tiebreakers.Distinct());
    }
}
