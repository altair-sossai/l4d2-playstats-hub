using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;

namespace L4D2PlayStats.Hub.Tests.Core.Championships;

[TestClass]
public class ChampionshipHelpersTests
{
    [TestMethod]
    public void InARoundRobinEveryTeamMeetsEveryOtherTeamOnceAndPlaysOncePerRound()
    {
        var pairings = RoundRobinScheduler.Rounds([1, 2, 3, 4, 5, 6], 1);

        Assert.HasCount(15, pairings);
        Assert.AreEqual(5, pairings.Max(pairing => pairing.Round));

        foreach (var round in pairings.GroupBy(pairing => pairing.Round))
        {
            var teams = round.SelectMany(pairing => new[] { pairing.TeamA, pairing.TeamB }).ToList();

            Assert.HasCount(6, teams);
            Assert.HasCount(6, teams.Distinct().ToList());
        }

        var meetings = pairings.Select(pairing => (Math.Min(pairing.TeamA, pairing.TeamB), Math.Max(pairing.TeamA, pairing.TeamB))).Distinct().ToList();

        Assert.HasCount(15, meetings);
    }

    [TestMethod]
    public void WithAnOddNumberOfTeamsOneTeamRestsInEachRound()
    {
        var pairings = RoundRobinScheduler.Rounds([1, 2, 3, 4, 5], 1);

        Assert.HasCount(10, pairings);
        Assert.AreEqual(5, pairings.Max(pairing => pairing.Round));
        Assert.IsTrue(pairings.GroupBy(pairing => pairing.Round).All(round => round.Count() == 2));
    }

    [TestMethod]
    public void TheSecondLegRepeatsTheFirstWithTheSidesSwapped()
    {
        var pairings = RoundRobinScheduler.Rounds([1, 2, 3, 4], 2);
        var first = pairings.Where(pairing => pairing.Round <= 3).ToList();
        var second = pairings.Where(pairing => pairing.Round > 3).ToList();

        Assert.HasCount(6, first);
        Assert.HasCount(6, second);

        foreach (var pairing in first)
            Assert.IsTrue(second.Any(other => other.Round == pairing.Round + 3 && other.TeamA == pairing.TeamB && other.TeamB == pairing.TeamA));
    }

    [TestMethod]
    public void TheBracketPlacesTheSeedsSoTheBestOnlyMeetInTheEnd()
    {
        CollectionAssert.AreEqual(new[] { 1, 8, 4, 5, 2, 7, 3, 6 }, ChampionshipBracket.SeedOrder(8));
        CollectionAssert.AreEqual(new[] { 1, 4, 2, 3 }, ChampionshipBracket.SeedOrder(4));
        Assert.AreEqual(8, ChampionshipBracket.Size(6));
        Assert.AreEqual(3, ChampionshipBracket.Rounds(6));
        Assert.AreEqual(1, ChampionshipBracket.Rounds(2));
    }

    [TestMethod]
    public void WithSixTeamsTheTopTwoSeedsGetAByeAndThirdMeetsSixthAndFourthMeetsFifth()
    {
        var slots = ChampionshipBracket.FirstRound([10, 20, 30, 40, 50, 60]);

        Assert.HasCount(4, slots);
        Assert.AreEqual(new BracketSlot(1, 10, null), slots[0]);
        Assert.AreEqual(new BracketSlot(2, 40, 50), slots[1]);
        Assert.AreEqual(new BracketSlot(3, 20, null), slots[2]);
        Assert.AreEqual(new BracketSlot(4, 30, 60), slots[3]);
    }

    [TestMethod]
    public void TheGroupsAreFilledInASnakeSoTheSeedsAreSpread()
    {
        var groups = GroupSplitter.Split([1, 2, 3, 4, 5, 6, 7, 8], 2);

        CollectionAssert.AreEqual(new[] { 1, 4, 5, 8 }, groups[0]);
        CollectionAssert.AreEqual(new[] { 2, 3, 6, 7 }, groups[1]);
        Assert.AreEqual("A", GroupSplitter.Name(0));
        Assert.AreEqual("B", GroupSplitter.Name(1));
    }

