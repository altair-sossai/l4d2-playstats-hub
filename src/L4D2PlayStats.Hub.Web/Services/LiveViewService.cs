using L4D2PlayStats.Hub.Core.Modules.Campaigns;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Web.Infrastructure;
using L4D2PlayStats.Hub.Web.Models.Live;

namespace L4D2PlayStats.Hub.Web.Services;

public class LiveViewService(
    IGameServerService gameServerService,
    IGameServerPresence presence,
    ILiveDataService liveDataService,
    ICampaignService campaignService,
    ISettingsService settingsService,
    IChatBanService chatBanService,
    ICurrentUser currentUser) : ILiveViewService
{
    public async Task<GameServer?> FindServerAsync(string publicId, CancellationToken cancellationToken = default)
    {
        var server = await gameServerService.FindAsync(publicId, cancellationToken);

        return server is { Enabled: true } ? server : null;
    }

    public async Task<ServerListViewModel> ListAsync(CancellationToken cancellationToken = default)
    {
        var cards = new List<ServerCardViewModel>();

        foreach (var server in await EnabledServersAsync(cancellationToken))
            cards.Add(await CardAsync(server, cancellationToken));

        return new ServerListViewModel { Servers = cards };
    }

    public async Task<ServerCardViewModel> CardAsync(GameServer server, CancellationToken cancellationToken = default)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        var catalog = await campaignService.GetCatalogAsync(cancellationToken);
        var status = presence.GetStatus(server);
        var map = status.Signal?.Map;

        return new ServerCardViewModel
        {
            Server = server,
            Status = status,
            Snapshot = liveDataService.Snapshot(server),
            Campaign = string.IsNullOrEmpty(map) ? null : catalog.FindUsingMapName(map)?.Name,
            RankingEnabled = modules.IsEnabled(HubModule.Ranking)
        };
    }

    public async Task<LiveServerViewModel> LiveAsync(GameServer server, CancellationToken cancellationToken = default)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);

        return new LiveServerViewModel
        {
            Card = await CardAsync(server, cancellationToken),
            Servers = await EnabledServersAsync(cancellationToken),
            Feed = await FeedAsync(server, 0, cancellationToken),
            Chat = modules.IsEnabled(HubModule.SiteChat) ? await ChatAsync(server, null, null, cancellationToken) : null
        };
    }

    public async Task<LiveFeedViewModel> FeedAsync(GameServer server, long after, CancellationToken cancellationToken = default)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);
        var items = liveDataService.FeedAfter(server, after);

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(site.TimeZone, out var timeZone))
            timeZone = TimeZoneInfo.Utc;

        return new LiveFeedViewModel
        {
            PublicId = server.PublicId,
            Items = [.. items.Reverse()],
            After = items.Count == 0 ? after : items.Max(item => item.Sequence),
            TimeZone = timeZone,
            RankingEnabled = modules.IsEnabled(HubModule.Ranking)
        };
    }

    public async Task<SiteChatFormViewModel> ChatAsync(GameServer server, SiteChatResult? result, string? message, CancellationToken cancellationToken = default)
    {
        var communityId = currentUser.CommunityId;

        return new SiteChatFormViewModel
        {
            PublicId = server.PublicId,
            SignedIn = communityId != null,
            Ban = communityId == null ? null : await chatBanService.FindAsync(communityId.Value, cancellationToken),
            Result = result,
            Message = message
        };
    }

    private async Task<List<GameServer>> EnabledServersAsync(CancellationToken cancellationToken)
    {
        var servers = await gameServerService.ListAsync(cancellationToken);

        return [.. servers.Where(server => server.Enabled)];
    }
}
