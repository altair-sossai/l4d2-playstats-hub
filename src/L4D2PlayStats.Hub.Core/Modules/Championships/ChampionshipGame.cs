using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Championships;

public class ChampionshipGame
{
    public int Id { get; set; }
    public int SeriesId { get; set; }
    public int Number { get; set; }
    public int? CampaignId { get; set; }
    public ChampionshipCampaignSource? CampaignSource { get; set; }
    public int? PickedByTeamId { get; set; }
    public ChampionshipGameStatus Status { get; set; }
    public ChampionshipResultSource? ResultSource { get; set; }
    public int? MatchId { get; set; }
    public bool MatchTeamAIsTeamA { get; set; } = true;
    public int TeamAScore { get; set; }
    public int TeamBScore { get; set; }
    public int TeamAPenalty { get; set; }
    public int TeamBPenalty { get; set; }
    public DateTime? PlayedAtUtc { get; set; }
    public bool Replay { get; set; }
    public bool AutoLinked { get; set; }
    public DateTime? LastStatisticsAtUtc { get; set; }
    public int? UnlinkedMatchId { get; set; }
    public string? Notes { get; set; }

    public bool IsPlayed => Status == ChampionshipGameStatus.Played;

    public bool IsInProgress => Status == ChampionshipGameStatus.InProgress;

    public int TeamATotal => TeamAScore - TeamAPenalty;

    public int TeamBTotal => TeamBScore - TeamBPenalty;

    public bool IsTie => IsPlayed && TeamATotal == TeamBTotal;
}
