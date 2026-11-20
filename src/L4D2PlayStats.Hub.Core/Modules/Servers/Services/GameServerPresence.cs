using L4D2PlayStats.Hub.Core.Modules.Servers.Enums;
using L4D2PlayStats.Hub.Core.Modules.Servers.Models;
using System.Collections.Concurrent;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Servers.Services;

public class GameServerPresence(IClock clock, ILiveSettings liveSettings) : IGameServerPresence
{
    private readonly ConcurrentDictionary<string, DateTime> _invalidKeys = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<int, DateTime> _lastLiveData = new();
    private readonly ConcurrentDictionary<int, DateTime> _lastSaved = new();
    private readonly ConcurrentDictionary<int, DateTime> _lastSeen = new();
    private readonly ConcurrentDictionary<int, DateTime> _lastStatistics = new();
    private readonly ConcurrentDictionary<int, GameServerSignal> _signals = new();

    public bool Signal(int serverId, GameServerSignal signal)
    {
        signal.ReceivedAtUtc = clock.UtcNow;
        _signals[serverId] = signal;

        return Touch(serverId);
    }

    public void Seen(int serverId)
    {
        _lastSeen[serverId] = clock.UtcNow;
    }

    public void InvalidKey(string publicId)
    {
        _invalidKeys[publicId] = clock.UtcNow;
    }

    public void StatisticsReceived(int serverId)
    {
        _lastStatistics[serverId] = clock.UtcNow;
        Seen(serverId);
    }

    public void LiveDataReceived(int serverId)
    {
        _lastLiveData[serverId] = clock.UtcNow;
        Seen(serverId);
    }

    public GameServerStatus GetStatus(GameServer server)
    {
        var now = clock.UtcNow;
        var lastSeen = _lastSeen.TryGetValue(server.Id, out var seen) ? seen : server.LastSeenAtUtc;
        var invalidKey = _invalidKeys.TryGetValue(server.PublicId, out var invalid) ? invalid : (DateTime?)null;

        var status = new GameServerStatus
        {
            LastSeenAtUtc = lastSeen,
            InvalidKeyAtUtc = invalidKey,
            Signal = _signals.GetValueOrDefault(server.Id),
            LastStatisticsAtUtc = _lastStatistics.TryGetValue(server.Id, out var statistics) ? statistics : null,
            LastLiveDataAtUtc = _lastLiveData.TryGetValue(server.Id, out var liveData) ? liveData : null
        };

        var recentInvalidKey = invalidKey != null
                               && now - invalidKey.Value <= TimeSpan.FromMinutes(AppConsts.InvalidKeyWindowInMinutes)
                               && (lastSeen == null || invalidKey.Value > lastSeen.Value);

        if (recentInvalidKey)
            status.State = GameServerConnectionState.InvalidKey;
        else if (lastSeen == null)
            status.State = GameServerConnectionState.Waiting;
        else if (now - lastSeen.Value <= TimeSpan.FromSeconds(liveSettings.Current.OnlineWindowInSeconds))
            status.State = GameServerConnectionState.Online;
        else
            status.State = GameServerConnectionState.Offline;

        return status;
    }

    public void Forget(GameServer server)
    {
        _signals.TryRemove(server.Id, out _);
        _lastSeen.TryRemove(server.Id, out _);
        _lastSaved.TryRemove(server.Id, out _);
        _lastStatistics.TryRemove(server.Id, out _);
        _lastLiveData.TryRemove(server.Id, out _);
        _invalidKeys.TryRemove(server.PublicId, out _);
    }

    private bool Touch(int serverId)
    {
        var now = clock.UtcNow;
        _lastSeen[serverId] = now;

        var mustSave = !_lastSaved.TryGetValue(serverId, out var saved)
                       || now - saved >= TimeSpan.FromSeconds(AppConsts.ServerLastSeenSaveIntervalInSeconds);

        if (mustSave)
            _lastSaved[serverId] = now;

        return mustSave;
    }
}
