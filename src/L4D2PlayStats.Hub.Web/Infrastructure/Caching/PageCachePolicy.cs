using System.Globalization;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Web.Infrastructure.InGame;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Options;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Caching;

public class PageCachePolicy : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var httpContext = context.HttpContext;
        var cacheable = IsCacheable(httpContext);

        context.EnableOutputCaching = true;
        context.AllowCacheLookup = cacheable;
        context.AllowCacheStorage = cacheable;
        context.AllowLocking = true;
        context.ResponseExpirationTimeSpan = TimeSpan.FromSeconds(AppConsts.PageCacheInSeconds);
        context.Tags.Add(AppConsts.PageCacheTag);
        context.CacheVaryByRules.QueryKeys = AppConsts.PageCacheAllQueryKeys;
        context.CacheVaryByRules.VaryByValues[AppConsts.PageCacheCultureKey] = CultureInfo.CurrentUICulture.Name;
        context.CacheVaryByRules.VaryByValues[AppConsts.PageCacheThemeKey] = Theme(httpContext.Request);
        context.CacheVaryByRules.VaryByValues[AppConsts.PageCacheInGameKey] = InGameBrowser.IsInGame(httpContext).ToString(CultureInfo.InvariantCulture);
        context.CacheVaryByRules.VaryByValues[AppConsts.PageCacheDataKey] = httpContext.RequestServices.GetRequiredService<HubState>().DataVersion.ToString(CultureInfo.InvariantCulture);

        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        if (!IsStorable(context.HttpContext.Response))
            context.AllowCacheStorage = false;

        return ValueTask.CompletedTask;
    }

    public static bool IsCacheable(HttpContext httpContext)
    {
        var request = httpContext.Request;

        if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
            return false;

        if (request.Headers.ContainsKey(AppConsts.AuthorizationHeader))
            return false;

        if (httpContext.User.Identity?.IsAuthenticated == true)
            return false;

        var personalCookies = PersonalCookies(httpContext.RequestServices);

        return !request.Cookies.Keys.Any(cookie => personalCookies.Any(personal => cookie.StartsWith(personal, StringComparison.Ordinal)));
    }

    public static bool IsStorable(HttpResponse response)
    {
        if (response.StatusCode != StatusCodes.Status200OK)
            return false;

        if (response.Headers.SetCookie.Count > 0)
            return false;

        var cacheControl = response.Headers.CacheControl.ToString();

        return !AppConsts.PageCacheRefusedCacheControls.Any(directive => cacheControl.Contains(directive, StringComparison.OrdinalIgnoreCase));
    }

    private static string Theme(HttpRequest request)
    {
        return request.Cookies[AppConsts.ThemeCookie] == AppConsts.LightTheme ? AppConsts.LightTheme : AppConsts.DefaultTheme;
    }

    private static string[] PersonalCookies(IServiceProvider services)
    {
        var authentication = services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme).Cookie.Name;
        var tempData = services.GetRequiredService<IOptions<CookieTempDataProviderOptions>>().Value.Cookie.Name;

        string?[] names = [authentication, tempData, AppConsts.SetupCookie];

        return [.. names.OfType<string>().Where(name => name.Length > 0)];
    }
}