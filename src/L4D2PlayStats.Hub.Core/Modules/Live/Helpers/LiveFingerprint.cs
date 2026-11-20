using System.Text.Json;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Servers.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Helpers;

public static class LiveFingerprint
{
    public static string Header(LiveSnapshot snapshot, GameServerSignal? signal)
    {
        return JsonSerializer.Serialize(new
        {
            snapshot.Configuration,
            snapshot.Round,
            snapshot.Scoreboard,
            snapshot.PlayerCount,
            signal?.Hostname,
            signal?.Map,
            signal?.MaxPlayers
        });
    }

    public static string Players(LiveSnapshot snapshot)
    {
        return JsonSerializer.Serialize(new
        {
            snapshot.Scoreboard,
            snapshot.Survivors,
            snapshot.Infecteds,
            snapshot.Spectators
        });
    }
}
