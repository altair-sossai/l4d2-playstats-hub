using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static L4D2PlayStats.Hub.Tests.Core.Championships.ChampionshipFixture;

namespace L4D2PlayStats.Hub.Tests.Core.Championships;

[TestClass]
public class ChampionshipStageTests
{
    [TestMethod]
    public async Task ATemplateCreatesItsStagesAndEachChampionshipGetsItsOwnAddress()
    {
        using var hub = new TestHub();

        var first = await CreateAsync(hub, "Copa L4D2", RoundRobin(qualifiers: 4), Knockout());
        var second = await CreateAsync(hub, "Copa L4D2", Knockout());

        Assert.AreEqual("copa-l4d2", first.Slug);
        Assert.AreEqual("copa-l4d2-2", second.Slug);
        Assert.AreEqual(ChampionshipStatus.Draft, first.Status);

        var loaded = await LoadAsync(hub, first.Id);
        var stages = loaded.Stages.OrderBy(stage => stage.Order).ToList();

        Assert.HasCount(2, stages);
        Assert.AreEqual(ChampionshipStageType.RoundRobin, stages[0].Type);
        Assert.AreEqual(4, stages[0].Qualifiers);
        Assert.AreEqual(ChampionshipStageType.Knockout, stages[1].Type);
        Assert.AreEqual(3, stages[1].BestOf);
        Assert.IsTrue(stages[1].OncePerTeam);
    }

