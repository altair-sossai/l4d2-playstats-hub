using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Administrators.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public class RemoteCommandService(
    HubDbContext dbContext,
    ILiveServerStore store,
    ISettingsService settingsService,
    IAdministratorService administratorService,
    IClock clock) : IRemoteCommandService
{
    public async Task<RemoteCommandResult> QueueAsync(GameServer server, long administratorId, string? administratorName, string? command, CancellationToken cancellationToken = default)
    {
        if (!await EnabledAsync(cancellationToken))
            return new RemoteCommandResult(RemoteCommandStatus.ModuleDisabled);

        if (!await administratorService.IsAdministratorAsync(administratorId, cancellationToken))
            return new RemoteCommandResult(RemoteCommandStatus.NotAdministrator);

        var serverCommand = ServerCommand.Parse(command);
        if (serverCommand == null)
            return new RemoteCommandResult(RemoteCommandStatus.InvalidCommand);

        var log = new RemoteCommandLog
        {
            GameServerId = server.Id,
            GameServerName = server.Name,
            AdministratorCommunityId = administratorId,
            AdministratorName = string.IsNullOrWhiteSpace(administratorName) ? null : administratorName.Trim(),
            Command = serverCommand.FullCommand,
            CreatedAtUtc = clock.UtcNow
        };

        dbContext.Set<RemoteCommandLog>().Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);

        var state = store.For(server.Id);
        state.Enqueue(new QueuedCommand(log.Id, serverCommand, log.CreatedAtUtc));

        return new RemoteCommandResult(RemoteCommandStatus.Queued, log);
    }

    public async Task<int> QueueInternalAsync(long administratorId, string? administratorName, string command, CancellationToken cancellationToken = default)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        var serverCommand = ServerCommand.Parse(command);

        if (!modules.IsEnabled(HubModule.LiveServer) || serverCommand == null)
            return 0;

        var servers = await dbContext.GameServers.AsNoTracking().Where(server => server.Enabled).OrderBy(server => server.Id).ToListAsync(cancellationToken);
        var now = clock.UtcNow;
        var queued = new List<(LiveServerState State, RemoteCommandLog Log)>();

        foreach (var server in servers)
        {
            var state = store.For(server.Id);
            if (state.HasInternal(serverCommand.FullCommand))
                continue;

            var log = new RemoteCommandLog
            {
                GameServerId = server.Id,
                GameServerName = server.Name,
                AdministratorCommunityId = administratorId,
                AdministratorName = string.IsNullOrWhiteSpace(administratorName) ? null : administratorName.Trim(),
                Command = serverCommand.FullCommand,
                CreatedAtUtc = now
            };

            dbContext.Set<RemoteCommandLog>().Add(log);
            queued.Add((state, log));
        }

        if (queued.Count == 0)
            return 0;

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var (state, log) in queued)
            state.EnqueueInternal(new QueuedCommand(log.Id, serverCommand, log.CreatedAtUtc));

        return queued.Count;
    }

    public async Task<ServerCommand?> DequeueAsync(GameServer server, CancellationToken cancellationToken = default)
    {
        var state = store.For(server.Id);
        var queued = state.DequeueInternal();

        if (queued == null && await EnabledAsync(cancellationToken))
            queued = state.Dequeue();

        if (queued == null)
            return null;

        var log = await dbContext.Set<RemoteCommandLog>().FirstOrDefaultAsync(entry => entry.Id == queued.LogId, cancellationToken);
        if (log != null)
        {
            log.DeliveredAtUtc = clock.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return queued.Command;
    }

    public Task<Page<RemoteCommandLog>> PageAsync(int page, int size, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<RemoteCommandLog>()
            .AsNoTracking()
            .OrderByDescending(entry => entry.CreatedAtUtc)
            .ThenByDescending(entry => entry.Id)
            .ToPageAsync(page, size, cancellationToken);
    }

    public Task<int> DeliveredCountAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<RemoteCommandLog>()
            .AsNoTracking()
            .Where(entry => entry.DeliveredAtUtc != null)
            .CountAsync(cancellationToken);
    }

    private async Task<bool> EnabledAsync(CancellationToken cancellationToken)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);

        return modules.IsEnabled(HubModule.RemoteCommands);
    }
}
