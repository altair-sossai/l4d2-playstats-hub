using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Models.Players;
using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Services;

public class PlayerCommunityService(
    ISettingsService settingsService,
    ICommunityRankingService communityRankingService,
    ILiveViewService liveViewService) : IPlayerCommunityService
{
    public async Task<PlayerCommunityModel?> BuildAsync(long communityId, long? compareWith, CancellationToken cancellationToken = default)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        if (!modules.IsEnabled(HubModule.CommunityRanking))
            return null;

        var settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken);
        var entries = await communityRankingService.ListAsync(cancellationToken);
        var size = AppConsts.PlayerCommunityVotes;
        var received = compareWith == null ? await communityRankingService.LatestVotesAsync(null, communityId, 0, size + 1, cancellationToken) : [];
        var cast = compareWith == null && settings.ShowVoterNames ? await communityRankingService.LatestVotesAsync(communityId, null, 0, size + 1, cancellationToken) : [];

        return new PlayerCommunityModel
        {
            First = await StandingAsync(communityId, entries, cancellationToken),
            Second = compareWith == null ? null : await StandingAsync(compareWith.Value, entries, cancellationToken),
            Received = [.. received.Take(size)],
            Cast = [.. cast.Take(size)],
            HasMoreReceived = received.Count > size,
            HasMoreCast = cast.Count > size,
            ShowVoters = settings.ShowVoterNames,
            TimeZone = await liveViewService.TimeZoneAsync(cancellationToken)
        };
    }

    private async Task<PlayerCommunityStanding> StandingAsync(long communityId, IReadOnlyList<CommunityRankingEntry> entries, CancellationToken cancellationToken)
    {
        var entry = entries.FirstOrDefault(item => item.CommunityId == communityId);
        var rating = await communityRankingService.RatingAsync(communityId, cancellationToken);

        return new PlayerCommunityStanding(communityId, entry, rating);
    }
}
