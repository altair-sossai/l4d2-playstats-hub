using System.Collections.Concurrent;
using Microsoft.AspNetCore.OutputCaching;

namespace L4D2PlayStats.Hub.Tests.Support;

public class RecordingOutputCacheStore(IOutputCacheStore inner) : IOutputCacheStore
{
    public ConcurrentQueue<string> StoredKeys { get; } = new();

    public ConcurrentQueue<string> EvictedTags { get; } = new();

    public ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken)
    {
        EvictedTags.Enqueue(tag);

        return inner.EvictByTagAsync(tag, cancellationToken);
    }

    public ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken)
    {
        return inner.GetAsync(key, cancellationToken);
    }

    public ValueTask SetAsync(string key, byte[] value, string[]? tags, TimeSpan validFor, CancellationToken cancellationToken)
    {
        StoredKeys.Enqueue(key);

        return inner.SetAsync(key, value, tags, validFor, cancellationToken);
    }
}