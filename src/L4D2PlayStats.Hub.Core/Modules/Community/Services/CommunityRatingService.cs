using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Community.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Services;

public class CommunityRatingService(HubDbContext dbContext, ISettingsService settingsService) : ICommunityRatingService
{
    public async Task<int> RecalculateAsync(long? withoutVoterId = null, CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken);
        var ratings = await dbContext.Set<CommunityRating>().ToListAsync(cancellationToken);

        foreach (var rating in ratings)
        {
            rating.Comparisons = 0;
            rating.Wins = 0;
            rating.Losses = 0;
            rating.Draws = 0;
        }

        var votes = await dbContext.Set<CommunityVote>()
            .AsNoTracking()
            .Where(v => withoutVoterId == null || v.VoterId != withoutVoterId)
            .OrderBy(v => v.Id)
            .Select(v => new { v.LeftPlayerId, v.RightPlayerId, v.Outcome })
            .ToListAsync(cancellationToken);

        var ledger = new CommunityRatingLedger(settings, ratings);

        foreach (var vote in votes)
            ledger.Apply(vote.LeftPlayerId, vote.RightPlayerId, vote.Outcome);

        ledger.Complete();

        dbContext.AddRange(ledger.Created);
        dbContext.RemoveRange(ratings.Where(rating => rating.Comparisons == 0));

        return votes.Count;
    }

    public Task<bool> NeedsRecalculationAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<CommunityRating>().AnyAsync(rating => rating.Deviation <= 0, cancellationToken);
    }
}
