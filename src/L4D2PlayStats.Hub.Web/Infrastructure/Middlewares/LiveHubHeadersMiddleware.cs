namespace L4D2PlayStats.Hub.Web.Infrastructure.Middlewares;

public class LiveHubHeadersMiddleware(RequestDelegate next)
{
    public Task Invoke(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments(AppConsts.LiveHubUrl))
            context.Response.Headers[AppConsts.ProxyBufferingHeader] = AppConsts.ProxyBufferingDisabled;

        return next(context);
    }
}
