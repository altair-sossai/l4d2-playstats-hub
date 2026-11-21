using L4D2PlayStats.Hub.Web.Infrastructure.Errors;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Middlewares;

public class ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);

            var statusCode = context.Response.StatusCode;

            if (statusCode >= StatusCodes.Status500InternalServerError && ErrorPages.IsUnloggedServerError(context))
                logger.LogError("Request resulted in error: {Method} {Path} => {StatusCode}", context.Request.Method, ErrorPages.OriginalPath(context), statusCode);
            else if (statusCode >= StatusCodes.Status400BadRequest && !ErrorPages.IsReExecuted(context))
                logger.Log(statusCode == StatusCodes.Status404NotFound ? LogLevel.Debug : LogLevel.Warning, "Request resulted in error: {Method} {Path} => {StatusCode}", context.Request.Method, context.Request.Path, statusCode);
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !context.RequestAborted.IsCancellationRequested)
        {
            logger.LogError(exception, "Unhandled exception during request: {Method} {Path}", context.Request.Method, context.Request.Path);

            ErrorPages.MarkExceptionLogged(context);

            throw;
        }
    }
}
