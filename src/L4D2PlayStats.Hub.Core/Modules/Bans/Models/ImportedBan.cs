namespace L4D2PlayStats.Hub.Core.Modules.Bans.Models;

public record ImportedBan(long CommunityId, string? Name, string? Reason, DateTime? BannedOn, DateTime? ExpiresOn, int Minutes);
