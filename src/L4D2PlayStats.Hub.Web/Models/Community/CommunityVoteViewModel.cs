using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;

namespace L4D2PlayStats.Hub.Web.Models.Community;

public class CommunityVoteViewModel
{
    public CommunityBallot? Ballot { get; init; }
    public string? Refusal { get; init; }
    public CommunityRankingSettings Settings { get; init; } = new();

    public bool SignedIn => Ballot != null;
}