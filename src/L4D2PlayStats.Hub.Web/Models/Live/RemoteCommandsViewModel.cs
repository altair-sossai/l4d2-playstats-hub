using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Servers;

namespace L4D2PlayStats.Hub.Web.Models.Live;

public class RemoteCommandsViewModel
{
    public IReadOnlyList<GameServer> Servers { get; init; } = [];
    public required PagedListModel<RemoteCommandLog> Logs { get; init; }
    public int Delivered { get; init; }
    public TimeZoneInfo TimeZone { get; init; } = TimeZoneInfo.Utc;
}
