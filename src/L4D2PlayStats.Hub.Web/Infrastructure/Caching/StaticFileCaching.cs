using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Caching;

public static class StaticFileCaching
{
    public static StaticFileOptions Options()
    {
        return new StaticFileOptions { OnPrepareResponse = Apply };
    }

    private static void Apply(StaticFileResponseContext context)
    {
        var extension = Path.GetExtension(context.File.Name);

        context.Context.Response.Headers[HeaderNames.CacheControl] = IsCurrentVersion(context.Context)
            ? AppConsts.VersionedStaticFileCacheControl
            : AppConsts.CodeExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)
                ? AppConsts.UnversionedCodeCacheControl
                : AppConsts.UnversionedStaticFileCacheControl;
    }

    private static bool IsCurrentVersion(HttpContext context)
    {
        var request = context.Request;
        var version = request.Query[AppConsts.StaticFileVersionParameter].ToString();

        if (string.IsNullOrEmpty(version) || !request.Path.HasValue)
            return false;

        var provider = context.RequestServices.GetRequiredService<IFileVersionProvider>();
        var expected = provider.AddFileVersionToPath(request.PathBase, request.Path.Value);

        return string.Equals(expected, $"{request.Path.Value}?{AppConsts.StaticFileVersionParameter}={version}", StringComparison.Ordinal);
    }
}
