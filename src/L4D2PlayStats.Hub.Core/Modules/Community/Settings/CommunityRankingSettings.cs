using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Settings;

public class CommunityRankingSettings : INormalizable
{
    public int MinimumMatchesToVote { get; set; } = AppConsts.DefaultCommunityMinimumMatchesToVote;
    public int VotesPerDay { get; set; } = AppConsts.DefaultCommunityVotesPerDay;
    public int MinimumMatchesToBeCandidate { get; set; } = AppConsts.DefaultCommunityMinimumMatchesToBeCandidate;
    public int CandidatePlayedWithinDays { get; set; } = AppConsts.DefaultCommunityCandidatePlayedWithinDays;
    public int MinimumComparisonsToBeListed { get; set; } = AppConsts.DefaultCommunityMinimumComparisonsToBeListed;
    public int MaximumPlayersListed { get; set; } = AppConsts.DefaultCommunityMaximumPlayersListed;
    public bool ShowVoterNames { get; set; } = true;
    public int OtherNamesShown { get; set; } = AppConsts.CommunityBallotNames;
    public int InitialRating { get; set; } = AppConsts.DefaultCommunityInitialRating;

    public void Normalize()
    {
        MinimumMatchesToVote = Math.Max(0, MinimumMatchesToVote);
        VotesPerDay = Math.Max(AppConsts.CommunityMinVotesPerDay, VotesPerDay);
        MinimumMatchesToBeCandidate = Math.Max(0, MinimumMatchesToBeCandidate);
        CandidatePlayedWithinDays = Math.Max(0, CandidatePlayedWithinDays);
        MinimumComparisonsToBeListed = Math.Max(0, MinimumComparisonsToBeListed);
        MaximumPlayersListed = Math.Max(0, MaximumPlayersListed);
        OtherNamesShown = Math.Clamp(OtherNamesShown, 0, AppConsts.CommunityMaxBallotNames);
        InitialRating = Math.Max(0, InitialRating);
    }

    public DateTime? CandidatesPlayedSince(DateTime utcNow)
    {
        return CandidatePlayedWithinDays > 0 ? utcNow.AddDays(-CandidatePlayedWithinDays) : null;
    }
}
