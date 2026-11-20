using L4D2PlayStats.Hub.Core.Modules.Community.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Extensions;

public static class CommunityVoteOutcomeExtensions
{
    public static bool IsJudgement(this CommunityVoteOutcome outcome)
    {
        return outcome is CommunityVoteOutcome.Left or CommunityVoteOutcome.Right or CommunityVoteOutcome.SameLevel;
    }

    public static bool HidesLeft(this CommunityVoteOutcome outcome)
    {
        return outcome is CommunityVoteOutcome.UnknownLeft or CommunityVoteOutcome.UnknownBoth;
    }

    public static bool HidesRight(this CommunityVoteOutcome outcome)
    {
        return outcome is CommunityVoteOutcome.UnknownRight or CommunityVoteOutcome.UnknownBoth;
    }
}