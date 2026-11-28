using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Bans.Enums;
using L4D2PlayStats.Hub.Core.Modules.Bans.Extensions;
using L4D2PlayStats.Hub.Core.Modules.Bans.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Bans.Models;
using L4D2PlayStats.Hub.Core.Modules.Bans.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Enums;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Models;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Bans.Services;

public class BanService(
    HubDbContext dbContext,
    IClock clock,
    IPlayerNameService playerNameService,
    IBanAlertService alertService,
    IRemoteCommandService remoteCommandService,
    INotificationService notifications) : IBanService
{
    public Task<Ban?> FindAsync(int banId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Ban>().AsNoTracking().FirstOrDefaultAsync(ban => ban.Id == banId, cancellationToken);
    }

    public Task<Ban?> ActiveAsync(long communityId, CancellationToken cancellationToken = default)
    {
        return Active().AsNoTracking().Where(ban => ban.CommunityId == communityId).OrderBy(ban => ban.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<HashSet<long>> BannedAsync(IReadOnlyCollection<long> communityIds, CancellationToken cancellationToken = default)
    {
        List<long> ids = [.. communityIds.Distinct()];

        if (ids.Count == 0)
            return [];

        var banned = await Active().AsNoTracking().Where(ban => ids.Contains(ban.CommunityId)).Select(ban => ban.CommunityId).ToListAsync(cancellationToken);

        return [.. banned];
    }

    public Task<Page<Ban>> PageAsync(PlayerSearch search, BanListFilter filter, int page, int size, CancellationToken cancellationToken = default)
    {
        return search
            .Apply(Filtered(filter), ban => ban.CommunityId, ban => ban.Name, ban => ban.Reason)
            .OrderByDescending(ban => ban.CreatedAtUtc)
            .ThenByDescending(ban => ban.Id)
            .ToPageAsync(page, size, cancellationToken);
    }

    public Task<int> CountAsync(BanListFilter filter, CancellationToken cancellationToken = default)
    {
        return Filtered(filter).CountAsync(cancellationToken);
    }

    public Task<List<Ban>> HistoryAsync(long communityId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Ban>()
            .AsNoTracking()
            .Where(ban => ban.CommunityId == communityId)
            .OrderByDescending(ban => ban.CreatedAtUtc)
            .ThenByDescending(ban => ban.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<BanSaveResult> AddAsync(BanRequest request, BanActor actor, CancellationToken cancellationToken = default)
    {
        if (!SteamIds.IsValid(request.CommunityId))
            return BanSaveResult.InvalidPlayer;

        var now = clock.UtcNow;

        if (request.EndsAtUtc <= now)
            return BanSaveResult.InvalidEnd;

        if (await Active().AnyAsync(ban => ban.CommunityId == request.CommunityId, cancellationToken))
            return BanSaveResult.AlreadyBanned;

        var ban = new Ban
        {
            CommunityId = request.CommunityId,
            Name = Text.Clip(request.Name, AppConsts.LivePlayerNameMaxLength) ?? Text.Clip(await playerNameService.RealNameAsync(request.CommunityId, cancellationToken), AppConsts.LivePlayerNameMaxLength),
            Reason = Text.Clip(request.Reason, AppConsts.BanReasonMaxLength),
            Source = BanSource.Site,
            RelatedToBanId = request.RelatedToBanId,
            CreatedByCommunityId = actor.CommunityId,
            CreatedByName = Text.Clip(actor.Name, AppConsts.LivePlayerNameMaxLength),
            CreatedAtUtc = now,
            EndsAtUtc = request.EndsAtUtc
        };

        dbContext.Set<Ban>().Add(ban);
        Banned(ban);
        await dbContext.SaveChangesAsync(cancellationToken);
        await alertService.ResolveAsync(ban.CommunityId, actor.CommunityId, cancellationToken);
        await SyncAsync(ban.CommunityId, actor, cancellationToken);

        return BanSaveResult.Saved;
    }

    public async Task<BanSaveResult> EditAsync(int banId, DateTime? endsAtUtc, string? reason, BanActor actor, CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        var ban = await Active().FirstOrDefaultAsync(item => item.Id == banId, cancellationToken);

        if (ban == null)
            return BanSaveResult.NotFound;

        if (endsAtUtc <= now)
            return BanSaveResult.InvalidEnd;

        ban.EndsAtUtc = endsAtUtc;
        ban.Reason = Text.Clip(reason, AppConsts.BanReasonMaxLength);
        Updated(ban, actor);
        await dbContext.SaveChangesAsync(cancellationToken);
        await SyncAsync(ban.CommunityId, actor, cancellationToken);

        return BanSaveResult.Saved;
    }

    public async Task<bool> LiftAsync(int banId, string? reason, BanActor actor, CancellationToken cancellationToken = default)
    {
        var ban = await Active().FirstOrDefaultAsync(item => item.Id == banId, cancellationToken);

        if (ban == null)
            return false;

        Lift(ban, Text.Clip(reason, AppConsts.BanLiftReasonMaxLength), actor);
        await dbContext.SaveChangesAsync(cancellationToken);
        await SyncAsync(ban.CommunityId, actor, cancellationToken);

        return true;
    }

    public async Task<Ban?> CaptureAsync(GameBanCommand? command, string? serverName, CancellationToken cancellationToken = default)
    {
        var communityId = SteamIdParser.Parse(command?.Player);

        if (command == null || communityId == null || command.Minutes < 0)
            return null;

        var now = clock.UtcNow;
        var actor = new BanActor(SteamIdParser.Parse(command.AdminCommunityId), command.AdminName);
        var endsAt = command.Minutes == 0 ? (DateTime?)null : now.AddMinutes(command.Minutes);
        var ban = await Active().FirstOrDefaultAsync(item => item.CommunityId == communityId.Value, cancellationToken);
        var created = ban == null;

        if (ban == null)
        {
            ban = new Ban
            {
                CommunityId = communityId.Value,
                Source = BanSource.Game,
                GameServerName = Text.Clip(serverName, AppConsts.GameServerNameMaxLength),
                CreatedByCommunityId = actor.CommunityId,
                CreatedByName = Text.Clip(actor.Name, AppConsts.LivePlayerNameMaxLength),
                CreatedAtUtc = now
            };

            dbContext.Set<Ban>().Add(ban);
            Banned(ban);
        }
        else
            Updated(ban, actor);

        ban.Name = Text.Clip(command.Name, AppConsts.LivePlayerNameMaxLength) ?? ban.Name ?? Text.Clip(await playerNameService.RealNameAsync(communityId.Value, cancellationToken), AppConsts.LivePlayerNameMaxLength);
        ban.Reason = Text.Clip(command.Reason, AppConsts.BanReasonMaxLength);
        ban.Command = Text.Clip(command.Command, AppConsts.BanCommandMaxLength);
        ban.EndsAtUtc = endsAt;

        await dbContext.SaveChangesAsync(cancellationToken);
        await alertService.ResolveAsync(ban.CommunityId, actor.CommunityId, cancellationToken);

        if (created)
            await alertService.LinkedAsync(ban, cancellationToken);

        return ban;
    }

    public async Task<bool> CaptureLiftAsync(GameUnbanCommand? command, CancellationToken cancellationToken = default)
    {
        var communityId = SteamIdParser.Parse(command?.Player);

        if (command == null || communityId == null)
            return false;

        var bans = await Active().Where(ban => ban.CommunityId == communityId.Value).ToListAsync(cancellationToken);
        var actor = new BanActor(SteamIdParser.Parse(command.AdminCommunityId), command.AdminName);

        foreach (var ban in bans)
            Lift(ban, null, actor);

        if (bans.Count > 0)
            await dbContext.SaveChangesAsync(cancellationToken);

        return bans.Count > 0;
    }

    public async Task<Ban?> AttemptAsync(long communityId, CancellationToken cancellationToken = default)
    {
        var ban = await Active().Where(item => item.CommunityId == communityId).OrderBy(item => item.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);

        if (ban == null)
            return null;

        ban.Attempts++;
        ban.LastAttemptAtUtc = clock.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ban;
    }

    public async Task<BanImport> ImportAsync(string? text, BanActor actor, CancellationToken cancellationToken = default)
    {
        var file = BanFileParser.Parse(text);
        var now = clock.UtcNow;
        var ids = file.Bans.Select(item => item.CommunityId).ToList();
        var banned = await BannedAsync(ids, cancellationToken);
        var added = new List<Ban>();
        var unchanged = 0;

        foreach (var imported in file.Bans)
        {
            var bannedAt = imported.BannedOn.HasValue ? DateTime.SpecifyKind(imported.BannedOn.Value, DateTimeKind.Utc) : now;
            var endsAt = imported.Minutes > 0 ? bannedAt.AddMinutes(imported.Minutes) : imported.ExpiresOn.HasValue ? DateTime.SpecifyKind(imported.ExpiresOn.Value, DateTimeKind.Utc) : (DateTime?)null;

            if (banned.Contains(imported.CommunityId) || endsAt <= now || !SteamIds.IsValid(imported.CommunityId))
            {
                unchanged++;
                continue;
            }

            var ban = new Ban
            {
                CommunityId = imported.CommunityId,
                Name = imported.Name ?? Text.Clip(await playerNameService.RealNameAsync(imported.CommunityId, cancellationToken), AppConsts.LivePlayerNameMaxLength),
                Reason = imported.Reason,
                Source = BanSource.Import,
                CreatedByCommunityId = actor.CommunityId,
                CreatedByName = Text.Clip(actor.Name, AppConsts.LivePlayerNameMaxLength),
                CreatedAtUtc = bannedAt > now ? now : bannedAt,
                EndsAtUtc = endsAt
            };

            dbContext.Set<Ban>().Add(ban);
            added.Add(ban);
        }

        if (added.Count > 0)
            await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var ban in added)
        {
            await alertService.ResolveAsync(ban.CommunityId, actor.CommunityId, cancellationToken);
            await alertService.LinkedAsync(ban, cancellationToken);
        }

        return new BanImport(added.Count, unchanged, file.Invalid);
    }

    private IQueryable<Ban> Active()
    {
        return dbContext.Set<Ban>().Active(clock.UtcNow);
    }

    private IQueryable<Ban> Filtered(BanListFilter filter)
    {
        var bans = dbContext.Set<Ban>().AsNoTracking();
        var now = clock.UtcNow;

        return filter == BanListFilter.Active ? bans.Active(now) : bans.Ended(now);
    }

    private void Banned(Ban ban)
    {
        notifications.Send(ban.CommunityId, new NotificationMessage(NotificationTopic.Bans, "You were banned from the game servers. The bans page shows the reason and when it ends.", AppConsts.BansPath));
    }

    private void Updated(Ban ban, BanActor actor)
    {
        ban.UpdatedByCommunityId = actor.CommunityId;
        ban.UpdatedByName = Text.Clip(actor.Name, AppConsts.LivePlayerNameMaxLength);
        ban.UpdatedAtUtc = clock.UtcNow;
    }

    private void Lift(Ban ban, string? reason, BanActor actor)
    {
        ban.LiftedByCommunityId = actor.CommunityId;
        ban.LiftedByName = Text.Clip(actor.Name, AppConsts.LivePlayerNameMaxLength);
        ban.LiftedAtUtc = clock.UtcNow;
        ban.LiftReason = reason;
        notifications.Send(ban.CommunityId, new NotificationMessage(NotificationTopic.Bans, "Your ban from the game servers was lifted. You can play again.", null));
    }

    private async Task SyncAsync(long communityId, BanActor actor, CancellationToken cancellationToken)
    {
        if (actor.CommunityId is { } administrator)
            await remoteCommandService.QueueInternalAsync(administrator, actor.Name, $"{AppConsts.BanSyncCommand} {communityId}", cancellationToken);
    }
}
