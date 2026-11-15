using L4D2PlayStats.Hub.Core.Modules.Live.Models;

namespace L4D2PlayStats.Hub.Web.Models.Live;

public class PlayerConnectionsViewModel
{
    public long? CommunityId { get; init; }
    public PlayerConnectionDetails? Details { get; init; }
}
