using System.Collections.Concurrent;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public class LiveServerStore(IClock clock, ILiveDelay liveDelay) : ILiveServerStore
{
    private readonly ConcurrentDictionary<int, LiveServerState> _states = new();

    public IReadOnlyCollection<LiveServerState> States => [.. _states.Values];

    public LiveServerState For(int serverId)
    {
        return _states.GetOrAdd(serverId, _ => new LiveServerState(clock, () => liveDelay.Current));
    }

    public void Forget(int serverId)
    {
        _states.TryRemove(serverId, out _);
    }
}
