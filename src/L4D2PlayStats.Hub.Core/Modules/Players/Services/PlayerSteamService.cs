using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services;

public class PlayerSteamService(HubDbContext dbContext, HubState hubState, IClock clock, IPlayerRegistryService playerRegistryService) : IPlayerSteamService
{
    public Task<List<long>> DueAsync(int count, CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        var stale = now.AddHours(-AppConsts.SteamDataMaxAgeInHours);
        var inactiveStale = now.AddDays(-AppConsts.SteamDataInactiveMaxAgeInDays);
        var active = now.AddDays(-AppConsts.SteamDataActivePlayerInDays);

        return dbContext.PlayerProfiles
            .AsNoTracking()
            .Where(p => p.CommunityId > AppConsts.SteamCommunityIdBase)
            .Where(p => p.SteamUpdatedAtUtc == null || p.SteamUpdatedAtUtc < inactiveStale || (p.LastSeenAtUtc >= active && p.SteamUpdatedAtUtc < stale))
            .OrderBy(p => p.SteamUpdatedAtUtc == null ? 0 : 1)
            .ThenByDescending(p => p.LastSeenAtUtc)
            .ThenBy(p => p.CommunityId)
            .Select(p => p.CommunityId)
            .Take(Math.Max(0, count))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ApplyAsync(IReadOnlyCollection<PlayerSteamUpdate> updates, CancellationToken cancellationToken = default)
    {
        var answered = updates.Where(update => update.Answered).ToDictionary(update => update.CommunityId);

        if (answered.Count == 0)
            return false;

        var renamed = new List<PlayerProfile>();

        await using (await hubState.LockAsync(cancellationToken))
        {
            List<long> ids = [.. answered.Keys];
            var profiles = await dbContext.PlayerProfiles.Where(profile => ids.Contains(profile.CommunityId)).ToListAsync(cancellationToken);
            var now = clock.UtcNow;

            foreach (var profile in profiles)
            {
                Apply(profile, answered[profile.CommunityId]);
                profile.SteamUpdatedAtUtc = now;

                if (PlayerDisplayNames.Refresh(profile))
                    renamed.Add(profile);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (renamed.Count == 0)
            return false;

        playerRegistryService.Publish(renamed);
        hubState.DataChanged();

        return true;
    }

    public async Task<Dictionary<long, PlayerSteamDetails>> DetailsAsync(IReadOnlyCollection<long> communityIds, CancellationToken cancellationToken = default)
    {
        List<long> ids = [.. communityIds.Distinct()];

        return await dbContext.PlayerProfiles
            .AsNoTracking()
            .Where(p => ids.Contains(p.CommunityId))
            .Select(p => new PlayerSteamDetails(p.CommunityId, p.SteamName, p.AccountCreatedAtUtc, p.VacBans, p.GameBans, p.DaysSinceLastBan, p.Left4Dead2Minutes, p.Left4Dead2RecentMinutes, p.SteamLevel, p.SteamUpdatedAtUtc))
            .ToDictionaryAsync(details => details.CommunityId, cancellationToken);
    }

    private static void Apply(PlayerProfile profile, PlayerSteamUpdate update)
    {
        if (update.HasSummary)
        {
            profile.SteamName = Text.Clip(update.SteamName, AppConsts.PlayerNameColumnMaxLength);
            profile.AccountCreatedAtUtc = update.AccountCreatedAtUtc;
        }

        if (update.HasBans)
        {
            profile.VacBans = update.VacBans;
            profile.GameBans = update.GameBans;
            profile.DaysSinceLastBan = update.DaysSinceLastBan;
        }

        if (update.HasPlaytime)
        {
            profile.Left4Dead2Minutes = update.Left4Dead2Minutes;
            profile.Left4Dead2RecentMinutes = update.Left4Dead2RecentMinutes;
        }

        if (update.HasLevel)
            profile.SteamLevel = update.SteamLevel;
    }
}
