using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;

namespace L4D2PlayStats.Hub.Web.Models.Community;

public class CommunityAdminViewModel
{
    public CommunityRankingSettings Settings { get; init; } = new();
    public required PagedListModel<CommunityVoter> Voters { get; init; }
    public int Votes { get; init; }
    public int RatedPlayers { get; init; }
}