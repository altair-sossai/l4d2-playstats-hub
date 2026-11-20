using L4D2PlayStats.Hub.Core.Modules.GameModes;

namespace L4D2PlayStats.Hub.Web.Models;

public class GameModesViewModel
{
    public IReadOnlyList<GameModeRule> Rules { get; init; } = [];
    public IReadOnlyList<SeenConfiguration> Configurations { get; init; } = [];
}