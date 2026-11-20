namespace L4D2PlayStats.Hub.Core.Modules.Players.Models;

public record PlayerDirectoryPage(IReadOnlyList<PlayerDirectoryEntry> Players, int Total, int Page, bool HasMore);
