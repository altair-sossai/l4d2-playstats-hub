using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Services;

public class CommunityMixService(HubDbContext dbContext, ISettingsService settingsService, ICommunityRandomService random) : ICommunityMixService
{
    public async Task<CommunityMixResult> BalanceAsync(IReadOnlyList<long> communityIds, CancellationToken cancellationToken = default)
    {
        if (!IsValid(communityIds))
            return new CommunityMixResult(CommunityMixStatus.Invalid, null, []);

        var settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken);
        var minimum = Math.Max(1, settings.MinimumComparisonsToBeListed);

        var ratings = await dbContext.Set<CommunityRating>()
            .AsNoTracking()
            .Where(rating => communityIds.Contains(rating.CommunityId) && rating.Comparisons >= minimum)
            .ToDictionaryAsync(rating => rating.CommunityId, cancellationToken);

        List<long> unranked = [.. communityIds.Where(communityId => !ratings.ContainsKey(communityId))];

        if (unranked.Count > 0)
            return new CommunityMixResult(CommunityMixStatus.Unranked, null, unranked);

        List<CommunityMixPlayer> players = [.. communityIds.Select(communityId => new CommunityMixPlayer(communityId, ratings[communityId].Rating, ratings[communityId].Deviation))];

        return new CommunityMixResult(CommunityMixStatus.Balanced, CommunityMixBalancer.Split(players, random), []);
    }

    private static bool IsValid(IReadOnlyList<long> communityIds)
    {
        return communityIds.Count >= AppConsts.CommunityMixMinPlayers
               && communityIds.Count <= AppConsts.CommunityMixMaxPlayers
               && communityIds.Count % 2 == 0
               && communityIds.Distinct().Count() == communityIds.Count
               && communityIds.All(SteamIds.IsValid);
    }
}
