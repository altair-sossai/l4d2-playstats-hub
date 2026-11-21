namespace L4D2PlayStats.Hub.Web.Helpers;

public static class AdminListUrls
{
    public static string Page(string url, int page)
    {
        return page > 1 ? $"{url}?{AppConsts.AdminPageQuery}={page}" : url;
    }

    public static string TabPage(string url, string tab, string query, int page)
    {
        var tabUrl = AdminTabs.Url(url, tab);

        return page > 1 ? $"{tabUrl}&{query}={page}" : tabUrl;
    }
}
