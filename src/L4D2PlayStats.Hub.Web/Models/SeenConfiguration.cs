using L4D2PlayStats.Hub.Core.Modules.GameModes;

namespace L4D2PlayStats.Hub.Web.Models;

public record SeenConfiguration(GameConfiguration Configuration, GameModeRule? Rule);
