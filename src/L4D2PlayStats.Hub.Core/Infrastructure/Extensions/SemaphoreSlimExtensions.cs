using L4D2PlayStats.Hub.Core.Infrastructure.Models;

namespace L4D2PlayStats.Hub.Core.Infrastructure.Extensions;

public static class SemaphoreSlimExtensions
{
    public static async Task<SemaphoreRelease> LockAsync(this SemaphoreSlim semaphore, CancellationToken cancellationToken = default)
    {
        await semaphore.WaitAsync(cancellationToken);

        return new SemaphoreRelease(semaphore);
    }

    public static async Task<SemaphoreRelease?> TryLockAsync(this SemaphoreSlim semaphore, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        return await semaphore.WaitAsync(timeout, cancellationToken) ? new SemaphoreRelease(semaphore) : null;
    }
}
