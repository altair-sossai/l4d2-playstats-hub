namespace L4D2PlayStats.Hub.Core.Modules.Live.Settings;

public class LiveServerSettings
{
    public int DelayInSeconds { get; set; } = AppConsts.LiveDelayInSeconds;
    public int RefreshInMilliseconds { get; set; } = AppConsts.LiveRefreshInMilliseconds;
    public int OnlineWindowInSeconds { get; set; } = AppConsts.ServerOnlineWindowInSeconds;
    public int RemoteCommandExpirationInMinutes { get; set; } = AppConsts.RemoteCommandExpirationInMinutes;

    public void Normalize()
    {
        DelayInSeconds = Math.Clamp(DelayInSeconds, 0, AppConsts.LiveMaxDelayInSeconds);
        RefreshInMilliseconds = Math.Clamp(RefreshInMilliseconds, AppConsts.LiveMinRefreshInMilliseconds, AppConsts.LiveMaxRefreshInMilliseconds);
        OnlineWindowInSeconds = Math.Clamp(OnlineWindowInSeconds, AppConsts.ServerMinOnlineWindowInSeconds, AppConsts.ServerMaxOnlineWindowInSeconds);
        RemoteCommandExpirationInMinutes = Math.Clamp(RemoteCommandExpirationInMinutes, AppConsts.RemoteCommandMinExpirationInMinutes, AppConsts.RemoteCommandMaxExpirationInMinutes);
    }
}
