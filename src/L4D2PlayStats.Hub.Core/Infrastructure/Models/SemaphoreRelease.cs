namespace L4D2PlayStats.Hub.Core.Infrastructure.Models;

public sealed class SemaphoreRelease(SemaphoreSlim semaphore) : IDisposable, IAsyncDisposable
{
    private int _released;

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _released, 1) == 0)
            semaphore.Release();
    }

    public ValueTask DisposeAsync()
    {
        Dispose();

        return ValueTask.CompletedTask;
    }
}
