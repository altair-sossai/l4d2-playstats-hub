namespace L4D2PlayStats.Hub.Core.Modules.Players.Models;

public record ForcedNameEntry(long CommunityId, string Name, string? CurrentName, DateTime UpdatedAtUtc);
