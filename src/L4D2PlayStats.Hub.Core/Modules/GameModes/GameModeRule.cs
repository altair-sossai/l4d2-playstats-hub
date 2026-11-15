using L4D2PlayStats.Hub.Core.Modules.GameModes.Helpers;

namespace L4D2PlayStats.Hub.Core.Modules.GameModes;

public class GameModeRule
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string ConfigurationPattern { get; set; } = AppConsts.AnyConfigurationPattern;
    public int? TeamSize { get; set; }
    public int MinimumMaps { get; set; } = AppConsts.DefaultMinimumMaps;
    public bool CountsForRanking { get; set; } = true;
    public int Priority { get; set; }

    public bool Matches(string? configurationName, int teamSize)
    {
        if (TeamSize != null && TeamSize != teamSize)
            return false;

        return WildcardPattern.IsMatch(ConfigurationPattern, configurationName ?? string.Empty);
    }
}
