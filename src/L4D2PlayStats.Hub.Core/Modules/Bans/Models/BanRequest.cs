namespace L4D2PlayStats.Hub.Core.Modules.Bans.Models;

public record BanRequest(long CommunityId, string? Name, DateTime? EndsAtUtc, string? Reason, int? RelatedToBanId = null);
