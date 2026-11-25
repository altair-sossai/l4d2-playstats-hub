using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using static L4D2PlayStats.Hub.Tests.Core.Championships.ChampionshipFixture;

namespace L4D2PlayStats.Hub.Tests.Core.Championships;

[TestClass]
public class ChampionshipTeamTests
{
    private static readonly DateTime Friday = new(2026, 11, 20, 22, 0, 0, DateTimeKind.Utc);

    [TestMethod]
    public async Task APlayerJoinsOneTeamOnlyAndTheRosterHasALimit()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "Cup", Knockout());
        var teams = await TeamsAsync(hub, championship.Id, 2, 2);

        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().AddMemberAsync(teams[1], FirstPlayer, Admin)));
        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().AddMemberAsync(teams[0], FirstPlayer + 50, Admin)));

        var loaded = await LoadAsync(hub, championship.Id);

        Assert.HasCount(4, loaded.Participants);
        Assert.AreEqual(FirstPlayer, loaded.Teams.First(team => team.Id == teams[0]).CaptainCommunityId);

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().RemoveParticipantAsync(championship.Id, FirstPlayer, Admin));

        var team = (await LoadAsync(hub, championship.Id)).Teams.First(item => item.Id == teams[0]);

        Assert.IsNull(team.CaptainCommunityId);
        Assert.HasCount(1, team.Members);
    }

    [TestMethod]
    public async Task TheMaximumNumberOfParticipantsIsKept()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "Cup", Knockout());

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipService>().UpdateAsync(championship.Id, new ChampionshipSettingsForm { Name = "Cup", MaxParticipants = 3 }, Admin));

        var added = await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().AddParticipantsAsync(championship.Id, [FirstPlayer, FirstPlayer + 1, FirstPlayer + 1], Admin));

        Assert.AreEqual(2, added);

        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().AddParticipantsAsync(championship.Id, [FirstPlayer + 2, FirstPlayer + 3], Admin)));
    }

    [TestMethod]
    public async Task TheSnakeDraftFollowsTheDrawnOrderAndCanBeUndone()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "Draft cup", Knockout());

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipService>().UpdateAsync(championship.Id, new ChampionshipSettingsForm { Name = "Draft cup", PlayersPerGame = 3, RosterSize = 3, DraftMethod = ChampionshipDraftMethod.Snake }, Admin));
        await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().CreateTeamsAsync(championship.Id, 3, "Team", Admin));

        var teams = (await LoadAsync(hub, championship.Id)).Teams.OrderBy(team => team.Seed).ToList();

        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => hub.RunAsync(services => services.GetRequiredService<IChampionshipDraftService>().DrawOrderAsync(championship.Id, Admin)));

        for (var index = 0; index < teams.Count; index++)
        {
            var teamId = teams[index].Id;
            var captain = FirstPlayer + index;

            await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().SetLeadersAsync(teamId, captain, null, Admin));
        }

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().AddParticipantsAsync(championship.Id, Enumerable.Range(10, 6).Select(index => FirstPlayer + index).ToList(), Admin));
        await hub.RunAsync(services => services.GetRequiredService<IChampionshipDraftService>().DrawOrderAsync(championship.Id, Admin));

        var order = (await LoadAsync(hub, championship.Id)).Teams.OrderBy(team => team.DraftPosition).Select(team => team.Id).ToList();
        var picked = new List<int>();

        for (var index = 0; index < 6; index++)
        {
            var player = FirstPlayer + 10 + index;
            var pick = await hub.RunAsync(services => services.GetRequiredService<IChampionshipDraftService>().PickAsync(championship.Id, player, Admin));

            picked.Add(pick.TeamId);
        }

        CollectionAssert.AreEqual(new[] { order[0], order[1], order[2], order[2], order[1], order[0] }, picked);

        var full = await LoadAsync(hub, championship.Id);

        Assert.IsNull(ChampionshipDraftService.OnTheClock(full));
        Assert.IsTrue(full.Teams.All(team => team.Members.Count == 3));

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipDraftService>().UndoAsync(championship.Id, Admin));

        var undone = await LoadAsync(hub, championship.Id);

        Assert.HasCount(5, undone.DraftPicks);
        Assert.AreEqual(order[0], ChampionshipDraftService.OnTheClock(undone)!.Id);
        Assert.HasCount(2, undone.Teams.First(team => team.Id == order[0]).Members);

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipDraftService>().ResetAsync(championship.Id, Admin));

        var reset = await LoadAsync(hub, championship.Id);

        Assert.IsEmpty(reset.DraftPicks);
        Assert.IsTrue(reset.Teams.All(team => team.Members.Count == 1 && team.DraftPosition == null));
    }

    [TestMethod]
    public async Task TheRouletteFillsTheTeamWithFewestPlayersAndThePotsGiveOnePlayerOfEachPotToEachTeam()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "Lucky cup", Knockout());

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipService>().UpdateAsync(championship.Id, new ChampionshipSettingsForm { Name = "Lucky cup", PlayersPerGame = 2, RosterSize = 2, DraftMethod = ChampionshipDraftMethod.Roulette }, Admin));
        await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().CreateTeamsAsync(championship.Id, 2, "Team", Admin));
        await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().AddParticipantsAsync(championship.Id, Enumerable.Range(0, 4).Select(index => FirstPlayer + index).ToList(), Admin));

        for (var index = 0; index < 4; index++)
            await hub.RunAsync(services => services.GetRequiredService<IChampionshipDraftService>().SpinAsync(championship.Id, Admin));

        var spun = await LoadAsync(hub, championship.Id);

        Assert.IsTrue(spun.Teams.All(team => team.Members.Count == 2));
        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => hub.RunAsync(services => services.GetRequiredService<IChampionshipDraftService>().SpinAsync(championship.Id, Admin)));

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipDraftService>().ResetAsync(championship.Id, Admin));
        await hub.RunAsync(services => services.GetRequiredService<IChampionshipService>().UpdateAsync(championship.Id, new ChampionshipSettingsForm { Name = "Lucky cup", PlayersPerGame = 2, RosterSize = 2, DraftMethod = ChampionshipDraftMethod.Pots }, Admin));

        for (var index = 0; index < 4; index++)
        {
            var player = FirstPlayer + index;
            var pot = index < 2 ? 1 : 2;

            await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().SetPotAsync(championship.Id, player, pot));
        }

        var placed = await hub.RunAsync(services => services.GetRequiredService<IChampionshipDraftService>().DrawPotsAsync(championship.Id, Admin));
        var drawn = await LoadAsync(hub, championship.Id);

        Assert.AreEqual(4, placed);

        foreach (var team in drawn.Teams)
        {
            var pots = team.Members.Select(member => drawn.Participants.First(participant => participant.CommunityId == member.CommunityId).Pot).OrderBy(pot => pot).ToList();

            CollectionAssert.AreEqual(new[] { 1, 2 }, pots);
            Assert.IsTrue(team.CaptainCommunityId is FirstPlayer or FirstPlayer + 1);
        }
    }

    [TestMethod]
    public async Task ADateIsSetWhenBothTeamsAcceptItAndOnlyTheirLeadersCanAnswer()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "Cup", Knockout());
        var teams = await TeamsAsync(hub, championship.Id, 2);
        var stageId = (await LoadAsync(hub, championship.Id)).Stages.Single().Id;

        await GenerateAsync(hub, stageId);

        var series = (await LoadAsync(hub, championship.Id)).Stages.Single().Series.Single();
        var schedule = (Func<IServiceProvider, IChampionshipScheduleService>)(services => services.GetRequiredService<IChampionshipScheduleService>());

        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => hub.RunAsync(services => schedule(services).ProposeAsync(series.Id, Friday, FirstPlayer + 2)));
        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => hub.RunAsync(services => schedule(services).ProposeAsync(series.Id, hub.Clock.UtcNow.AddHours(-1), FirstPlayer)));

        var proposal = await hub.RunAsync(services => schedule(services).ProposeAsync(series.Id, Friday, FirstPlayer + 1));

        Assert.AreEqual(ChampionshipAnswer.Accepted, proposal.TeamAAnswer);
        Assert.AreEqual(teams[0], proposal.TeamId);

        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => hub.RunAsync(services => schedule(services).AnswerAsync(proposal.Id, true, FirstPlayer)));

        await hub.RunAsync(services => schedule(services).AnswerAsync(proposal.Id, true, FirstPlayer + 100));

        var scheduled = Series(await LoadAsync(hub, championship.Id), series.Id);

        Assert.AreEqual(Friday, scheduled.ScheduledAtUtc);
        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => hub.RunAsync(services => schedule(services).DeleteAsync(proposal.Id, FirstPlayer, false)));

        var other = await hub.RunAsync(services => schedule(services).ProposeAsync(series.Id, Friday.AddDays(1), FirstPlayer + 100));

        await hub.RunAsync(services => schedule(services).AnswerAsync(other.Id, false, FirstPlayer));
        await hub.RunAsync(services => schedule(services).DeleteAsync(other.Id, FirstPlayer + 101, false));

        var after = Series(await LoadAsync(hub, championship.Id), series.Id);

        Assert.AreEqual(Friday, after.ScheduledAtUtc);
        Assert.HasCount(1, after.Proposals);
    }

    [TestMethod]
    public async Task LinkingAMatchFindsWhichSideEachTeamPlayedOnAndFeedsTheStatistics()
    {
        using var hub = new TestHub();

        hub.Clock.UtcNow = new DateTime(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc);

        var server = (await hub.CreateServerAsync()).Server;
        var night = new DateTime(2026, 11, 14, 21, 0, 0);
        var first = StatisticsBuilder.Map("c2m1_highway", night, 1, 300, 200);

        await hub.UploadAsync(server, first);
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", night.AddMinutes(15), 2, 450, 900));

        var championship = await CreateAsync(hub, "Cup", Knockout(bestOf: 1, rule: ChampionshipCampaignRule.Manual));
        var teamA = await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().CreateTeamAsync(championship.Id, "Blue", null, Admin));
        var teamB = await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().CreateTeamAsync(championship.Id, "Red", null, Admin));

        foreach (var player in first.Survivors('B'))
            await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().AddMemberAsync(teamA.Id, player, Admin));

        foreach (var player in first.Survivors('A'))
            await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().AddMemberAsync(teamB.Id, player, Admin));

        var stageId = (await LoadAsync(hub, championship.Id)).Stages.Single().Id;

        await GenerateAsync(hub, stageId);

        var series = (await LoadAsync(hub, championship.Id)).Stages.Single().Series.Single();
        var candidates = await hub.RunAsync(services => services.GetRequiredService<IChampionshipSeriesService>().CandidatesAsync(series.Id));
        var candidate = candidates.Single();

        Assert.IsFalse(candidate.MatchTeamAIsTeamA);
        Assert.AreEqual(4, candidate.TeamAPlayers);
        Assert.AreEqual(4, candidate.TeamBPlayers);

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipSeriesService>().LinkMatchAsync(series.Games[0].Id, candidate.MatchId, Admin));

        var loaded = await LoadAsync(hub, championship.Id);
        var game = Series(loaded, series.Id).Games[0];

        Assert.AreEqual(candidate.MatchId, game.MatchId);
        Assert.AreEqual(candidate.TeamAScore, game.TeamAScore);
        Assert.AreEqual(candidate.TeamBScore, game.TeamBScore);
        Assert.IsNotNull(game.CampaignId);
        Assert.IsTrue(Series(loaded, series.Id).Finished);

        var statistics = await hub.RunAsync(services => services.GetRequiredService<IChampionshipQueryService>().StatisticsAsync(loaded));

        Assert.HasCount(8, statistics);
        Assert.IsTrue(statistics.All(row => row.Games == 1));
        Assert.IsTrue(statistics.Where(row => row.TeamId == teamA.Id).All(row => first.Survivors('B').Contains(row.CommunityId)));

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipService>().SetStatusAsync(championship.Id, ChampionshipStatus.Finished, null, Admin));

        var link = await hub.RunAsync(services => services.GetRequiredService<IChampionshipQueryService>().MatchLinkAsync(candidate.MatchId));

        Assert.AreEqual("cup", link!.Slug);

        var other = await CreateAsync(hub, "Other", Knockout(bestOf: 1, rule: ChampionshipCampaignRule.Manual));
        var blue = await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().CreateTeamAsync(other.Id, "Blue", null, Admin));
        var red = await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().CreateTeamAsync(other.Id, "Red", null, Admin));

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().AddMemberAsync(blue.Id, first.Survivors('A')[0], Admin));
        await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().AddMemberAsync(red.Id, first.Survivors('B')[0], Admin));
        await GenerateAsync(hub, (await LoadAsync(hub, other.Id)).Stages.Single().Id);

        var otherGame = (await LoadAsync(hub, other.Id)).Stages.Single().Series.Single().Games[0];

        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => hub.RunAsync(services => services.GetRequiredService<IChampionshipSeriesService>().LinkMatchAsync(otherGame.Id, candidate.MatchId, Admin)));
    }

    [TestMethod]
    public async Task OnlyPublicChampionshipsAreListedAndTheFinishedOnesShowTheirChampion()
    {
        using var hub = new TestHub();

        var hidden = await CreateAsync(hub, "Hidden", Knockout());
        var done = await CreateAsync(hub, "Done", Knockout(bestOf: 1, rule: ChampionshipCampaignRule.Manual));
        var teams = await TeamsAsync(hub, done.Id, 2);

        await GenerateAsync(hub, (await LoadAsync(hub, done.Id)).Stages.Single().Id);

        var series = (await LoadAsync(hub, done.Id)).Stages.Single().Series.Single();

        await ResultAsync(hub, series.Games[0].Id, 100, 900);
        await hub.RunAsync(services => services.GetRequiredService<IChampionshipService>().SetStatusAsync(done.Id, ChampionshipStatus.Finished, null, Admin));

        var listed = await hub.RunAsync(services => services.GetRequiredService<IChampionshipService>().ListAsync(true));
        var all = await hub.RunAsync(services => services.GetRequiredService<IChampionshipService>().ListAsync(false));

        Assert.HasCount(1, listed);
        Assert.AreEqual("Team B", listed[0].Champion);
        Assert.AreEqual("Team A", listed[0].RunnerUp);
        Assert.HasCount(2, all);
        Assert.AreEqual(hidden.Id, all[0].Id);
        Assert.AreEqual(teams[1], (await LoadAsync(hub, done.Id)).Stages.Single().Series.Single().WinnerTeamId);
    }
}
