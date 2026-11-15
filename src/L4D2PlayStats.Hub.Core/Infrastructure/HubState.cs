namespace L4D2PlayStats.Hub.Core.Infrastructure;

public class HubState
{
    private long _dataVersion;

    public SemaphoreSlim WriteLock { get; } = new(1, 1);

    public long DataVersion => Interlocked.Read(ref _dataVersion);

    public void DataChanged()
    {
        Interlocked.Increment(ref _dataVersion);
    }
}
