namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PunishmentCommand
{
    public string? CommunityId { get; set; }
    public int LostExperiencePoints { get; set; }
    public int? SeasonId { get; set; }
}