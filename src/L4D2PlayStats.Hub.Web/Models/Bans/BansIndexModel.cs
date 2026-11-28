using L4D2PlayStats.Hub.Core.Modules.Bans;

namespace L4D2PlayStats.Hub.Web.Models.Bans;

public record BansIndexModel(PagedListModel<Ban> Bans, string? Search, TimeZoneInfo TimeZone);
