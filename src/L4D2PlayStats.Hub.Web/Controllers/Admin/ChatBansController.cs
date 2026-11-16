using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Live;
using L4D2PlayStats.Hub.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[Authorize(Policy = AppConsts.AdministratorPolicy)]
[RequireModule(HubModule.SiteChat)]
public class ChatBansController(IChatBanService chatBanService, ILiveViewService liveViewService) : Controller
{
    [HttpGet("/admin/chat-bans")]
    public async Task<IActionResult> Index(string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        ViewBag.Admin = "active";
        ViewBag.ChatBans = "active";

        var bans = await chatBanService.ListAsync(cancellationToken);
        var term = q?.Trim();

        var filtered = bans
            .Select(ban => new ChatBanResult(ban))
            .Where(ban => string.IsNullOrEmpty(term)
                          || ban.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                          || ban.CommunityId.Contains(term, StringComparison.OrdinalIgnoreCase)
                          || ban.SteamId.Contains(term, StringComparison.OrdinalIgnoreCase)
                          || (ban.Reason?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();

        var timeZone = await liveViewService.TimeZoneAsync(cancellationToken);
        var model = new ChatBansModel(new PagedListModel<ChatBanResult>(filtered, page, AppConsts.ChatBansPageSize), q, bans.Count, timeZone);

        return View(model);
    }

    [HttpGet("/chat-bans")]
    public IActionResult Legacy()
    {
        return RedirectPermanent($"{AppConsts.ChatBansUrl}{Request.QueryString}");
    }
}
