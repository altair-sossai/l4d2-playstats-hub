using System.Collections.Concurrent;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public class LiveServerStore(IClock clock, ILiveSettings liveSettings) : ILiveServerStore
{
    private readonly ConcurrentDictionary<int, LiveServerState> _states = new();

    public IReadOnlyCollection<LiveServerState> States => [.. _states.Values];

    public LiveServerState For(int serverId)
    {
        return _states.GetOrAdd(serverId, _ => new LiveServerState(clock, () => liveSettings.Current));
    }

    public void Forget(int serverId)
    {
        _states.TryRemove(serverId, out _);
    }
}