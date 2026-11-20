namespace L4D2PlayStats.Hub.Core.Modules.Live.Settings;

public class SiteChatSettings
{
    public int GlobalCooldownInSeconds { get; set; } = AppConsts.SiteChatGlobalCooldownInSeconds;
    public int UserCooldownInSeconds { get; set; } = AppConsts.SiteChatUserCooldownInSeconds;
    public int MaxMessageLength { get; set; } = AppConsts.SiteChatMaxMessageLength;

    public void Normalize()
    {
        GlobalCooldownInSeconds = Math.Clamp(GlobalCooldownInSeconds, 0, AppConsts.SiteChatMaxCooldownInSeconds);
        UserCooldownInSeconds = Math.Clamp(UserCooldownInSeconds, 0, AppConsts.SiteChatMaxCooldownInSeconds);
        MaxMessageLength = Math.Clamp(MaxMessageLength, AppConsts.SiteChatMinMessageLength, AppConsts.SiteChatMaxMessageLength);
    }
}