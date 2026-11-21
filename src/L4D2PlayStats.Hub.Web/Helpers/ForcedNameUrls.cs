namespace L4D2PlayStats.Hub.Web.Helpers;

public static class ForcedNameUrls
{
    public static string Page(string? search, int page)
    {
        return QueryUrl.Build(AppConsts.ForcedNamesUrl, (AppConsts.SearchQuery, search), (AppConsts.AdminPageQuery, page));
    }
}
