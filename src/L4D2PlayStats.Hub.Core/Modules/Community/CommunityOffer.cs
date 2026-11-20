namespace L4D2PlayStats.Hub.Core.Modules.Community;

public class CommunityOffer
{
    public long VoterId { get; set; }
    public long LeftPlayerId { get; set; }
    public long RightPlayerId { get; set; }
    public DateTime OfferedAtUtc { get; set; }
}