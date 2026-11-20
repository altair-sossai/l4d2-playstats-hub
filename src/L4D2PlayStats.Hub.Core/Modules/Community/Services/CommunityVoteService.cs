using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Extensions;
using L4D2PlayStats.Hub.Core.Modules.Community.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Services;

public class CommunityVoteService(
    HubDbContext dbContext,
    HubState hubState,
    IClock clock,
    ISettingsService settingsService,
    ISeasonService seasonService,
    ICommunityPlayerService playerService,
    ICommunityRandomService random) : ICommunityVoteService
{
    public async Task<CommunityBallot> BallotAsync(long voterId, CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken);
        var matches = await playerService.MatchesAsync(voterId, cancellationToken);

        if (matches < settings.MinimumMatchesToVote)
            return Ballot(CommunityBallotStatus.TooFewMatches, settings, matches, 0);

        var votesLeft = await VotesLeftAsync(voterId, settings, cancellationToken);

        if (votesLeft == 0)
            return Ballot(CommunityBallotStatus.LimitReached, settings, matches, votesLeft);

        var pair = await OfferAsync(voterId, settings, cancellationToken);

        if (pair == null)
            return Ballot(CommunityBallotStatus.NobodyLeft, settings, matches, votesLeft);

        var names = await playerService.NamesAsync([pair.Left, pair.Right], cancellationToken);
        var used = await playerService.MostUsedNamesAsync([pair.Left, pair.Right], settings.OtherNamesShown + 1, cancellationToken);
        var left = BallotPlayer(pair.Left, names, used, settings.OtherNamesShown);
        var right = BallotPlayer(pair.Right, names, used, settings.OtherNamesShown);

        return Ballot(CommunityBallotStatus.Ready, settings, matches, votesLeft, left, right);
    }

    public async Task<CommunityVoteResult> VoteAsync(long voterId, long leftPlayerId, long rightPlayerId, CommunityVoteOutcome outcome, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(outcome))
            return CommunityVoteResult.InvalidOutcome;

        var settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken);
        var pair = new CommunityPair(leftPlayerId, rightPlayerId);

        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            if (await playerService.MatchesAsync(voterId, cancellationToken) < settings.MinimumMatchesToVote)
                return CommunityVoteResult.TooFewMatches;

            if (await VotesLeftAsync(voterId, settings, cancellationToken) == 0)
                return CommunityVoteResult.LimitReached;

            var offer = await dbContext.Set<CommunityOffer>().FirstOrDefaultAsync(o => o.VoterId == voterId, cancellationToken);

            if (offer == null || offer.LeftPlayerId != leftPlayerId || offer.RightPlayerId != rightPlayerId)
                return CommunityVoteResult.NotOffered;

            dbContext.Remove(offer);

            var selection = await SelectionAsync(voterId, settings, cancellationToken);

            if (!selection.Allows(pair))
            {
                await dbContext.SaveChangesAsync(cancellationToken);

                return CommunityVoteResult.NotOffered;
            }

            dbContext.Add(new CommunityVote
            {
                VoterId = voterId,
                LeftPlayerId = leftPlayerId,
                RightPlayerId = rightPlayerId,
                Outcome = outcome,
                CreatedAtUtc = clock.UtcNow,
                SiteDay = await TodayAsync(cancellationToken)
            });

            if (outcome.IsJudgement())
                await ApplyAsync(pair, outcome, settings, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            hubState.WriteLock.Release();
        }

        hubState.CommunityChanged();

        return CommunityVoteResult.Accepted;
    }

    private async Task ApplyAsync(CommunityPair pair, CommunityVoteOutcome outcome, CommunityRankingSettings settings, CancellationToken cancellationToken)
    {
        var ratings = await dbContext.Set<CommunityRating>()
            .Where(r => r.CommunityId == pair.Left || r.CommunityId == pair.Right)
            .ToListAsync(cancellationToken);

        var ledger = new CommunityRatingLedger(settings, ratings);

        ledger.Apply(pair.Left, pair.Right, outcome);

        dbContext.AddRange(ledger.Created);
    }

    private async Task<CommunityPair?> OfferAsync(long voterId, CommunityRankingSettings settings, CancellationToken cancellationToken)
    {
        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            var selection = await SelectionAsync(voterId, settings, cancellationToken);
            var offer = await dbContext.Set<CommunityOffer>().FirstOrDefaultAsync(o => o.VoterId == voterId, cancellationToken);
            var offered = offer == null ? null : new CommunityPair(offer.LeftPlayerId, offer.RightPlayerId);

            if (offered != null && selection.Allows(offered))
                return offered;

            var pair = CommunityPairSelector.Select(selection.Candidates, selection.Judged, random);

            if (offer != null)
                dbContext.Remove(offer);

            if (pair != null)
                dbContext.Add(new CommunityOffer { VoterId = voterId, LeftPlayerId = pair.Left, RightPlayerId = pair.Right, OfferedAtUtc = clock.UtcNow });

            await dbContext.SaveChangesAsync(cancellationToken);

            return pair;
        }
        finally
        {
            hubState.WriteLock.Release();
        }
    }

    private async Task<CommunitySelection> SelectionAsync(long voterId, CommunityRankingSettings settings, CancellationToken cancellationToken)
    {
        var eligible = await playerService.CandidatesAsync(settings.MinimumMatchesToBeCandidate, settings.CandidatesPlayedSince(clock.UtcNow), cancellationToken);
        var familiar = await playerService.FamiliarAsync(voterId, cancellationToken);
        var ratings = await dbContext.Set<CommunityRating>().AsNoTracking().ToDictionaryAsync(r => r.CommunityId, cancellationToken);

        var votes = await dbContext.Set<CommunityVote>()
            .AsNoTracking()
            .Where(v => v.VoterId == voterId)
            .Select(v => new { v.LeftPlayerId, v.RightPlayerId, v.Outcome })
            .ToListAsync(cancellationToken);

        var hidden = votes.Where(v => v.Outcome.HidesLeft()).Select(v => v.LeftPlayerId)
            .Concat(votes.Where(v => v.Outcome.HidesRight()).Select(v => v.RightPlayerId))
            .ToHashSet();

        var judged = votes.Select(v => new CommunityPair(v.LeftPlayerId, v.RightPlayerId).Normalized).ToHashSet();

        var candidates = eligible
            .Where(id => id != voterId && !hidden.Contains(id))
            .OrderBy(id => id)
            .Select(id => Candidate(id, ratings.GetValueOrDefault(id), settings, familiar))
            .ToList();

        return new CommunitySelection(candidates, judged);
    }

    private async Task<int> VotesLeftAsync(long voterId, CommunityRankingSettings settings, CancellationToken cancellationToken)
    {
        var today = await TodayAsync(cancellationToken);

        var used = await dbContext.Set<CommunityVote>().CountAsync(v =>
            v.VoterId == voterId
            && v.SiteDay == today
            && (v.Outcome == CommunityVoteOutcome.Left || v.Outcome == CommunityVoteOutcome.Right || v.Outcome == CommunityVoteOutcome.SameLevel), cancellationToken);

        return Math.Max(0, settings.VotesPerDay - used);
    }

    private async Task<DateTime> TodayAsync(CancellationToken cancellationToken)
    {
        var now = await seasonService.SiteNowAsync(cancellationToken);

        return now.Date;
    }

    private static CommunityBallotPlayer BallotPlayer(long communityId, Dictionary<long, string> names, Dictionary<long, List<string>> used, int count)
    {
        var name = names[communityId];

        var others = used[communityId]
            .Where(other => !string.Equals(other, name, StringComparison.CurrentCultureIgnoreCase))
            .Take(count)
            .ToList();

        return new CommunityBallotPlayer(communityId, name, others);
    }

    private static CommunityCandidate Candidate(long communityId, CommunityRating? rating, CommunityRankingSettings settings, HashSet<long> familiar)
    {
        return new CommunityCandidate(communityId, rating?.Rating ?? settings.InitialRating, rating?.Comparisons ?? 0, familiar.Contains(communityId));
    }

    private static CommunityBallot Ballot(CommunityBallotStatus status, CommunityRankingSettings settings, int matches, int votesLeft, CommunityBallotPlayer? left = null, CommunityBallotPlayer? right = null)
    {
        return new CommunityBallot
        {
            Status = status,
            Left = left,
            Right = right,
            Matches = matches,
            MinimumMatches = settings.MinimumMatchesToVote,
            VotesLeft = votesLeft,
            VotesPerDay = settings.VotesPerDay
        };
    }
}
