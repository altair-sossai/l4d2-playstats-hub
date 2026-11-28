using L4D2PlayStats.Hub.Core.Modules.Bans;

namespace L4D2PlayStats.Hub.Web.Models.Bans;

public record BanDetailsModel(Ban Ban, string Name, TimeZoneInfo TimeZone);
