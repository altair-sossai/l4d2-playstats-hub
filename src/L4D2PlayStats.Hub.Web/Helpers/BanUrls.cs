namespace L4D2PlayStats.Hub.Web.Helpers;

public static class BanUrls
{
    public static string Admin(string? search, string tab, int page)
    {
        var pageQuery = tab switch
        {
            AppConsts.BansEndedTab => AppConsts.BansEndedPageQuery,
            AppConsts.BansAlertsTab => AppConsts.BansAlertsPageQuery,
            _ => AppConsts.AdminPageQuery
        };

        return QueryUrl.Build(AppConsts.BansAdminUrl, (AppConsts.SearchQuery, search), (AppConsts.AdminTabQuery, tab == AppConsts.BansActiveTab ? null : tab), (pageQuery, page > 1 ? page : null));
    }

    public static string Public(string? search, int page)
    {
        return QueryUrl.Build(AppConsts.BansUrl, (AppConsts.SearchQuery, search), (AppConsts.AdminPageQuery, page > 1 ? page : null));
    }
}
