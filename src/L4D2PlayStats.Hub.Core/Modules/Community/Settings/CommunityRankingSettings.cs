namespace L4D2PlayStats.Hub.Core.Modules.Community.Settings;

public class CommunityRankingSettings
{
    public int MinimumMatchesToVote { get; set; } = AppConsts.DefaultCommunityMinimumMatchesToVote;
    public int VotesPerDay { get; set; } = AppConsts.DefaultCommunityVotesPerDay;
    public int MinimumMatchesToBeCandidate { get; set; } = AppConsts.DefaultCommunityMinimumMatchesToBeCandidate;
    public int CandidatePlayedWithinDays { get; set; } = AppConsts.DefaultCommunityCandidatePlayedWithinDays;
    public int MinimumComparisonsToBeListed { get; set; } = AppConsts.DefaultCommunityMinimumComparisonsToBeListed;
    public int MaximumPlayersListed { get; set; } = AppConsts.DefaultCommunityMaximumPlayersListed;
    public int KFactor { get; set; } = AppConsts.DefaultCommunityKFactor;
    public int InitialRating { get; set; } = AppConsts.DefaultCommunityInitialRating;

    public void Normalize()
    {
        MinimumMatchesToVote = Math.Max(0, MinimumMatchesToVote);
        VotesPerDay = Math.Max(AppConsts.CommunityMinVotesPerDay, VotesPerDay);
        MinimumMatchesToBeCandidate = Math.Max(0, MinimumMatchesToBeCandidate);
        CandidatePlayedWithinDays = Math.Max(0, CandidatePlayedWithinDays);
        MinimumComparisonsToBeListed = Math.Max(0, MinimumComparisonsToBeListed);
        MaximumPlayersListed = Math.Max(0, MaximumPlayersListed);
        KFactor = Math.Clamp(KFactor, AppConsts.CommunityMinKFactor, AppConsts.CommunityMaxKFactor);
        InitialRating = Math.Max(0, InitialRating);
    }

    public DateTime? CandidatesPlayedSince(DateTime utcNow)
    {
        return CandidatePlayedWithinDays > 0 ? utcNow.AddDays(-CandidatePlayedWithinDays) : null;
    }
}
