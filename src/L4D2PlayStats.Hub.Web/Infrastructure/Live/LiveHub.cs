using L4D2PlayStats.Hub.Core.Modules.Servers.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using Microsoft.AspNetCore.SignalR;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Live;

public class LiveHub(IGameServerService gameServerService, ISettingsService settingsService, LiveWatchers watchers, LiveNotifier notifier) : Microsoft.AspNetCore.SignalR.Hub
{
    public async Task<bool> Watch(string? publicId)
    {
        var previous = watchers.Remove(Context.ConnectionId);

        if (previous != null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, LiveWatchers.Group(previous.PublicId), Context.ConnectionAborted);

        if (string.IsNullOrWhiteSpace(publicId))
            return false;

        var modules = await settingsService.GetAsync<ModuleSettings>(Context.ConnectionAborted);
        if (!modules.IsEnabled(HubModule.LiveServer))
            return false;

        var server = await gameServerService.FindAsync(publicId, Context.ConnectionAborted);
        if (server is not { Enabled: true })
            return false;

        await Groups.AddToGroupAsync(Context.ConnectionId, LiveWatchers.Group(server.PublicId), Context.ConnectionAborted);

        watchers.Add(Context.ConnectionId, server);
        notifier.Track(server);

        return true;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        watchers.Remove(Context.ConnectionId);

        return base.OnDisconnectedAsync(exception);
    }
}
