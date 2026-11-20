using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Branding;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers;

public class SiteController(BrandingStore brandingStore, ILiveViewService liveViewService) : Controller
{
    [HttpGet(AppConsts.LogoUrl)]
    [ResponseCache(Duration = AppConsts.LogoCacheInSeconds)]
    public IActionResult Logo()
    {
        var path = brandingStore.LogoPath;
        var contentType = brandingStore.LogoContentType;

        if (path == null || contentType == null)
            return NotFound();

        Response.Headers.XContentTypeOptions = AppConsts.NoSniff;

        return PhysicalFile(path, contentType);
    }

    [HttpGet("/theme/{theme}")]
    public IActionResult Theme(string theme, string? returnUrl = null)
    {
        var options = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true };

        Response.Cookies.Append(AppConsts.ThemeCookie, theme == AppConsts.LightTheme ? AppConsts.LightTheme : AppConsts.DefaultTheme, options);

        return Redirect(!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
    }

    [HttpGet("/connect")]
    [RequireModule(HubModule.LiveServer)]
    public async Task<IActionResult> Connect(CancellationToken cancellationToken)
    {
        var server = await liveViewService.MainServerAsync(cancellationToken);

        return server?.ConnectUrl == null ? Redirect("/servers") : Redirect(server.ConnectUrl);
    }

    [HttpGet("/api/server")]
    [RequireModule(HubModule.LiveServer)]
    public async Task<IActionResult> Server(CancellationToken cancellationToken)
    {
        var server = await liveViewService.MainServerAsync(cancellationToken);
        if (server == null)
            return Ok(null);

        return Ok(new
        {
            publicId = server.Server.PublicId,
            name = server.Name,
            address = server.Server.Address,
            map = server.Map,
            players = server.Players,
            max_players = server.MaxPlayers
        });
    }
}
