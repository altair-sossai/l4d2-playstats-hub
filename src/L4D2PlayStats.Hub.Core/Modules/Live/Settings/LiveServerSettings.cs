namespace L4D2PlayStats.Hub.Core.Modules.Live.Settings;

public class LiveServerSettings
{
    public int DelayInSeconds { get; set; } = AppConsts.LiveDelayInSeconds;

    public void Normalize()
    {
        DelayInSeconds = Math.Clamp(DelayInSeconds, 0, AppConsts.LiveMaxDelayInSeconds);
    }
}
