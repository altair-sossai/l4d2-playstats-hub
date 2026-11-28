namespace L4D2PlayStats.Hub.Core.Modules.Championships.Models;

public record ChampionshipCaptainTeam(int ChampionshipId, string Slug, string Championship, int TeamId, string Team, string? Flag, bool Captain, List<ChampionshipCaptainTask> Tasks);
