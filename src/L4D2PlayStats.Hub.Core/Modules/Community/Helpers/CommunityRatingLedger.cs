using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Extensions;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Helpers;

public class CommunityRatingLedger(CommunityRankingSettings settings, IEnumerable<CommunityRating> ratings)
{
    private readonly Dictionary<long, CommunityRating> _ratings = ratings.ToDictionary(rating => rating.CommunityId);

    public List<CommunityRating> Created { get; } = [];

    public void Apply(long leftPlayerId, long rightPlayerId, CommunityVoteOutcome outcome)
    {
        if (!outcome.IsJudgement() || leftPlayerId == rightPlayerId)
            return;

        var left = GetOrCreate(leftPlayerId);
        var right = GetOrCreate(rightPlayerId);
        var change = CommunityEloCalculator.Calculate(left.Rating, right.Rating, outcome, settings.KFactor);

        left.Rating = change.Left;
        right.Rating = change.Right;
        left.Comparisons++;
        right.Comparisons++;

        switch (outcome)
        {
            case CommunityVoteOutcome.Left:
                left.Wins++;
                right.Losses++;
                break;
            case CommunityVoteOutcome.Right:
                left.Losses++;
                right.Wins++;
                break;
            default:
                left.Draws++;
                right.Draws++;
                break;
        }
    }

    private CommunityRating GetOrCreate(long communityId)
    {
        if (_ratings.TryGetValue(communityId, out var rating))
            return rating;

        rating = new CommunityRating { CommunityId = communityId, Rating = settings.InitialRating };

        _ratings.Add(communityId, rating);
        Created.Add(rating);

        return rating;
    }
}
