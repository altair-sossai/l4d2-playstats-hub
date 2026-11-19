namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public interface ILiveDelay
{
    TimeSpan Current { get; }
    void Set(int seconds);
}
