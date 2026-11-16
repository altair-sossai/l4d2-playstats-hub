using L4D2PlayStats.Hub.Core.Modules.Matches;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Models;

public record PlayerMatch(Match Match, IReadOnlyList<PlayerMatchMap> Maps);
