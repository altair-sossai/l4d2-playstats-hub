using L4D2PlayStats.Hub.Core.Modules.Live.Settings;
using L4D2PlayStats.Hub.Core.Modules.Administrators.Services;
using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public class SiteChatService(
    ILiveServerStore store,
    IChatBanService chatBanService,
    IAdministratorService administratorService,
    ISettingsService settingsService,
    IRemoteCommandService remoteCommandService) : ISiteChatService
{
    public async Task<SiteChatResult> SendAsync(GameServer server, SiteChatUser user, string? message, CancellationToken cancellationToken = default)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        if (!modules.IsEnabled(HubModule.SiteChat))
            return SiteChatResult.Failed(SiteChatError.Disabled);

        var ban = await chatBanService.FindAsync(user.CommunityId, cancellationToken);
        if (ban != null)
            return SiteChatResult.Failed(SiteChatError.Banned, ban.Reason);

        var administrator = await administratorService.IsAdministratorAsync(user.CommunityId, cancellationToken);
        var state = store.For(server.Id);
        var limits = await settingsService.GetAsync<SiteChatSettings>(cancellationToken);
        var result = state.AddSiteMessage(user, message, administrator, limits);

        if (result.Success && administrator)
            await remoteCommandService.QueueAsync(server, user.CommunityId, user.Name, message, cancellationToken);

        return result;
    }

    public async Task<List<RecentChatSender>> RecentSendersAsync(CancellationToken cancellationToken = default)
    {
        var bans = await chatBanService.ListAsync(cancellationToken);
        var banned = bans.Select(ban => ban.CommunityId).ToHashSet();

        return
        [
            .. store.States
                .SelectMany(state => state.SiteMessages)
                .GroupBy(message => message.UserId)
                .Select(group => group.OrderByDescending(message => message.When).First())
                .OrderByDescending(message => message.When)
                .Take(AppConsts.SiteChatRecentSenders)
                .Select(message => new RecentChatSender(message.UserId, message.Name, message.Text, message.When, banned.Contains(message.UserId)))
        ];
    }
}
