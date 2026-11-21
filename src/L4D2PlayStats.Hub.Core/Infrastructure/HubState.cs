using System.Collections.Concurrent;
using L4D2PlayStats.Hub.Core.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using Microsoft.Extensions.Primitives;

namespace L4D2PlayStats.Hub.Core.Infrastructure;

public class HubState
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _gates = new();
    private CancellationTokenSource _changes = new();
    private CancellationTokenSource _communityChanges = new();
    private long _dataVersion;

    public SemaphoreSlim WriteLock { get; } = new(1, 1);

    public SemaphoreSlim SeasonLock { get; } = new(1, 1);

    public long DataVersion => Interlocked.Read(ref _dataVersion);

    public IChangeToken DataChangeToken => new CancellationChangeToken(Volatile.Read(ref _changes).Token);

    public IChangeToken CommunityChangeToken => new CancellationChangeToken(Volatile.Read(ref _communityChanges).Token);

    public void DataChanged()
    {
        Interlocked.Increment(ref _dataVersion);

        var previous = Interlocked.Exchange(ref _changes, new CancellationTokenSource());

        previous.Cancel();
    }

    public void CommunityChanged()
    {
        var previous = Interlocked.Exchange(ref _communityChanges, new CancellationTokenSource());

        previous.Cancel();
    }

    public Task<SemaphoreRelease> LockAsync(CancellationToken cancellationToken = default)
    {
        return WriteLock.LockAsync(cancellationToken);
    }

    public Task<SemaphoreRelease?> TryLockAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        return WriteLock.TryLockAsync(timeout, cancellationToken);
    }

    public Task<SemaphoreRelease> SeasonLockAsync(CancellationToken cancellationToken = default)
    {
        return SeasonLock.LockAsync(cancellationToken);
    }

    public Task<SemaphoreRelease> GateAsync(string key, CancellationToken cancellationToken = default)
    {
        return Gate(key).LockAsync(cancellationToken);
    }

    public SemaphoreSlim Gate(string key)
    {
        return _gates.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
    }
}
