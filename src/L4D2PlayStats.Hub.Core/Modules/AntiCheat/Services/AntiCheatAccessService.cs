using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Enums;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Extensions;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Models;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players;
using L4D2PlayStats.Hub.Core.Modules.Players.Extensions;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
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
    ILogger<AntiCheatAccessService> logger,
    IForcedNameStore forcedNames) : IAntiCheatAccessService
{
    public async Task<AntiCheatAccessSnapshot> GetAsync(CancellationToken cancellationToken = default)
    {
        var setting = await SettingAsync(cancellationToken);
        var whitelist = await PlayersAsync<AntiCheatWhitelistPlayer>(cancellationToken);
        var required = await PlayersAsync<AntiCheatRequiredPlayer>(cancellationToken);

        return Snapshot(setting, whitelist, required);
    }

    public async Task<AntiCheatAccessSnapshot> ModeAsync(CancellationToken cancellationToken = default)
    {
        return Snapshot(await SettingAsync(cancellationToken), [], []);
    }

    public Task<Page<AntiCheatAccessPlayer>> PageAsync(AntiCheatAccessList list, PlayerSearch search, int page, int size, CancellationToken cancellationToken = default)
    {
        return list == AntiCheatAccessList.Whitelist
            ? PageAsync<AntiCheatWhitelistPlayer>(search, page, size, cancellationToken)
            : PageAsync<AntiCheatRequiredPlayer>(search, page, size, cancellationToken);
    }

    public async Task<IReadOnlyList<long>> IdsAsync(AntiCheatAccessList list, CancellationToken cancellationToken = default)
    {
        return list == AntiCheatAccessList.Whitelist
            ? await dbContext.Set<AntiCheatWhitelistPlayer>().AsNoTracking().Select(player => player.CommunityId).ToListAsync(cancellationToken)
            : await dbContext.Set<AntiCheatRequiredPlayer>().AsNoTracking().Select(player => player.CommunityId).ToListAsync(cancellationToken);
    }

    public async Task SetModeAsync(AntiCheatAccessMode mode, AntiCheatAccessActor actor, AntiCheatAccessSource source, CancellationToken cancellationToken = default)
    {
        var actorName = ActorName(actor, source);

        await using (await hubState.LockAsync(cancellationToken))
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
    }

    public async Task<AntiCheatAccessAddResult?> AddAsync(AntiCheatAccessList list, long communityId, string? name, AntiCheatAccessActor actor, AntiCheatAccessSource source, CancellationToken cancellationToken = default)
    {
        if (!SteamIds.IsValid(communityId))
            return null;

        var actorName = ActorName(actor, source);

        await using (await hubState.LockAsync(cancellationToken))
        {
            var existing = await FindAsync(list, communityId, cancellationToken);
            if (existing != null)
                return new AntiCheatAccessAddResult(existing, false);

            AntiCheatAccessPlayer player = list == AntiCheatAccessList.Whitelist ? new AntiCheatWhitelistPlayer() : new AntiCheatRequiredPlayer();

            player.CommunityId = communityId;
            player.Name = Text.Clip(name, AppConsts.AntiCheatAccessNameMaxLength) ?? Text.Clip(await playerNameService.KnownNameAsync(communityId, cancellationToken), AppConsts.AntiCheatAccessNameMaxLength);
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
    }

    public async Task<bool> RemoveAsync(AntiCheatAccessList list, long communityId, AntiCheatAccessActor actor, AntiCheatAccessSource source, CancellationToken cancellationToken = default)
    {
        var actorName = ActorName(actor, source);

        await using (await hubState.LockAsync(cancellationToken))
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
    }

    private async Task<AntiCheatAccessPlayer?> FindAsync(AntiCheatAccessList list, long communityId, CancellationToken cancellationToken)
    {
        return list == AntiCheatAccessList.Whitelist
            ? await dbContext.Set<AntiCheatWhitelistPlayer>().FirstOrDefaultAsync(player => player.CommunityId == communityId, cancellationToken)
            : await dbContext.Set<AntiCheatRequiredPlayer>().FirstOrDefaultAsync(player => player.CommunityId == communityId, cancellationToken);
    }

    private Task<AntiCheatAccessSetting?> SettingAsync(CancellationToken cancellationToken)
    {
        return dbContext.Set<AntiCheatAccessSetting>().AsNoTracking().FirstOrDefaultAsync(entry => entry.Id == AppConsts.AntiCheatAccessSettingId, cancellationToken);
    }

    private AntiCheatAccessSnapshot Snapshot(AntiCheatAccessSetting? setting, IReadOnlyList<AntiCheatAccessPlayer> whitelist, IReadOnlyList<AntiCheatAccessPlayer> required)
    {
        return new AntiCheatAccessSnapshot
        {
            Mode = setting?.Mode ?? AppConsts.AntiCheatDefaultMode,
            ModeSaved = setting != null,
            UpdatedByName = forcedNames.Apply(setting?.UpdatedByCommunityId, setting?.UpdatedByName),
            UpdatedFrom = setting?.UpdatedFrom,
            UpdatedAtUtc = setting?.UpdatedAtUtc,
            Whitelist = whitelist,
            Required = required
        };
    }

    private async Task<Page<AntiCheatAccessPlayer>> PageAsync<T>(PlayerSearch search, int page, int size, CancellationToken cancellationToken) where T : AntiCheatAccessPlayer
    {
        var forced = dbContext.Set<ForcedName>();

        var players = await search
            .Apply(dbContext.Set<T>().AsNoTracking(), player => player.CommunityId, player => forced.Where(entry => entry.CommunityId == player.CommunityId).Select(entry => entry.Name).FirstOrDefault() ?? player.Name)
            .OrderByDescending(player => player.CreatedAtUtc)
            .ThenBy(player => player.CommunityId)
            .ToPageAsync(page, size, cancellationToken);

        return players.Map(Displayed);
    }

    private AntiCheatAccessPlayer Displayed(AntiCheatAccessPlayer player)
    {
        player.Name = forcedNames.Apply(player.CommunityId, player.Name);
        player.AddedByName = forcedNames.Apply(player.AddedByCommunityId, player.AddedByName);

        return player;
    }

    private async Task<IReadOnlyList<AntiCheatAccessPlayer>> PlayersAsync<T>(CancellationToken cancellationToken) where T : AntiCheatAccessPlayer
    {
        var players = await dbContext.Set<T>()
            .AsNoTracking()
            .OrderByDescending(player => player.CreatedAtUtc)
            .ThenBy(player => player.CommunityId)
            .ToListAsync(cancellationToken);

        return [.. players.Select(Displayed)];
    }

    private async Task RequestServerSyncAsync(AntiCheatAccessActor actor, string? actorName, AntiCheatAccessSource source, CancellationToken cancellationToken)
    {
        if (source != AntiCheatAccessSource.Site || actor.CommunityId == null)
            return;

        await remoteCommandService.QueueInternalAsync(actor.CommunityId.Value, actorName, AppConsts.AntiCheatSyncCommand, cancellationToken);
    }

    private static string? ActorName(AntiCheatAccessActor actor, AntiCheatAccessSource source)
    {
        var name = Text.Clip(actor.Name, AppConsts.AntiCheatAccessNameMaxLength);

        return name == null && source == AntiCheatAccessSource.Game ? AppConsts.AntiCheatGameAdminName : name;
    }
}
