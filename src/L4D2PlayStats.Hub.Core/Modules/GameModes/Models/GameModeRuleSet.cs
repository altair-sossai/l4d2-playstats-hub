namespace L4D2PlayStats.Hub.Core.Modules.GameModes.Models;

public class GameModeRuleSet(IReadOnlyList<GameModeRule> rules)
{
    private readonly List<GameModeRule> _rules = [.. rules.OrderBy(r => r.Priority).ThenBy(r => r.Id)];

    public GameModeRule? Find(string? configurationName, int teamSize)
    {
        return _rules.FirstOrDefault(rule => rule.Matches(configurationName, teamSize));
    }

    public bool CountsForRanking(string? configurationName, int teamSize, int maps)
    {
        var rule = Find(configurationName, teamSize);

        return rule is { CountsForRanking: true } && maps >= rule.MinimumMaps;
    }
}