using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Extensions;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Helpers;

public class CommunityRatingLedger(CommunityRankingSettings settings, IEnumerable<CommunityRating> ratings)
{
    private readonly List<CommunityComparison> _comparisons = [];
    private readonly Dictionary<long, CommunityRating> _ratings = ratings.ToDictionary(rating => rating.CommunityId);

    public List<CommunityRating> Created { get; } = [];

    public void Apply(long leftPlayerId, long rightPlayerId, CommunityVoteOutcome outcome)
    {
        if (!outcome.IsJudgement() || leftPlayerId == rightPlayerId)
            return;

        var left = GetOrCreate(leftPlayerId);
        var right = GetOrCreate(rightPlayerId);

        _comparisons.Add(new CommunityComparison(leftPlayerId, rightPlayerId, CommunityRatingFit.Score(outcome)));

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

    public void Complete()
    {
        var strengths = CommunityRatingFit.Fit(_comparisons, settings.InitialRating);

        foreach (var rating in _ratings.Values)
        {
            var strength = strengths.GetValueOrDefault(rating.CommunityId);

            rating.Rating = strength?.Rating ?? settings.InitialRating;
            rating.Deviation = strength?.Deviation ?? AppConsts.CommunityPriorDeviation;
        }
    }

    private CommunityRating GetOrCreate(long communityId)
    {
        if (_ratings.TryGetValue(communityId, out var rating))
            return rating;

        rating = new CommunityRating { CommunityId = communityId, Rating = settings.InitialRating, Deviation = AppConsts.CommunityPriorDeviation };

        _ratings.Add(communityId, rating);
        Created.Add(rating);

        return rating;
    }
}
