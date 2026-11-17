using System.Collections.Concurrent;
using Microsoft.Extensions.Primitives;

namespace L4D2PlayStats.Hub.Core.Infrastructure;

public class HubState
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _gates = new();
    private CancellationTokenSource _changes = new();
    private long _dataVersion;

    public SemaphoreSlim WriteLock { get; } = new(1, 1);

    public SemaphoreSlim SeasonLock { get; } = new(1, 1);

    public long DataVersion => Interlocked.Read(ref _dataVersion);

    public IChangeToken DataChangeToken => new CancellationChangeToken(Volatile.Read(ref _changes).Token);

    public void DataChanged()
    {
        Interlocked.Increment(ref _dataVersion);

        var previous = Interlocked.Exchange(ref _changes, new CancellationTokenSource());

        previous.Cancel();
    }

    public SemaphoreSlim Gate(string key)
    {
        return _gates.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
    }
}
