using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Championships;

public class ChampionshipDateProposal
{
    public int Id { get; set; }
    public int SeriesId { get; set; }
    public DateTime DateUtc { get; set; }
    public int? TeamId { get; set; }
    public long? ProposedByCommunityId { get; set; }
    public ChampionshipAnswer TeamAAnswer { get; set; }
    public ChampionshipAnswer TeamBAnswer { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public bool Confirmed => TeamAAnswer == ChampionshipAnswer.Accepted && TeamBAnswer == ChampionshipAnswer.Accepted;

    public bool Refused => TeamAAnswer == ChampionshipAnswer.Refused || TeamBAnswer == ChampionshipAnswer.Refused;
}
