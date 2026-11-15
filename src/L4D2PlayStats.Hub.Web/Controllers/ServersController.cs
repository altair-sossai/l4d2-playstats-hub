using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Web.Infrastructure;
using L4D2PlayStats.Hub.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.LiveServer)]
[AutoValidateAntiforgeryToken]
public class ServersController(ILiveViewService liveViewService, ISiteChatService siteChatService, ICurrentUser currentUser) : Controller
{
    private bool Htmx => Request.Headers.ContainsKey(AppConsts.HtmxRequestHeader);

    [HttpGet("/servers")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await liveViewService.ListAsync(cancellationToken);

        return Htmx ? PartialView("_Cards", model) : View(model);
    }

    [HttpGet("/servers/{publicId}")]
    public async Task<IActionResult> Live(string publicId, CancellationToken cancellationToken)
    {
        var server = await liveViewService.FindServerAsync(publicId, cancellationToken);
        if (server == null)
            return NotFound();

        return View(await liveViewService.LiveAsync(server, cancellationToken));
    }

    [HttpGet("/servers/{publicId}/header")]
    public async Task<IActionResult> Header(string publicId, CancellationToken cancellationToken)
    {
        var server = await liveViewService.FindServerAsync(publicId, cancellationToken);
        if (server == null)
            return NotFound();

        return PartialView("_Header", await liveViewService.CardAsync(server, cancellationToken));
    }

    [HttpGet("/servers/{publicId}/players")]
    public async Task<IActionResult> Players(string publicId, CancellationToken cancellationToken)
    {
        var server = await liveViewService.FindServerAsync(publicId, cancellationToken);
        if (server == null)
            return NotFound();

        return PartialView("_Players", await liveViewService.CardAsync(server, cancellationToken));
    }

    [HttpGet("/servers/{publicId}/feed")]
    public async Task<IActionResult> Feed(string publicId, long after = 0, CancellationToken cancellationToken = default)
    {
        var server = await liveViewService.FindServerAsync(publicId, cancellationToken);
        if (server == null)
            return NotFound();

        return PartialView("_FeedUpdate", await liveViewService.FeedAsync(server, after, cancellationToken));
    }

    [HttpPost("/servers/{publicId}/chat")]
    [RequireModule(HubModule.SiteChat)]
    public async Task<IActionResult> Chat(string publicId, string? message, CancellationToken cancellationToken)
    {
        var server = await liveViewService.FindServerAsync(publicId, cancellationToken);
        if (server == null)
            return NotFound();

        var communityId = currentUser.CommunityId;
        if (communityId == null)
            return Unauthorized();

        var user = new SiteChatUser(communityId.Value, currentUser.Name);
        var result = await siteChatService.SendAsync(server, user, message, cancellationToken);

        if (!Htmx)
            return Redirect($"/servers/{server.PublicId}");

        return PartialView("_ChatForm", await liveViewService.ChatAsync(server, result, result.Success ? null : message, cancellationToken));
    }
}
