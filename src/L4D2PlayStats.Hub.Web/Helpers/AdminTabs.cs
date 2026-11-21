using L4D2PlayStats.Hub.Web.Models.Admin;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class AdminTabs
{
    public static string Active(string? requested, params string[] keys)
    {
        var match = keys.FirstOrDefault(key => string.Equals(key, requested, StringComparison.OrdinalIgnoreCase));

        return match ?? keys[0];
    }

    public static string PaneId(string key)
    {
        return $"{AppConsts.AdminTabPanePrefix}{key}";
    }

    public static string Pane(AdminTabsModel model, string key)
    {
        return model.Active == key ? "tab-pane active" : "tab-pane";
    }

    public static string Url(string page, string key)
    {
        return $"{page}?{AppConsts.AdminTabQuery}={key}";
    }
}
