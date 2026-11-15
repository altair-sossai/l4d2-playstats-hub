using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Live;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[Authorize(Policy = AppConsts.AdministratorPolicy)]
[AutoValidateAntiforgeryToken]
[RequireModule(HubModule.SiteChat)]
[Route("admin/chat-bans")]
public class ChatBansController(IChatBanService chatBanService, ISiteChatService siteChatService, ICurrentUser currentUser) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new ChatBansViewModel
        {
            Bans = await chatBanService.ListAsync(cancellationToken),
            RecentSenders = await siteChatService.RecentSendersAsync(cancellationToken)
        };

        return View(model);
    }

    [HttpPost("")]
    public async Task<IActionResult> Save(long communityId, string? name, string? reason, CancellationToken cancellationToken)
    {
        var ban = await chatBanService.SaveAsync(communityId, name, reason, currentUser.CommunityId, currentUser.Name, cancellationToken);

        if (ban == null)
            TempData["Error"] = "That is not a valid SteamID64.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{communityId:long}/delete")]
    public async Task<IActionResult> Remove(long communityId, CancellationToken cancellationToken)
    {
        await chatBanService.RemoveAsync(communityId, cancellationToken);

        return RedirectToAction(nameof(Index));
    }
}
