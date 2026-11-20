using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Enums;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Extensions;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Models;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace L4D2PlayStats.Hub.Core.Modules.AntiCheat.Services;

public class AntiCheatAccessService(
    HubDbContext dbContext,
    HubState hubState,
    IRemoteCommandService remoteCommandService,
    IPlayerNameService playerNameService,
    IClock clock,
    ILogger<AntiCheatAccessService> logger) : IAntiCheatAccessService
{
    public async Task<AntiCheatAccessSnapshot> GetAsync(CancellationToken cancellationToken = default)
    {
        var setting = await dbContext.Set<AntiCheatAccessSetting>().AsNoTracking().FirstOrDefaultAsync(entry => entry.Id == AppConsts.AntiCheatAccessSettingId, cancellationToken);
        var whitelist = await PlayersAsync<AntiCheatWhitelistPlayer>(cancellationToken);
        var required = await PlayersAsync<AntiCheatRequiredPlayer>(cancellationToken);

        return new AntiCheatAccessSnapshot
        {
            Mode = setting?.Mode ?? AppConsts.AntiCheatDefaultMode,
            ModeSaved = setting != null,
            UpdatedByName = setting?.UpdatedByName,
            UpdatedFrom = setting?.UpdatedFrom,
            UpdatedAtUtc = setting?.UpdatedAtUtc,
            Whitelist = whitelist,
            Required = required
        };
    }

    public async Task SetModeAsync(AntiCheatAccessMode mode, AntiCheatAccessActor actor, AntiCheatAccessSource source, CancellationToken cancellationToken = default)
    {
        var actorName = ActorName(actor, source);

        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            var setting = await dbContext.Set<AntiCheatAccessSetting>().FirstOrDefaultAsync(entry => entry.Id == AppConsts.AntiCheatAccessSettingId, cancellationToken);

            if (setting == null)
            {
                setting = new AntiCheatAccessSetting { Id = AppConsts.AntiCheatAccessSettingId };
                dbContext.Add(setting);
            }

            setting.Mode = mode;
            setting.UpdatedByCommunityId = actor.CommunityId;
            setting.UpdatedByName = actorName;
            setting.UpdatedFrom = source;
            setting.UpdatedAtUtc = clock.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Anti-cheat mode set to {Mode} by {Admin} ({AdminCommunityId}) from the {Source}", mode.ToName(), actorName, actor.CommunityId, source);

            await RequestServerSyncAsync(actor, actorName, source, cancellationToken);
        }
        finally
        {
            hubState.WriteLock.Release();
        }
    }

    public async Task<AntiCheatAccessAddResult?> AddAsync(AntiCheatAccessList list, long communityId, string? name, AntiCheatAccessActor actor, AntiCheatAccessSource source, CancellationToken cancellationToken = default)
    {
        if (!SteamIds.IsValid(communityId))
            return null;

        var actorName = ActorName(actor, source);

        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            var existing = await FindAsync(list, communityId, cancellationToken);
            if (existing != null)
                return new AntiCheatAccessAddResult(existing, false);

            AntiCheatAccessPlayer player = list == AntiCheatAccessList.Whitelist ? new AntiCheatWhitelistPlayer() : new AntiCheatRequiredPlayer();

            player.CommunityId = communityId;
            player.Name = Truncate(name) ?? Truncate(await playerNameService.KnownNameAsync(communityId, cancellationToken));
            player.AddedByCommunityId = actor.CommunityId;
            player.AddedByName = actorName;
            player.AddedFrom = source;
            player.CreatedAtUtc = clock.UtcNow;

            dbContext.Add(player);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Player {CommunityId} added to the anti-cheat {List} by {Admin} ({AdminCommunityId}) from the {Source}", communityId, list.ToName(), actorName, actor.CommunityId, source);

            await RequestServerSyncAsync(actor, actorName, source, cancellationToken);

            return new AntiCheatAccessAddResult(player, true);
        }
        finally
        {
            hubState.WriteLock.Release();
        }
    }

    public async Task<bool> RemoveAsync(AntiCheatAccessList list, long communityId, AntiCheatAccessActor actor, AntiCheatAccessSource source, CancellationToken cancellationToken = default)
    {
        var actorName = ActorName(actor, source);

        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            var player = await FindAsync(list, communityId, cancellationToken);
            if (player == null)
                return false;

            dbContext.Remove(player);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Player {CommunityId} removed from the anti-cheat {List} by {Admin} ({AdminCommunityId}) from the {Source}", communityId, list.ToName(), actorName, actor.CommunityId, source);

            await RequestServerSyncAsync(actor, actorName, source, cancellationToken);

            return true;
        }
        finally
        {
            hubState.WriteLock.Release();
        }
    }

    private async Task<AntiCheatAccessPlayer?> FindAsync(AntiCheatAccessList list, long communityId, CancellationToken cancellationToken)
    {
        return list == AntiCheatAccessList.Whitelist
            ? await dbContext.Set<AntiCheatWhitelistPlayer>().FirstOrDefaultAsync(player => player.CommunityId == communityId, cancellationToken)
            : await dbContext.Set<AntiCheatRequiredPlayer>().FirstOrDefaultAsync(player => player.CommunityId == communityId, cancellationToken);
    }

    private async Task<IReadOnlyList<AntiCheatAccessPlayer>> PlayersAsync<T>(CancellationToken cancellationToken) where T : AntiCheatAccessPlayer
    {
        var players = await dbContext.Set<T>()
            .AsNoTracking()
            .OrderByDescending(player => player.CreatedAtUtc)
            .ThenBy(player => player.CommunityId)
            .ToListAsync(cancellationToken);

        return [.. players];
    }

    private async Task RequestServerSyncAsync(AntiCheatAccessActor actor, string? actorName, AntiCheatAccessSource source, CancellationToken cancellationToken)
    {
        if (source != AntiCheatAccessSource.Site || actor.CommunityId == null)
            return;

        await remoteCommandService.QueueInternalAsync(actor.CommunityId.Value, actorName, AppConsts.AntiCheatSyncCommand, cancellationToken);
    }

    private static string? ActorName(AntiCheatAccessActor actor, AntiCheatAccessSource source)
    {
        var name = Truncate(actor.Name);

        return name == null && source == AntiCheatAccessSource.Game ? AppConsts.AntiCheatGameAdminName : name;
    }

    private static string? Truncate(string? value)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
            return null;

        return trimmed.Length > AppConsts.AntiCheatAccessNameMaxLength ? trimmed[..AppConsts.AntiCheatAccessNameMaxLength] : trimmed;
    }
}
