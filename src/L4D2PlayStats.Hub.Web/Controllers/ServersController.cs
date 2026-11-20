using L4D2PlayStats.Hub.Core.Modules.Live.Settings;
using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using L4D2PlayStats.Hub.Web.Models.Live;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.LiveServer)]
[AutoValidateAntiforgeryToken]
public class ServersController(
    ILiveViewService liveViewService,
    ISiteChatService siteChatService,
    IPlayerNameService playerNameService,
    ISettingsService settingsService,
    ICurrentUser currentUser,
    IStringLocalizer<SharedResource> localizer) : Controller
{
    [HttpGet("/servers")]
    public async Task<IActionResult> Index(bool cards = false, CancellationToken cancellationToken = default)
    {
        ViewBag.Server = "active";

        var servers = await liveViewService.EnabledServersAsync(cancellationToken);

        if (servers.Count == 1 && !cards)
            return View("Live", await liveViewService.LiveAsync(servers[0], cancellationToken));

        var model = await liveViewService.ListAsync(cancellationToken);

        return cards ? PartialView("_Cards", model) : View(model);
    }

    [HttpGet("/servers/{publicId}")]
    public async Task<IActionResult> Live(string publicId, CancellationToken cancellationToken)
    {
        ViewBag.Server = "active";

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

        var feed = await liveViewService.FeedAsync(server, after, cancellationToken);

        if (feed.Items.Count == 0)
            return NoContent();

        return PartialView("_Feed", feed);
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

        var name = string.IsNullOrWhiteSpace(currentUser.Name) ? await playerNameService.KnownNameAsync(communityId.Value, cancellationToken) : currentUser.Name;
        var user = new SiteChatUser(communityId.Value, name);
        var result = await siteChatService.SendAsync(server, user, message, cancellationToken);
        var limits = await settingsService.GetAsync<SiteChatSettings>(cancellationToken);
        var answer = new SiteChatCommandResult { Success = result.Success, ErrorMessage = ErrorMessage(result, limits) };

        return result.Error == SiteChatError.Banned ? StatusCode(StatusCodes.Status403Forbidden, answer) : Ok(answer);
    }

    [HttpGet("/server")]
    public IActionResult Legacy()
    {
        return RedirectPermanent(AppConsts.ServersUrl);
    }

    [HttpGet("/server/{partial:regex(^(header|players|feed)$)}")]
    public async Task<IActionResult> LegacyPartial(string partial, CancellationToken cancellationToken)
    {
        var server = await liveViewService.MainServerAsync(cancellationToken);
        if (server == null)
            return NotFound();

        return Redirect($"{AppConsts.ServersUrl}/{server.Server.PublicId}/{partial}{Request.QueryString}");
    }

    private string? ErrorMessage(SiteChatResult result, SiteChatSettings limits)
    {
        return result.Error switch
        {
            null => null,
            SiteChatError.EmptyMessage => localizer["Write a message first."].Value,
            SiteChatError.MessageTooLong => localizer["The message cannot be longer than {0} characters.", limits.MaxMessageLength].Value,
            SiteChatError.RateLimited => localizer["Too many messages. Please wait a moment."].Value,
            SiteChatError.Banned when !string.IsNullOrWhiteSpace(result.Reason) => $"{localizer["You have been banned from the chat."].Value} {localizer["Reason"].Value}: {result.Reason}",
            SiteChatError.Banned => localizer["You have been banned from the chat."].Value,
            _ => localizer["The chat is turned off."].Value
        };
    }
}
