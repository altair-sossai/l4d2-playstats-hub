using L4D2PlayStats.Hub.Core.Modules.Live.Settings;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public interface ILiveSettings
{
    LiveServerSettings Current { get; }
    void Set(LiveServerSettings settings);
}
