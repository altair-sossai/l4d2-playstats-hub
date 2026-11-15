namespace L4D2PlayStats.Hub.Web.Infrastructure.Middlewares;

public class ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);

            if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
                logger.LogWarning("Request resulted in error: {Method} {Path} => {StatusCode}", context.Request.Method, context.Request.Path, context.Response.StatusCode);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Unhandled exception during request: {Method} {Path}", context.Request.Method, context.Request.Path);

            throw;
        }
    }
}
