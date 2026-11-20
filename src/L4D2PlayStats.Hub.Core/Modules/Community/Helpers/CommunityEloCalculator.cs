using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Extensions;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Helpers;

public static class CommunityEloCalculator
{
    public static double Expected(double rating, double opponent)
    {
        return 1 / (1 + Math.Pow(AppConsts.CommunityEloBase, (opponent - rating) / AppConsts.CommunityEloScale));
    }

    public static double Score(CommunityVoteOutcome outcome)
    {
        return outcome switch
        {
            CommunityVoteOutcome.Left => AppConsts.CommunityWinScore,
            CommunityVoteOutcome.Right => AppConsts.CommunityLossScore,
            _ => AppConsts.CommunityDrawScore
        };
    }

    public static CommunityEloChange Calculate(double left, double right, CommunityVoteOutcome outcome, int kFactor)
    {
        if (!outcome.IsJudgement())
            return new CommunityEloChange(left, right);

        var change = kFactor * (Score(outcome) - Expected(left, right));

        return new CommunityEloChange(left + change, right - change);
    }
}
