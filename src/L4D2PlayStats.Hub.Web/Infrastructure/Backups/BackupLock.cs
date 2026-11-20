namespace L4D2PlayStats.Hub.Web.Infrastructure.Backups;

public sealed class BackupLock : IDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public bool IsRunning => _gate.CurrentCount == 0;

    public void Dispose()
    {
        _gate.Dispose();
    }

    public bool TryEnter()
    {
        return _gate.Wait(0);
    }

    public void Exit()
    {
        _gate.Release();
    }
}