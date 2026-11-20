namespace L4D2PlayStats.Hub.Web.Infrastructure.Middlewares;

public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public Task Invoke(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;

            headers[AppConsts.ContentTypeOptionsHeader] = AppConsts.ContentTypeOptionsValue;
            headers[AppConsts.ReferrerPolicyHeader] = AppConsts.ReferrerPolicyValue;
            headers[AppConsts.FrameOptionsHeader] = AppConsts.FrameOptionsValue;

            return Task.CompletedTask;
        });

        return next(context);
    }
}