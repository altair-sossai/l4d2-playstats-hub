using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Models;

public record ChampionshipSummary(int Id, string Name, string Slug, string? Description, ChampionshipStatus Status, DateTime? StartsOn, DateTime? EndsOn, int Teams, int Participants, string? Champion, string? RunnerUp, DateTime CreatedAtUtc, string? BannerImage, string? LogoImage, int? ChampionTeamId, string? ChampionFlag);
