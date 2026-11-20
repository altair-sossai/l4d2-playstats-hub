using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Net.Http.Headers;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Errors;

public static class ErrorPages
{
    public static bool ShowsPage(HttpContext context)
    {
        var request = context.Request;

        if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
            return false;

        if (request.Headers.ContainsKey(AppConsts.HtmxRequestHeader))
            return false;

        var path = OriginalPath(context);

        if (AppConsts.ErrorPageExcludedPaths.Any(excluded => path.StartsWithSegments(excluded, StringComparison.OrdinalIgnoreCase)))
            return false;

        if (Path.HasExtension(path.Value))
            return false;

        var accept = request.Headers[HeaderNames.Accept].ToString();

        return string.IsNullOrEmpty(accept) || accept.Contains(AppConsts.HtmlContentType, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsInGame(HttpContext context)
    {
        return OriginalPath(context).StartsWithSegments(AppConsts.InGamePath, StringComparison.OrdinalIgnoreCase);
    }

    public static PathString OriginalPath(HttpContext context)
    {
        var exception = context.Features.Get<IExceptionHandlerPathFeature>();

        if (exception != null)
            return exception.Path;

        var statusCode = context.Features.Get<IStatusCodeReExecuteFeature>();

        return statusCode != null ? new PathString(statusCode.OriginalPath) : context.Request.Path;
    }

    public static string OriginalPathAndQuery(HttpContext context)
    {
        var statusCode = context.Features.Get<IStatusCodeReExecuteFeature>();

        if (statusCode != null)
            return statusCode.OriginalPath + statusCode.OriginalQueryString;

        var exception = context.Features.Get<IExceptionHandlerPathFeature>();

        return (exception != null ? exception.Path : context.Request.Path.Value) + context.Request.QueryString;
    }

    public static bool IsReExecuted(HttpContext context)
    {
        return context.Features.Get<IStatusCodeReExecuteFeature>() != null || context.Features.Get<IExceptionHandlerFeature>() != null;
    }
}