    [TestMethod]
    public async Task BuildingARoundRobinCreatesEverySeriesAndDrawsOneCampaignPerRound()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "League", RoundRobin());
        await TeamsAsync(hub, championship.Id, 6);

        var stageId = (await LoadAsync(hub, championship.Id)).Stages.Single().Id;

        await GenerateAsync(hub, stageId);

        var loaded = await LoadAsync(hub, championship.Id);
        var stage = loaded.Stages.Single();

        Assert.AreEqual(ChampionshipStatus.Running, loaded.Status);
        Assert.IsTrue(stage.Generated);
        Assert.HasCount(1, stage.Groups);
        Assert.HasCount(15, stage.Series);
        Assert.IsTrue(stage.Series.All(series => series.Games.Count == 1));

        foreach (var round in stage.Series.GroupBy(series => series.Round))
        {
            var campaigns = round.Select(series => series.Games[0].CampaignId).Distinct().ToList();

            Assert.HasCount(1, campaigns);
            Assert.IsNotNull(campaigns[0]);
        }

        Assert.HasCount(5, stage.Series.Select(series => series.Games[0].CampaignId).Distinct().ToList());
    }

    [TestMethod]
    public async Task WithSixTeamsTheTwoBestSeedsWaitInTheSemifinals()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "Cup", Knockout());
        var teams = await TeamsAsync(hub, championship.Id, 6);
        var stageId = (await LoadAsync(hub, championship.Id)).Stages.Single().Id;

        await GenerateAsync(hub, stageId);

        var stage = (await LoadAsync(hub, championship.Id)).Stages.Single();
        var first = stage.Series.Where(series => series.Round == 1).OrderBy(series => series.Position).ToList();
        var semis = stage.Series.Where(series => series.Round == 2).OrderBy(series => series.Position).ToList();

        Assert.HasCount(4, first);
        Assert.IsTrue(first[0].Bye);
        Assert.IsTrue(first[2].Bye);
        Assert.AreEqual(teams[0], semis[0].TeamAId);
        Assert.IsNull(semis[0].TeamBId);
        Assert.AreEqual(teams[1], semis[1].TeamAId);
        Assert.AreEqual(teams[3], first[1].TeamAId);
        Assert.AreEqual(teams[4], first[1].TeamBId);
        Assert.HasCount(1, stage.Series.Where(series => series.Round == 3).ToList());
    }

    [TestMethod]
    public async Task TheWinnerMovesOnAndTheThirdGameIsNotNeededAfterTwoWins()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "Cup", Knockout());
        var teams = await TeamsAsync(hub, championship.Id, 4);
        var stageId = (await LoadAsync(hub, championship.Id)).Stages.Single().Id;

        await GenerateAsync(hub, stageId);

        var semifinal = (await LoadAsync(hub, championship.Id)).Stages.Single().Series.First(series => series.Round == 1 && series.Position == 1);

        await PlaySeriesAsync(hub, championship.Id, semifinal.Id, true);

        var loaded = await LoadAsync(hub, championship.Id);
        var played = Series(loaded, semifinal.Id);
        var final = loaded.Stages.Single().Series.Single(series => series.Round == 2);

        Assert.IsTrue(played.Finished);
        Assert.AreEqual(teams[0], played.WinnerTeamId);
        Assert.AreEqual(ChampionshipGameStatus.NotNeeded, played.Games.Single(game => game.Number == 3).Status);
        Assert.AreEqual(teams[0], final.TeamAId);
        Assert.IsNull(final.TeamBId);
    }

    [TestMethod]
    public async Task ATiedKnockoutGameAddsAnotherGameWithADrawnCampaign()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "Cup", Knockout(bestOf: 1));
        await TeamsAsync(hub, championship.Id, 2);
        var stageId = (await LoadAsync(hub, championship.Id)).Stages.Single().Id;

        await GenerateAsync(hub, stageId);

        var series = (await LoadAsync(hub, championship.Id)).Stages.Single().Series.Single();

        Assert.IsNotNull(series.Games.Single().CampaignId, "A best of one is decided by a drawn campaign.");

        await ResultAsync(hub, series.Games.Single().Id, 500, 500);

        var after = Series(await LoadAsync(hub, championship.Id), series.Id);
        var replay = after.Games.Single(game => game.Number == 2);

        Assert.IsFalse(after.Finished);
        Assert.IsTrue(replay.Replay);
        Assert.IsNotNull(replay.CampaignId);
        Assert.AreEqual(ChampionshipCampaignSource.Draw, replay.CampaignSource);

        await ResultAsync(hub, replay.Id, 700, 500);

        Assert.IsTrue(Series(await LoadAsync(hub, championship.Id), series.Id).Finished);

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipSeriesService>().ClearResultAsync(replay.Id, Admin));
        await ResultAsync(hub, series.Games.Single().Id, 600, 500);

        var decided = Series(await LoadAsync(hub, championship.Id), series.Id);

        Assert.HasCount(1, decided.Games);
        Assert.AreEqual(decided.TeamAId, decided.WinnerTeamId);
    }

    [TestMethod]
    public async Task TheDeciderIsDrawnOnlyAtOneAllAndAvoidsThePicksAndTheVetoes()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "Cup", Knockout());
        var stageId = (await LoadAsync(hub, championship.Id)).Stages.Single().Id;

        await TeamsAsync(hub, championship.Id, 2);
        await hub.RunAsync(services => services.GetRequiredService<IChampionshipStageService>().UpdateAsync(stageId, new ChampionshipStageForm { Name = "Final", Type = ChampionshipStageType.Knockout, BestOf = 3, FinalBestOf = 3, CampaignRule = ChampionshipCampaignRule.Picks, Veto = true, OncePerTeam = true, Tiebreakers = [] }, Admin));
        await GenerateAsync(hub, stageId);

        var series = (await LoadAsync(hub, championship.Id)).Stages.Single().Series.Single();
        var campaigns = await hub.RunAsync(services => services.GetRequiredService<IChampionshipLoadService>().CampaignsAsync());
        var captainA = FirstPlayer;
        var captainB = FirstPlayer + 100;
        var games = series.Games.OrderBy(game => game.Number).ToList();
        var series1 = series.Id;

        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => hub.RunAsync(services => services.GetRequiredService<IChampionshipSeriesService>().PickAsync(games[0].Id, campaigns[0].Id, captainA)));

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipSeriesService>().VetoAsync(series1, campaigns[0].Id, captainA));
        await hub.RunAsync(services => services.GetRequiredService<IChampionshipSeriesService>().VetoAsync(series1, campaigns[1].Id, captainB + 1));
        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => hub.RunAsync(services => services.GetRequiredService<IChampionshipSeriesService>().PickAsync(games[0].Id, campaigns[2].Id, captainB)));
        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => hub.RunAsync(services => services.GetRequiredService<IChampionshipSeriesService>().PickAsync(games[0].Id, campaigns[1].Id, captainA)));

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipSeriesService>().PickAsync(games[0].Id, campaigns[2].Id, captainA));
        await hub.RunAsync(services => services.GetRequiredService<IChampionshipSeriesService>().PickAsync(games[1].Id, campaigns[3].Id, captainB));

        Assert.IsNull(Series(await LoadAsync(hub, championship.Id), series1).Games.Single(game => game.Number == 3).CampaignId);

        await ResultAsync(hub, games[0].Id, 600, 400);
        await ResultAsync(hub, games[1].Id, 300, 400);

        var decider = Series(await LoadAsync(hub, championship.Id), series1).Games.Single(game => game.Number == 3);

        Assert.IsNotNull(decider.CampaignId);
        Assert.AreEqual(ChampionshipCampaignSource.Draw, decider.CampaignSource);
        CollectionAssert.DoesNotContain(new[] { campaigns[0].Id, campaigns[1].Id, campaigns[2].Id, campaigns[3].Id }, decider.CampaignId!.Value);
    }

    [TestMethod]
    public async Task AResultCannotChangeTheWinnerOnceTheNextSeriesHasStarted()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "Cup", Knockout(bestOf: 1, rule: ChampionshipCampaignRule.Manual));
        await TeamsAsync(hub, championship.Id, 4);
        var stageId = (await LoadAsync(hub, championship.Id)).Stages.Single().Id;

        await GenerateAsync(hub, stageId);

        var stage = (await LoadAsync(hub, championship.Id)).Stages.Single();
        var first = stage.Series.Single(series => series.Round == 1 && series.Position == 1);
        var second = stage.Series.Single(series => series.Round == 1 && series.Position == 2);

        await ResultAsync(hub, first.Games[0].Id, 500, 100);
        await ResultAsync(hub, second.Games[0].Id, 500, 100);

        var final = Series(await LoadAsync(hub, championship.Id), stage.Series.Single(series => series.Round == 2).Id);

        Assert.IsTrue(final.HasTeams);

        await ResultAsync(hub, final.Games[0].Id, 400, 300);

        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => ResultAsync(hub, first.Games[0].Id, 100, 500));

        var podium = ChampionshipPodiums.Of(await LoadAsync(hub, championship.Id));

        Assert.AreEqual(final.TeamAId, podium.Champion);
        Assert.AreEqual(final.TeamBId, podium.RunnerUp);
    }

    [TestMethod]
    public async Task TheBestOfEachGroupMoveOnToTheKnockoutInTheOrderOfTheStandings()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "World Cup", RoundRobin(groups: 2, qualifiers: 2), Knockout(bestOf: 1, seeding: ChampionshipSeeding.Standings, rule: ChampionshipCampaignRule.Manual));
        var teams = await TeamsAsync(hub, championship.Id, 8);
        var stages = (await LoadAsync(hub, championship.Id)).Stages.OrderBy(stage => stage.Order).ToList();

        await GenerateAsync(hub, stages[0].Id);

        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => GenerateAsync(hub, stages[1].Id));

        var groups = (await LoadAsync(hub, championship.Id)).Stages.First(stage => stage.Id == stages[0].Id);

        Assert.HasCount(2, groups.Groups);
        Assert.HasCount(12, groups.Series);

        foreach (var series in groups.Series)
        {
            var seedA = teams.IndexOf(series.TeamAId!.Value);
            var seedB = teams.IndexOf(series.TeamBId!.Value);

            await ResultAsync(hub, series.Games[0].Id, seedA < seedB ? 500 : 100, seedA < seedB ? 100 : 500);
        }

        var entrants = await hub.RunAsync(services => services.GetRequiredService<IChampionshipStageService>().EntrantsAsync(stages[1].Id));

        CollectionAssert.AreEqual(new[] { teams[0], teams[1], teams[3], teams[2] }, entrants);

        await GenerateAsync(hub, stages[1].Id);

        var knockout = (await LoadAsync(hub, championship.Id)).Stages.First(stage => stage.Id == stages[1].Id);
        var semifinal = knockout.Series.Single(series => series.Round == 1 && series.Position == 1);

        Assert.AreEqual(teams[0], semifinal.TeamAId);
        Assert.AreEqual(teams[2], semifinal.TeamBId);

        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => hub.RunAsync(services => services.GetRequiredService<IChampionshipStageService>().ResetAsync(stages[0].Id, Admin)));
    }

    [TestMethod]
    public async Task AWalkoverCountsAsAWinAndPenaltyPointsComeOffTheStandings()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "League", RoundRobin(rule: ChampionshipCampaignRule.Manual));
        var teams = await TeamsAsync(hub, championship.Id, 2);
        var stageId = (await LoadAsync(hub, championship.Id)).Stages.Single().Id;

        await GenerateAsync(hub, stageId);

        var series = (await LoadAsync(hub, championship.Id)).Stages.Single().Series.Single();

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipSeriesService>().WalkoverAsync(series.Id, teams[1], Admin));
        await hub.RunAsync(services => services.GetRequiredService<IChampionshipStageService>().AddPenaltyAsync(stageId, teams[1], 2, "Late", Admin));

        var stage = (await LoadAsync(hub, championship.Id)).Stages.Single();
        var standings = StandingsCalculator.Calculate(stage, stage.Groups[0].Teams.ToDictionary(team => team.TeamId, team => team.Seed), stage.Series, stage.Penalties);
        var winner = standings.Single(row => row.TeamId == teams[1]);

        Assert.AreEqual(1, winner.Wins);
        Assert.AreEqual(1, winner.Points);
        Assert.AreEqual(ChampionshipGameStatus.NotNeeded, stage.Series.Single().Games[0].Status);

        var podium = ChampionshipPodiums.Of(await LoadAsync(hub, championship.Id));

        Assert.AreEqual(teams[1], podium.Champion);
    }

    [TestMethod]
    public async Task ResettingAStageDeletesItsSeriesAndDeletingTheChampionshipRemovesEverything()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "Cup", Knockout());
        await TeamsAsync(hub, championship.Id, 5);
        var stageId = (await LoadAsync(hub, championship.Id)).Stages.Single().Id;

        await GenerateAsync(hub, stageId);
        await hub.RunAsync(services => services.GetRequiredService<IChampionshipStageService>().ResetAsync(stageId, Admin));

        var stage = (await LoadAsync(hub, championship.Id)).Stages.Single();

        Assert.IsFalse(stage.Generated);
        Assert.IsEmpty(stage.Series);

        await GenerateAsync(hub, stageId);
        await hub.RunAsync(services => services.GetRequiredService<IChampionshipService>().DeleteAsync(championship.Id));

        var left = await hub.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            return await dbContext.ChampionshipSeries.CountAsync() + await dbContext.ChampionshipTeams.CountAsync() + await dbContext.ChampionshipGames.CountAsync() + await dbContext.Championships.CountAsync();
        });

        Assert.AreEqual(0, left);
    }
}
