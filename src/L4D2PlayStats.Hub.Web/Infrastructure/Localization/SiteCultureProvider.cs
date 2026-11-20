using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Localization;

public class SiteCultureProvider : RequestCultureProvider
{
    public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var settingsService = httpContext.RequestServices.GetRequiredService<ISettingsService>();
        var site = await settingsService.GetAsync<SiteSettings>(httpContext.RequestAborted);

        return AppConsts.SupportedCultures.Contains(site.DefaultCulture) ? new ProviderCultureResult(site.DefaultCulture) : null;
    }
}
