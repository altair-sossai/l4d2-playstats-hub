using L4D2PlayStats.Hub.Core.Modules.Live.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public interface ILiveServerStore
{
    IReadOnlyCollection<LiveServerState> States { get; }
    LiveServerState For(int serverId);
    void Forget(int serverId);
}
