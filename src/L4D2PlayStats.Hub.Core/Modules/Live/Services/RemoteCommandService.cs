using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Administrators.Services;
using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
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

    public async Task<ServerCommand?> DequeueAsync(GameServer server, CancellationToken cancellationToken = default)
    {
        if (!await EnabledAsync(cancellationToken))
            return null;

        var state = store.For(server.Id);
        var queued = state.Dequeue();
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

    public Task<List<RemoteCommandLog>> ListAsync(int count, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<RemoteCommandLog>()
            .AsNoTracking()
            .OrderByDescending(entry => entry.CreatedAtUtc)
            .ThenByDescending(entry => entry.Id)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    private async Task<bool> EnabledAsync(CancellationToken cancellationToken)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);

        return modules.IsEnabled(HubModule.RemoteCommands);
    }
}
