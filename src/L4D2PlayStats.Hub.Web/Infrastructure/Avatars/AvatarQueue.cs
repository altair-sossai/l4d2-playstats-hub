using System.Collections.Concurrent;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Avatars;

public class AvatarQueue(IClock clock)
{
    private readonly ConcurrentDictionary<long, DateTime> _attempts = new();
    private readonly ConcurrentDictionary<long, byte> _requested = new();
    private readonly SemaphoreSlim _signal = new(0);

    public void Request(long communityId)
    {
        if (!SteamIds.IsValid(communityId) || _requested.Count >= AppConsts.AvatarQueueMaxSize)
            return;

        var now = clock.UtcNow;

        if (_attempts.TryGetValue(communityId, out var attempt) && attempt > now.AddHours(-AppConsts.AvatarRetryInHours))
            return;

        _attempts[communityId] = now;

        if (_requested.TryAdd(communityId, 0))
            Wake();
    }

    public IReadOnlyList<long> Take(int count)
    {
        ForgetOldAttempts();

        var ids = _requested.Keys.Take(count).ToList();

        foreach (var id in ids)
            _requested.TryRemove(id, out _);

        if (!_requested.IsEmpty)
            Wake();

        return ids;
    }

    public Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        return _signal.WaitAsync(timeout, cancellationToken);
    }

    private void ForgetOldAttempts()
    {
        var limit = clock.UtcNow.AddHours(-AppConsts.AvatarRetryInHours);

        foreach (var (communityId, attempt) in _attempts)
            if (attempt <= limit)
                _attempts.TryRemove(communityId, out _);
    }

    private void Wake()
    {
        if (_signal.CurrentCount == 0)
            _signal.Release();
    }
}
