using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Bans;
using L4D2PlayStats.Hub.Core.Modules.Bans.Extensions;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;

namespace L4D2PlayStats.Hub.Web.Models.Bans;

public class BanServerResult(long communityId, Ban? ban, DateTime now, TimeZoneInfo timeZone)
{
    public bool Banned { get; } = ban != null;
    public string CommunityId { get; } = communityId.ToString(CultureInfo.InvariantCulture);
    public string SteamId { get; } = SteamIds.SteamId(communityId);
    public string LegacySteamId { get; } = SteamIds.DisplaySteamId(communityId);
    public string? Reason { get; } = ban?.Reason;
    public bool Permanent { get; } = ban is { EndsAtUtc: null };
    public int MinutesLeft { get; } = ban?.MinutesLeft(now) ?? 0;
    public string? EndsAt { get; } = ban?.EndsAtUtc is { } ends ? SiteTime.FromUtc(ends, timeZone).ToString(AppConsts.BanDateFormat, CultureInfo.InvariantCulture) : null;
}
