using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;

namespace L4D2PlayStats.Hub.Web.Models.Championships;

public record ChampionshipsIndexModel(List<ChampionshipSummary> Current, Page<ChampionshipSummary> History, TimeZoneInfo TimeZone);
