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
        var request = context.Context.Request;
        var extension = Path.GetExtension(context.File.Name);

        context.Context.Response.Headers[HeaderNames.CacheControl] = request.Query.ContainsKey(AppConsts.StaticFileVersionParameter)
            ? AppConsts.VersionedStaticFileCacheControl
            : AppConsts.CodeExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)
                ? AppConsts.UnversionedCodeCacheControl
                : AppConsts.UnversionedStaticFileCacheControl;
    }
}
