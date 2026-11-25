using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services;

public class ChampionshipScheduleService(HubDbContext dbContext, IClock clock, IChampionshipLoadService loader, IChampionshipLogService log) : IChampionshipScheduleService
{
    public async Task<ChampionshipDateProposal> ProposeAsync(int seriesId, DateTime dateUtc, long communityId, CancellationToken cancellationToken = default)
    {
        var championship = await loader.LoadBySeriesAsync(seriesId, cancellationToken) ?? throw new ChampionshipException("The series was not found.");
        var stage = championship.Stages.First(item => item.Series.Any(series => series.Id == seriesId));
        var series = stage.Series.First(item => item.Id == seriesId);
        var team = ChampionshipSeriesService.Leader(championship, series, communityId);

        ChampionshipSeriesService.Open(championship, series);

        if (dateUtc <= clock.UtcNow)
            throw new ChampionshipException("Choose a date in the future.");

        if (series.Proposals.Any(proposal => proposal.DateUtc == dateUtc && !proposal.Refused))
            throw new ChampionshipException("This date was already proposed.");

        var teamA = series.TeamAId == team.Id;
        var proposal = new ChampionshipDateProposal
        {
            DateUtc = dateUtc,
            TeamId = team.Id,
            ProposedByCommunityId = communityId,
            TeamAAnswer = teamA ? ChampionshipAnswer.Accepted : ChampionshipAnswer.None,
            TeamBAnswer = teamA ? ChampionshipAnswer.None : ChampionshipAnswer.Accepted,
            CreatedAtUtc = clock.UtcNow
        };

        series.Proposals.Add(proposal);
        log.Add(championship.Id, communityId, "{0} proposed a date for {1}", team.Name, ChampionshipSeriesService.Title(championship, stage, series));
        await dbContext.SaveChangesAsync(cancellationToken);

        return proposal;
    }

    public async Task AnswerAsync(int proposalId, bool accept, long communityId, CancellationToken cancellationToken = default)
    {
        var seriesId = await SeriesOf(proposalId, cancellationToken);
        var championship = await loader.LoadBySeriesAsync(seriesId, cancellationToken) ?? throw new ChampionshipException("The series was not found.");
        var stage = championship.Stages.First(item => item.Series.Any(series => series.Id == seriesId));
        var series = stage.Series.First(item => item.Id == seriesId);
        var proposal = series.Proposals.First(item => item.Id == proposalId);
        var team = ChampionshipSeriesService.Leader(championship, series, communityId);

        ChampionshipSeriesService.Open(championship, series);

        if (proposal.TeamId == team.Id)
            throw new ChampionshipException("The other team must answer this proposal.");

        if (proposal.Confirmed)
            throw new ChampionshipException("This date is already confirmed.");

        var answer = accept ? ChampionshipAnswer.Accepted : ChampionshipAnswer.Refused;

        if (series.TeamAId == team.Id)
            proposal.TeamAAnswer = answer;
        else
            proposal.TeamBAnswer = answer;

        if (proposal.Confirmed)
            series.ScheduledAtUtc = proposal.DateUtc;

        log.Add(championship.Id, communityId, accept ? "{0} accepted a date for {1}" : "{0} refused a date for {1}", team.Name, ChampionshipSeriesService.Title(championship, stage, series));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int proposalId, long communityId, bool administrator, CancellationToken cancellationToken = default)
    {
        var seriesId = await SeriesOf(proposalId, cancellationToken);
        var championship = await loader.LoadBySeriesAsync(seriesId, cancellationToken) ?? throw new ChampionshipException("The series was not found.");
        var series = championship.Stages.SelectMany(stage => stage.Series).First(item => item.Id == seriesId);
        var proposal = series.Proposals.First(item => item.Id == proposalId);

        if (!administrator)
        {
            var team = ChampionshipSeriesService.Leader(championship, series, communityId);

            if (proposal.TeamId != team.Id)
                throw new ChampionshipException("Only the team that proposed this date can delete it.");

            if (proposal.Confirmed)
                throw new ChampionshipException("This date is already confirmed.");
        }

        series.Proposals.Remove(proposal);
        log.Add(championship.Id, communityId, "Deleted a date proposal");
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> SeriesOf(int proposalId, CancellationToken cancellationToken)
    {
        var seriesId = await dbContext.ChampionshipDateProposals.AsNoTracking().Where(proposal => proposal.Id == proposalId).Select(proposal => (int?)proposal.SeriesId).FirstOrDefaultAsync(cancellationToken);

        return seriesId ?? throw new ChampionshipException("The proposal was not found.");
    }
}
