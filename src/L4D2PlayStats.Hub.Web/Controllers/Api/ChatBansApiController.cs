using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using L4D2PlayStats.Hub.Web.Models.Live;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[Route("api/chat-bans")]
[Authorize(Policy = AppConsts.AdministratorPolicy)]
[AutoValidateAntiforgeryToken]
[RequireModule(HubModule.SiteChat)]
[ApiController]
public class ChatBansApiController(
    IChatBanService chatBanService,
    ISiteChatService siteChatService,
    ICurrentUser currentUser,
    IStringLocalizer<SharedResource> localizer) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var bans = await chatBanService.ListAsync(cancellationToken);

        return Ok(bans.Select(ban => new ChatBanResult(ban)));
    }

    [HttpGet("recent-senders")]
    public async Task<IActionResult> GetRecentSendersAsync(CancellationToken cancellationToken)
    {
        var senders = await siteChatService.RecentSendersAsync(cancellationToken);

        return Ok(senders.Select(sender => new RecentChatSenderResult(sender)));
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] ChatBanCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.CommunityId))
            return BadRequest(new { errorMessage = localizer["The Steam ID must be provided."].Value });

        var communityId = SteamIdParser.Parse(command.CommunityId);
        var ban = communityId == null ? null : await chatBanService.SaveAsync(communityId.Value, command.Name, command.Reason, currentUser.CommunityId, currentUser.Name, cancellationToken);

        if (ban == null)
            return BadRequest(new { errorMessage = localizer["The Steam ID is invalid."].Value });

        return Ok(new ChatBanResult(ban));
    }

    [HttpDelete("{communityId:long}")]
    public async Task<IActionResult> DeleteAsync(long communityId, CancellationToken cancellationToken)
    {
        await chatBanService.RemoveAsync(communityId, cancellationToken);

        return NoContent();
    }
}