    [TestMethod]
    public void TheSnakeDraftGoesBackAndForthAndTheLinearDraftDoesNot()
    {
        var snake = Enumerable.Range(0, 8).Select(index => DraftOrder.Position(index, 4, true)).ToList();
        var linear = Enumerable.Range(0, 8).Select(index => DraftOrder.Position(index, 4, false)).ToList();

        CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 3, 2, 1, 0 }, snake);
        CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 0, 1, 2, 3 }, linear);
        Assert.AreEqual(2, DraftOrder.Round(5, 4));
    }

    [TestMethod]
    public void ABestOfThreeEndsAfterTwoWinsAndATiedKnockoutGameNeedsAnotherGame()
    {
        var series = Series(3, (100, 50), (200, 100));
        var progress = SeriesCalculator.Evaluate(series, true);

        Assert.IsTrue(progress.Finished);
        Assert.AreEqual(1, progress.WinnerTeamId);
        Assert.AreEqual(2, progress.WinsA);

        var tied = Series(3, (100, 100), (200, 100));
        var tiedProgress = SeriesCalculator.Evaluate(tied, true);

        Assert.IsFalse(tiedProgress.Finished);
        Assert.AreEqual(1, tiedProgress.Ties);
        Assert.AreEqual(4, tiedProgress.RequiredGames);
    }

    [TestMethod]
    public void ATiedGameInARoundRobinIsADrawAndPenaltiesDecideTheWinner()
    {
        var draw = SeriesCalculator.Evaluate(Series(1, (400, 400)), false);

        Assert.IsTrue(draw.Finished);
        Assert.IsTrue(draw.Draw);

        var series = Series(1, (400, 350));

        series.Games[0].TeamAPenalty = 100;

        var progress = SeriesCalculator.Evaluate(series, false);

        Assert.AreEqual(2, progress.WinnerTeamId);
    }

    [TestMethod]
    public void TheStandingsGiveThreePointsForAWinAndUseTheDifferenceToBreakTies()
    {
        var stage = new ChampionshipStage { Tiebreakers = ChampionshipTiebreakers.Format([ChampionshipTiebreaker.Difference]) };
        var seeds = new Dictionary<int, int> { [1] = 1, [2] = 2, [3] = 3 };
        var series = new[]
        {
            Series(1, 1, 2, (500, 100)),
            Series(1, 2, 3, (500, 400)),
            Series(1, 3, 1, (500, 400))
        };

        var standings = StandingsCalculator.Calculate(stage, seeds, series, []);

        Assert.IsTrue(standings.All(row => row.Points == 3));
        CollectionAssert.AreEqual(new[] { 1, 3, 2 }, standings.Select(row => row.TeamId).ToList());
        Assert.AreEqual(300, standings[0].Difference);
    }

    [TestMethod]
    public void PenaltyPointsAndTheHeadToHeadChangeTheOrder()
    {
        var stage = new ChampionshipStage { Tiebreakers = ChampionshipTiebreakers.Format([ChampionshipTiebreaker.HeadToHead]) };
        var seeds = new Dictionary<int, int> { [1] = 1, [2] = 2 };
        var series = new[] { Series(1, 2, 1, (300, 200)), Series(1, 1, 2, (900, 100)) };

        var standings = StandingsCalculator.Calculate(stage, seeds, series, []);

        Assert.AreEqual(3, standings[0].Points);
        Assert.AreEqual(3, standings[1].Points);
        Assert.AreEqual(1, standings[0].TeamId);

        var penalized = StandingsCalculator.Calculate(stage, seeds, series, [new ChampionshipPenalty { TeamId = 1, Points = 1 }]);

        Assert.AreEqual(2, penalized[0].TeamId);
        Assert.AreEqual(2, penalized[1].Points);
    }

    [TestMethod]
    public void TheAddressComesFromTheNameWithoutAccents()
    {
        Assert.AreEqual("campeonato-brasileiro-de-acao-2026", ChampionshipSlugs.From("Campeonato Brasileiro de Ação 2026"));
        Assert.AreEqual("4-torneio-zonemod", ChampionshipSlugs.From("  4° Torneio — ZoneMod!  "));
        Assert.AreEqual("championship", ChampionshipSlugs.From("???"));
    }

    [TestMethod]
    public void ACampaignAlreadyPickedByTheTeamOrVetoedCannotBeChosenAgain()
    {
        var stage = new ChampionshipStage { OncePerTeam = true, CampaignRule = ChampionshipCampaignRule.Picks };
        var campaigns = new List<ChampionshipCampaignOption> { new(1, "Dark Carnival", 5, true), new(2, "Dead Center", 4, true), new(3, "No Mercy", 5, true), new(4, "The Passing", 3, true) };
        var earlier = new ChampionshipGame { Id = 50, CampaignId = 1, CampaignSource = ChampionshipCampaignSource.Pick, PickedByTeamId = 7 };
        var series = new ChampionshipSeries { TeamAId = 7, TeamBId = 8, BestOf = 3, TeamBVetoCampaignId = 2 };
        var game = new ChampionshipGame { Id = 60, Number = 1 };

        series.Games.Add(game);

        var allowed = CampaignRules.ForPick(stage, series, game, 7, campaigns, [earlier, game]);

        CollectionAssert.AreEqual(new[] { 3, 4 }, allowed.Select(campaign => campaign.Id).ToList());

        stage.MinimumMaps = 4;

        var draw = CampaignRules.ForDraw(stage, series, game, campaigns);

        CollectionAssert.AreEquivalent(new[] { 1, 3 }, draw.Select(campaign => campaign.Id).ToList());
    }

    [TestMethod]
    public void TheLastGameOfAnOddSeriesIsTheDeciderAndTheOthersAlternateBetweenTheTeams()
    {
        var games = Enumerable.Range(1, 5).Select(number => new ChampionshipGame { Number = number }).ToList();

        CollectionAssert.AreEqual(new bool?[] { true, false, true, false, null }, games.Select(game => GamePlan.PickedByTeamA(game, 5)).ToList());
        Assert.IsTrue(GamePlan.IsDecider(new ChampionshipGame { Number = 1 }, 1));
        Assert.IsTrue(GamePlan.IsDecider(new ChampionshipGame { Number = 2, Replay = true }, 3));
        Assert.IsFalse(GamePlan.IsDecider(new ChampionshipGame { Number = 2 }, 2));
    }

    private static ChampionshipSeries Series(int bestOf, params (int A, int B)[] scores)
    {
        return Series(bestOf, 1, 2, scores);
    }

    private static ChampionshipSeries Series(int bestOf, int teamA, int teamB, params (int A, int B)[] scores)
    {
        var series = new ChampionshipSeries { TeamAId = teamA, TeamBId = teamB, BestOf = bestOf };

        for (var index = 0; index < scores.Length; index++)
            series.Games.Add(new ChampionshipGame { Number = index + 1, Status = ChampionshipGameStatus.Played, TeamAScore = scores[index].A, TeamBScore = scores[index].B });

        return series;
    }
}
