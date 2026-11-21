using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Models.Players;
using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Services;

public class PlayerCommunityService(
    ISettingsService settingsService,
    ICommunityRankingService communityRankingService,
    ISiteClock siteClock,
    ICurrentUser currentUser) : IPlayerCommunityService
{
    public async Task<PlayerCommunityModel?> BuildAsync(long communityId, long? compareWith, CancellationToken cancellationToken = default)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        if (!modules.IsEnabled(HubModule.CommunityRanking))
            return null;

        var settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken);
        var moderate = await currentUser.IsAdministratorAsync();
        var showVoters = settings.ShowVoterNames || moderate;
        var entries = await communityRankingService.ListAsync(cancellationToken);
        var size = AppConsts.PlayerCommunityVotes;
        var received = compareWith == null ? await communityRankingService.VotesPageAsync(null, communityId, 1, size, cancellationToken) : Page<CommunityVoteFeedItem>.Empty(size);
        var cast = compareWith == null && showVoters ? await communityRankingService.VotesPageAsync(communityId, null, 1, size, cancellationToken) : Page<CommunityVoteFeedItem>.Empty(size);

        return new PlayerCommunityModel
        {
            First = await StandingAsync(communityId, entries, cancellationToken),
            Second = compareWith == null ? null : await StandingAsync(compareWith.Value, entries, cancellationToken),
            Received = received.Items,
            Cast = cast.Items,
            ReceivedTotal = received.Total,
            CastTotal = cast.Total,
            ShowVoters = showVoters,
            Moderate = moderate,
            TimeZone = await siteClock.ZoneAsync(cancellationToken)
        };
    }

    private async Task<PlayerCommunityStanding> StandingAsync(long communityId, IReadOnlyList<CommunityRankingEntry> entries, CancellationToken cancellationToken)
    {
        var entry = entries.FirstOrDefault(item => item.CommunityId == communityId);
        var rating = await communityRankingService.RatingAsync(communityId, cancellationToken);

        return new PlayerCommunityStanding(communityId, entry, rating);
    }
}
