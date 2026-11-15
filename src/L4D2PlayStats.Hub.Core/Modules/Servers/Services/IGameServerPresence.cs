using L4D2PlayStats.Hub.Core.Modules.Servers.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Servers.Services;

public interface IGameServerPresence
{
    bool Signal(int serverId, GameServerSignal signal);
    void Seen(int serverId);
    void InvalidKey(string publicId);
    void StatisticsReceived(int serverId);
    void LiveDataReceived(int serverId);
    GameServerStatus GetStatus(GameServer server);
    void Forget(GameServer server);
}
