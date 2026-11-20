namespace L4D2PlayStats.Hub.Web.Infrastructure.Middlewares;

public class InGameBrowserMiddleware(RequestDelegate next)
{
    public Task Invoke(HttpContext context)
    {
        if (context.Request.Query.ContainsKey(AppConsts.InGameQueryKey) && !context.Request.Cookies.ContainsKey(AppConsts.InGameCookie) && !context.Request.Path.StartsWithSegments(AppConsts.InGamePath, StringComparison.OrdinalIgnoreCase))
            context.Response.Cookies.Append(AppConsts.InGameCookie, AppConsts.InGameCookieValue, new CookieOptions { HttpOnly = true, IsEssential = true, SameSite = SameSiteMode.Lax });

        return next(context);
    }
}
