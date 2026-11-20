using System.Collections.Concurrent;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Live;

public class LiveNotifier(
    IHubContext<LiveHub> hubContext,
    LiveWatchers watchers,
    ILiveServerStore store,
    IGameServerPresence presence,
    ILiveSettings liveSettings,
    IClock clock)
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ConcurrentDictionary<int, LiveChangeTracker> _trackers = new();

    public async Task NotifyAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            var servers = watchers.Servers;
            var watched = servers.Select(server => server.Id).ToHashSet();

            foreach (var serverId in _trackers.Keys.Where(serverId => !watched.Contains(serverId)))
                _trackers.TryRemove(serverId, out _);

            var interval = TimeSpan.FromMilliseconds(liveSettings.Current.RefreshInMilliseconds);
            var feedInterval = interval / 2;
            var now = clock.UtcNow;

            foreach (var server in servers)
            {
                var tracker = _trackers.GetOrAdd(server.Id, _ => new LiveChangeTracker());
                var change = tracker.Detect(store.For(server.Id), presence.GetStatus(server).Signal, now, interval, feedInterval);

                if (change == LiveChange.None)
                    continue;

                await hubContext.Clients.Group(LiveWatchers.Group(server.PublicId)).SendAsync(AppConsts.LiveHubChangedMethod, Names(change), cancellationToken);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Track(GameServer server)
    {
        _trackers.GetOrAdd(server.Id, _ => Baseline(server));
    }

    private LiveChangeTracker Baseline(GameServer server)
    {
        var tracker = new LiveChangeTracker();
        var interval = TimeSpan.FromMilliseconds(liveSettings.Current.RefreshInMilliseconds);

        tracker.Detect(store.For(server.Id), presence.GetStatus(server).Signal, clock.UtcNow, interval, interval / 2);

        return tracker;
    }

    private static string[] Names(LiveChange change)
    {
        var names = new List<string>();

        if (change.HasFlag(LiveChange.Header))
            names.Add(AppConsts.LiveChangeHeader);

        if (change.HasFlag(LiveChange.Players))
            names.Add(AppConsts.LiveChangePlayers);

        if (change.HasFlag(LiveChange.Feed))
            names.Add(AppConsts.LiveChangeFeed);

        return [.. names];
    }
}
