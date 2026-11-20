namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public record PlayerConnectionDetails(PlayerConnectionResult? Player, IReadOnlyList<PlayerConnectionResult> RelatedPlayers);