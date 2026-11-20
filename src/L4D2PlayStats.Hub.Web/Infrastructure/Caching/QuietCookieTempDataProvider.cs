using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Caching;

public class QuietCookieTempDataProvider(CookieTempDataProvider inner, IOptions<CookieTempDataProviderOptions> options) : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context)
    {
        return inner.LoadTempData(context);
    }

    public void SaveTempData(HttpContext context, IDictionary<string, object> values)
    {
        if (values.Count == 0 && !HasCookie(context.Request))
            return;

        inner.SaveTempData(context, values);
    }

    private bool HasCookie(HttpRequest request)
    {
        var name = options.Value.Cookie.Name;

        return !string.IsNullOrEmpty(name) && request.Cookies.Keys.Any(cookie => cookie.StartsWith(name, StringComparison.Ordinal));
    }
}