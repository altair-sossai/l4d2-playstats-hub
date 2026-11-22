namespace L4D2PlayStats.Hub.Web.Models.Community;

public class BlockVoterForm
{
    public string? CommunityId { get; set; }
    public bool RemoveVotes { get; set; }
    public List<string>? RelatedIds { get; set; }
    public string? ReturnUrl { get; set; }
}
