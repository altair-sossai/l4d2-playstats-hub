using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Championships;

public class Championship
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public string? Rules { get; set; }
    public string? RulesPortuguese { get; set; }
    public string? RulesSpanish { get; set; }
    public ChampionshipStatus Status { get; set; }
    public string? StreamUrl { get; set; }
    public string? VideosUrl { get; set; }
    public string? GameConfiguration { get; set; }
    public int PlayersPerGame { get; set; } = AppConsts.ChampionshipDefaultPlayersPerGame;
    public int RosterSize { get; set; } = AppConsts.ChampionshipDefaultPlayersPerGame;
    public int? MaxParticipants { get; set; }
    public bool Substitutes { get; set; }
    public ChampionshipDraftMethod DraftMethod { get; set; }
    public DateTime? StartsOn { get; set; }
    public DateTime? EndsOn { get; set; }
    public string? CancelReason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public List<ChampionshipParticipant> Participants { get; set; } = [];
    public List<ChampionshipTeam> Teams { get; set; } = [];
    public List<ChampionshipStage> Stages { get; set; } = [];
    public List<ChampionshipDraftPick> DraftPicks { get; set; } = [];

    public bool IsPublic => Status != ChampionshipStatus.Draft;

    public bool IsOver => Status is ChampionshipStatus.Finished or ChampionshipStatus.Cancelled;
}
