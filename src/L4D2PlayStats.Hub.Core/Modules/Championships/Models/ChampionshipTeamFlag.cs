using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Models;

public record ChampionshipTeamFlag(int ChampionshipId, string? Flag, string? PendingFlag, ChampionshipStatus Status, long? Captain, long? ViceCaptain);
