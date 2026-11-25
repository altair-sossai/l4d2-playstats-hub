namespace L4D2PlayStats.Hub.Core.Modules.Championships;

public class ChampionshipSeries
{
    public int Id { get; set; }
    public int StageId { get; set; }
    public ChampionshipStage? Stage { get; set; }
    public int? GroupId { get; set; }
    public int Round { get; set; }
    public int Position { get; set; }
    public int? TeamAId { get; set; }
    public int? TeamBId { get; set; }
    public int BestOf { get; set; } = 1;
    public int? NextSeriesId { get; set; }
    public bool NextSlotA { get; set; }
    public int? LoserNextSeriesId { get; set; }
    public bool LoserNextSlotA { get; set; }
    public bool ThirdPlace { get; set; }
    public bool Bye { get; set; }
    public bool Finished { get; set; }
    public int? WinnerTeamId { get; set; }
    public bool Walkover { get; set; }
    public DateTime? DeadlineUtc { get; set; }
    public DateTime? ScheduledAtUtc { get; set; }
    public int? TeamAVetoCampaignId { get; set; }
    public int? TeamBVetoCampaignId { get; set; }
    public string? Notes { get; set; }
    public List<ChampionshipGame> Games { get; set; } = [];
    public List<ChampionshipDateProposal> Proposals { get; set; } = [];

    public bool HasTeams => TeamAId != null && TeamBId != null;

    public int? LoserTeamId => !Finished || WinnerTeamId == null || !HasTeams ? null : WinnerTeamId == TeamAId ? TeamBId : TeamAId;

    public bool Has(int teamId)
    {
        return TeamAId == teamId || TeamBId == teamId;
    }
}
