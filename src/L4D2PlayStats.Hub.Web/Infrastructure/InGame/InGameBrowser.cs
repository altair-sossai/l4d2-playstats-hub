namespace L4D2PlayStats.Hub.Web.Infrastructure.InGame;

public static class InGameBrowser
{
    public static bool IsInGame(HttpContext context)
    {
        return context.Request.Query.ContainsKey(AppConsts.InGameQueryKey) || context.Request.Cookies.ContainsKey(AppConsts.InGameCookie);
    }
}
