using L4D2PlayStats.Hub.Core.Modules.Championships.Models;

namespace L4D2PlayStats.Hub.Web.Models.Championships;

public record ChampionshipsAdminIndexModel(PagedListModel<ChampionshipSummary> Championships, TimeZoneInfo TimeZone, string ReturnUrl);
