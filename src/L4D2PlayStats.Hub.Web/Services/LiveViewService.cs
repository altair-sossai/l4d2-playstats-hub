using L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Campaigns.Services;
using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication;
using L4D2PlayStats.Hub.Web.Models.Live;

namespace L4D2PlayStats.Hub.Web.Services;

public class LiveViewService(
    IGameServerService gameServerService,
    IGameServerPresence presence,
    ILiveDataService liveDataService,
    ICampaignService campaignService,
    ISettingsService settingsService,
    IChatBanService chatBanService,
    ICurrentUser currentUser,
    ICampaignImageService campaignImageService) : ILiveViewService
{
    public async Task<GameServer?> FindServerAsync(string publicId, CancellationToken cancellationToken = default)
    {
        var server = await gameServerService.FindAsync(publicId, cancellationToken);

        return server is { Enabled: true } ? server : null;
    }

    public async Task<List<GameServer>> EnabledServersAsync(CancellationToken cancellationToken = default)
    {
        var servers = await gameServerService.ListAsync(cancellationToken);

        return [.. servers.Where(server => server.Enabled)];
    }

    public async Task<ServerListViewModel> ListAsync(CancellationToken cancellationToken = default)
    {
        var cards = new List<ServerCardViewModel>();

        foreach (var server in await EnabledServersAsync(cancellationToken))
            cards.Add(await CardAsync(server, cancellationToken));

        return new ServerListViewModel { Servers = cards };
    }

    public async Task<ServerCardViewModel?> MainServerAsync(CancellationToken cancellationToken = default)
    {
        var list = await ListAsync(cancellationToken);

        return list.Servers
            .OrderByDescending(server => server.Players)
            .ThenByDescending(server => server.Online)
            .FirstOrDefault();
    }

    public async Task<ServerCardViewModel> CardAsync(GameServer server, CancellationToken cancellationToken = default)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        var catalog = await campaignService.GetCatalogAsync(cancellationToken);
        var status = presence.GetStatus(server);
        var map = status.Signal?.Map;
        var campaign = string.IsNullOrEmpty(map) ? null : catalog.FindUsingMapName(map)?.Name;

        return new ServerCardViewModel
        {
            Server = server,
            Status = status,
            Snapshot = liveDataService.Snapshot(server),
            Campaign = campaign,
            CampaignImage = campaignImageService.Url(campaign),
            AntiCheatPage = modules.IsEnabled(HubModule.AntiCheat)
        };
    }

    public async Task<LiveServerViewModel> LiveAsync(GameServer server, CancellationToken cancellationToken = default)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);
        var communityId = currentUser.CommunityId;
        var chatEnabled = modules.IsEnabled(HubModule.SiteChat);

        return new LiveServerViewModel
        {
            Card = await CardAsync(server, cancellationToken),
            Servers = await EnabledServersAsync(cancellationToken),
            Feed = await FeedAsync(server, 0, cancellationToken),
            ChatEnabled = chatEnabled,
            RemoteCommandsEnabled = modules.IsEnabled(HubModule.RemoteCommands),
            ChatBan = chatEnabled && communityId != null ? await chatBanService.FindAsync(communityId.Value, cancellationToken) : null,
            Administrator = await currentUser.IsAdministratorAsync(),
            SiteName = site.Name
        };
    }

    public async Task<LiveFeedViewModel> FeedAsync(GameServer server, long after, CancellationToken cancellationToken = default)
    {
        return new LiveFeedViewModel
        {
            PublicId = server.PublicId,
            Items = liveDataService.FeedAfter(server, after),
            TimeZone = await TimeZoneAsync(cancellationToken)
        };
    }

    public async Task<TimeZoneInfo> TimeZoneAsync(CancellationToken cancellationToken = default)
    {
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);

        return SiteTime.Zone(site.TimeZone);
    }
}
