namespace L4D2PlayStats.Hub.Web.Helpers;

public static class CommunityAdminUrls
{
    private static readonly string[] Tabs = [AppConsts.CommunitySettingsTab, AppConsts.CommunityVotersTab, AppConsts.CommunityBlockedTab, AppConsts.CommunityMaintenanceTab];

    public static string Tab(string? tab, int page, int blockedPage)
    {
        if (tab != null && Tabs.Contains(tab, StringComparer.OrdinalIgnoreCase))
            return tab.ToLowerInvariant();

        if (page > 1)
            return AppConsts.CommunityVotersTab;

        return blockedPage > 1 ? AppConsts.CommunityBlockedTab : AppConsts.CommunitySettingsTab;
    }

    public static string Of(string tab)
    {
        return $"{AppConsts.AdminCommunityUrl}?tab={tab}";
    }

    public static string Voters(int page)
    {
        return page > 1 ? $"{Of(AppConsts.CommunityVotersTab)}&page={page}" : Of(AppConsts.CommunityVotersTab);
    }

    public static string Blocked(int page)
    {
        return page > 1 ? $"{Of(AppConsts.CommunityBlockedTab)}&blockedPage={page}" : Of(AppConsts.CommunityBlockedTab);
    }
}
