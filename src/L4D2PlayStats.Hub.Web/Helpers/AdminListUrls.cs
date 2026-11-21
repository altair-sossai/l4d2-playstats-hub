namespace L4D2PlayStats.Hub.Web.Helpers;

public static class AdminListUrls
{
    public static string Page(string url, int page)
    {
        return QueryUrl.Build(url, (AppConsts.AdminPageQuery, page));
    }

    public static string TabPage(string url, string tab, string query, int page)
    {
        return QueryUrl.Build(url, (AppConsts.AdminTabQuery, tab), (query, page));
    }
}
