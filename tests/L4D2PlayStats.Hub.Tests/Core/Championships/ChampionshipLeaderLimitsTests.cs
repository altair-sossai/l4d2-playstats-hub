using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using static L4D2PlayStats.Hub.Tests.Core.Championships.ChampionshipFixture;

namespace L4D2PlayStats.Hub.Tests.Core.Championships;

[TestClass]
public class ChampionshipLeaderLimitsTests
{
    private const long CaptainA = FirstPlayer;
    private const long CaptainB = FirstPlayer + 100;
    private static readonly DateTime Friday = new(2026, 11, 20, 22, 0, 0, DateTimeKind.Utc);

    [TestMethod]
    public async Task ALeaderCannotDeleteAProposalOfASeriesThatIsOver()
    {
        using var hub = new TestHub();

        var (championshipId, seriesId) = await SeriesAsync(hub);
        var proposal = await hub.RunAsync(services => Schedule(services).ProposeAsync(seriesId, Friday, CaptainB));

        await PlaySeriesAsync(hub, championshipId, seriesId, true);

        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => hub.RunAsync(services => Schedule(services).DeleteAsync(proposal.Id, CaptainB, false)));
        await hub.RunAsync(services => Schedule(services).DeleteAsync(proposal.Id, Admin, true));

        Assert.IsEmpty(Series(await LoadAsync(hub, championshipId), seriesId).Proposals);
    }

    [TestMethod]
    public async Task ATeamHasAtMostFiveDatesWaitingForAnAnswer()
    {
        using var hub = new TestHub();

        var (_, seriesId) = await SeriesAsync(hub);
        var proposals = new List<int>();

        for (var day = 0; day < 5; day++)
            proposals.Add((await hub.RunAsync(services => Schedule(services).ProposeAsync(seriesId, Friday.AddDays(day), CaptainA))).Id);

        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => hub.RunAsync(services => Schedule(services).ProposeAsync(seriesId, Friday.AddDays(6), CaptainA)));

        await hub.RunAsync(services => Schedule(services).ProposeAsync(seriesId, Friday.AddDays(6), CaptainB));
        await hub.RunAsync(services => Schedule(services).AnswerAsync(proposals[0], false, CaptainB));
        await hub.RunAsync(services => Schedule(services).ProposeAsync(seriesId, Friday.AddDays(7), CaptainA));
    }

    [TestMethod]
    public async Task ADateThatHasPassedCannotBeAccepted()
    {
        using var hub = new TestHub();

        var (championshipId, seriesId) = await SeriesAsync(hub);
        var proposal = await hub.RunAsync(services => Schedule(services).ProposeAsync(seriesId, Friday, CaptainA));

        hub.Clock.UtcNow = Friday.AddHours(1);

        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => hub.RunAsync(services => Schedule(services).AnswerAsync(proposal.Id, true, CaptainB)));
        await hub.RunAsync(services => Schedule(services).AnswerAsync(proposal.Id, false, CaptainB));

        Assert.IsNull(Series(await LoadAsync(hub, championshipId), seriesId).ScheduledAtUtc);
    }

    [TestMethod]
    public async Task SendingTheIdentityAgainWhileItWaitsDoesNotTellTheAdministratorsAgain()
    {
        using var hub = new TestHub();

        var (championshipId, _) = await SeriesAsync(hub);
        var teamId = (await LoadAsync(hub, championshipId)).Teams.First(team => team.CaptainCommunityId == CaptainA).Id;

        for (var attempt = 0; attempt < 4; attempt++)
            await hub.RunAsync(services => services.GetRequiredService<IChampionshipIdentityService>().ProposeAsync(teamId, CaptainA, $"Wolves {attempt}", "WLF", null));

        var told = await hub.RunAsync(services => services.GetRequiredService<INotificationService>().LatestAsync(Admin, true, 50));

        Assert.HasCount(1, told.Where(item => item.Text == "The team {0} sent a new name, tag or flag for approval in {1}."));
    }

    private static IChampionshipScheduleService Schedule(IServiceProvider services)
    {
        return services.GetRequiredService<IChampionshipScheduleService>();
    }

    private static async Task<(int ChampionshipId, int SeriesId)> SeriesAsync(TestHub hub)
    {
        var championship = await CreateAsync(hub, "Cup", Knockout());

        await TeamsAsync(hub, championship.Id, 2);

        var stageId = (await LoadAsync(hub, championship.Id)).Stages.Single().Id;

        await GenerateAsync(hub, stageId);

        var series = (await LoadAsync(hub, championship.Id)).Stages.Single().Series.Single();

        return (championship.Id, series.Id);
    }
}
