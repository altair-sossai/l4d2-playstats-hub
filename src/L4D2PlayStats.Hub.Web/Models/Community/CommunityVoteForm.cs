using L4D2PlayStats.Hub.Core.Modules.Community.Enums;

namespace L4D2PlayStats.Hub.Web.Models.Community;

public class CommunityVoteForm
{
    public long Left { get; set; }
    public long Right { get; set; }
    public CommunityVoteOutcome Outcome { get; set; }
}
