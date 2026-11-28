using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Models;

public record ChampionshipCaptainTask(ChampionshipCaptainTaskKind Kind, int? SeriesId = null, int? GameNumber = null);
