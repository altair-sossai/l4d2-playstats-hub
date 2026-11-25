using L4D2PlayStats.Hub.Core.Modules.Championships.Models;

namespace L4D2PlayStats.Hub.Web.Models.Championships;

public record ChampionshipsIndexModel(List<ChampionshipSummary> Championships, TimeZoneInfo TimeZone);
