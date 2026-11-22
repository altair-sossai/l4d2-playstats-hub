using System.Collections.Immutable;
using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class RankingCalculatorTests
{
    private const long Ana = 76561198000000001;
    private const long Bia = 76561198000000002;
    private const long Caio = 76561198000000003;
    private const long Davi = 76561198000000004;

    private static readonly RankingSettings Settings = new();
    private static readonly ImmutableDictionary<long, int> NoPunishments = [];

    [TestMethod]
    public void AWinAndALossGiveTheConfiguredPoints()
    {
        var ranking = Calculate(Match(1, Winner(Ana), Loser(Bia)), Match(2, Winner(Ana), Loser(Bia)));

        Assert.AreEqual(170, Player(ranking, Ana).Experience);
        Assert.AreEqual(2, Player(ranking, Ana).Wins);
        Assert.AreEqual(0, Player(ranking, Bia).Experience);
        Assert.AreEqual(2, Player(ranking, Bia).Loss);
    }

    [TestMethod]
    public void ExperienceNeverGoesBelowZero()
    {
        var ranking = Calculate(Match(1, Winner(Ana), Loser(Bia)), Match(2, Winner(Bia), Loser(Ana)));

        Assert.AreEqual(5, Player(ranking, Ana).Experience);
        Assert.AreEqual(85, Player(ranking, Bia).Experience);
    }

    [TestMethod]
    public void MvpsAddBonusPoints()
    {
        var ranking = Calculate(Match(1, Winner(Ana, 2, 1), Loser(Bia)));

        Assert.AreEqual(85 + 2 * 8 + 6, Player(ranking, Ana).Experience);
        Assert.AreEqual(3, Player(ranking, Ana).Stars);
    }

    [TestMethod]
    public void TankWipesAddBonusPointsButNoStars()
    {
        var winner = Winner(Ana, 1);
        var loser = Loser(Bia);

        winner.TankWipes = 2;
        loser.TankWipes = 1;

        var ranking = Calculate(Match(1, Winner(Bia), Loser(Caio)), Match(2, winner, loser));

        Assert.AreEqual(85 + 8 + 2 * 50, Player(ranking, Ana).Experience);
        Assert.AreEqual(2, Player(ranking, Ana).TankWipes);
        Assert.AreEqual(1, Player(ranking, Ana).Stars);
        Assert.AreEqual(85 - 80 + 50, Player(ranking, Bia).Experience);
        Assert.AreEqual(1, Player(ranking, Bia).TankWipes);
    }

    [TestMethod]
    public void TheTankWipePointsComeFromTheSettingsAndZeroTurnsThemOff()
    {
        var winner = Winner(Ana);

        winner.TankWipes = 3;

        var matches = new[] { Match(1, winner, Loser(Bia)) };
        var custom = RankingCalculator.Calculate(matches, NoPunishments, new RankingSettings { TankWipe = 4 });
        var off = RankingCalculator.Calculate(matches, NoPunishments, new RankingSettings { TankWipe = 0 });

        Assert.AreEqual(85 + 3 * 4, Player(custom, Ana).Experience);
        Assert.AreEqual(85, Player(off, Ana).Experience);
        Assert.AreEqual(3, Player(off, Ana).TankWipes);
    }

    [TestMethod]
    public void ARageQuitAlsoReplacesTheTankWipePoints()
    {
        var quitter = new MatchPlayer { CommunityId = Caio, Name = "Caio", RageQuits = 1, Participant = true, TeamA = true, TankWipes = 2 };
        var ranking = Calculate(Match(1, Winner(Caio), Loser(Bia)), Match(2, Winner(Caio), Loser(Bia)), Match(3, Winner(Ana), Loser(Bia), quitter));

        Assert.AreEqual(2 * 85 - 150, Player(ranking, Caio).Experience);
    }

    [TestMethod]
    public void ARageQuitReplacesEverythingElse()
    {
        var quitter = new MatchPlayer { CommunityId = Caio, Name = "Caio", RageQuits = 1, Participant = true };
        var ranking = Calculate(Match(1, Winner(Caio), Loser(Bia)), Match(2, Winner(Ana), Loser(Bia), quitter));

        Assert.AreEqual(0, Player(ranking, Caio).Experience);
        Assert.AreEqual(1, Player(ranking, Caio).RageQuit);
        Assert.AreEqual(1, Player(ranking, Caio).Loss);
    }

    [TestMethod]
    public void ADrawGivesNoPoints()
    {
        var first = new MatchPlayer { CommunityId = Ana, Name = "Ana", Draws = 1, TeamA = true, Participant = true };
        var second = new MatchPlayer { CommunityId = Bia, Name = "Bia", Draws = 1, TeamB = true, Participant = true };

        var ranking = Calculate(Match(1, first, second));

        Assert.AreEqual(0, Player(ranking, Ana).Experience);
        Assert.AreEqual(1, Player(ranking, Ana).Draws);
        Assert.AreEqual(0, Player(ranking, Ana).Games);
    }

    [TestMethod]
    public void TheOrderIsExperienceThenWinsThenLosses()
    {
        var ranking = Calculate(
            Match(1, Winner(Ana), Loser(Bia)),
            Match(2, Winner(Caio, 1), Loser(Davi)),
            Match(3, Winner(Bia), Loser(Davi)));

        CollectionAssert.AreEqual(new[] { Caio, Ana, Bia, Davi }, ranking.Select(p => p.CommunityId).ToList());
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, ranking.Select(p => p.Position).ToList());
    }

    [TestMethod]
    public void PunishmentsAreSubtractedAtTheEnd()
    {
        var ranking = RankingCalculator.Calculate(
            [Match(1, Winner(Ana), Loser(Bia)), Match(2, Winner(Ana), Loser(Bia))],
            new Dictionary<long, int> { [Ana] = 100 },
            Settings);

        Assert.AreEqual(70, Player(ranking, Ana).Experience);
        Assert.AreEqual(100, Player(ranking, Ana).Punishment);
    }

    [TestMethod]
    public void ThePreviousExperienceIsFromBeforeTheLastMatch()
    {
        var ranking = Calculate(Match(1, Winner(Ana), Loser(Bia)), Match(2, Winner(Ana), Loser(Caio)));

        Assert.AreEqual(85, Player(ranking, Ana).PreviousExperience);
        Assert.AreEqual(85, Player(ranking, Ana).ExperienceDifference);
        Assert.IsNull(Player(ranking, Bia).PreviousExperience);
    }

    [TestMethod]
    public void TheMostUsedNameIsTheMainName()
    {
        var ranking = Calculate(
            Match(1, Winner(Ana, name: "ana"), Loser(Bia)),
            Match(2, Winner(Ana, name: "Ana Pro"), Loser(Bia)),
            Match(3, Winner(Ana, name: "Ana Pro"), Loser(Bia)));

        Assert.AreEqual("Ana Pro", Player(ranking, Ana).MainName);
        Assert.AreEqual("Ana Pro | ana", Player(ranking, Ana).MostUsedNames);
    }

    [TestMethod]
    public void RelationsCountGamesTogetherAndAgainst()
    {
        var ranking = Calculate(Match(1, Winner(Ana), Winner(Bia), Loser(Caio), Loser(Davi)));
        var relations = Player(ranking, Ana).Relations;

        Assert.AreEqual(1, relations[Bia].TogetherWins);
        Assert.AreEqual(1, relations[Caio].AgainstWins);
        Assert.AreEqual(1, Player(ranking, Caio).Relations[Ana].AgainstLosses);
        Assert.AreEqual(1, Player(ranking, Caio).Relations[Davi].TogetherLosses);
    }

    [TestMethod]
    public void StreaksAndCampaignsAreTracked()
    {
        var ranking = Calculate(
            Match(1, "Dark Carnival", Winner(Ana), Loser(Bia)),
            Match(2, "Dark Carnival", Winner(Ana), Loser(Bia)),
            Match(3, "No Mercy", Winner(Bia), Loser(Ana)));

        var ana = Player(ranking, Ana);

        Assert.AreEqual(2, ana.MaxWinStreak);
        Assert.AreEqual(1, ana.MaxLossStreak);
        Assert.AreEqual(2, ana.Campaigns["Dark Carnival"].Wins);
        Assert.AreEqual(1, ana.Campaigns["No Mercy"].Loss);
    }

    [TestMethod]
    public void StatisticsAccumulateOverMatches()
    {
        var first = Winner(Ana);
        first.Common = 100;
        first.SiDamage = 2000;

        var second = Winner(Ana);
        second.Common = 50;
        second.SiDamage = 500;

        var ranking = Calculate(Match(1, first, Loser(Bia)), Match(2, second, Loser(Bia)));

        Assert.AreEqual(150, Player(ranking, Ana).Common);
        Assert.AreEqual(2500, Player(ranking, Ana).SiDamage);
    }

    [TestMethod]
    public void AMinimumNumberOfMatchesCanBeRequired()
    {
        var settings = new RankingSettings { MinimumMatches = 2 };

        var ranking = RankingCalculator.Calculate(
            [Match(1, Winner(Ana), Loser(Bia)), Match(2, Winner(Ana), Loser(Caio))],
            NoPunishments,
            settings);

        CollectionAssert.AreEqual(new[] { Ana }, ranking.Select(p => p.CommunityId).ToList());
        Assert.AreEqual(1, ranking[0].Position);
    }

    [TestMethod]
    public void ThePointsComeFromTheSettings()
    {
        var settings = new RankingSettings { Win = 10, Loss = -3, Mvp = 1, MvpCommon = 2 };

        var ranking = RankingCalculator.Calculate(
            [Match(1, Winner(Ana, 1, 1), Loser(Bia)), Match(2, Winner(Bia), Loser(Ana))],
            NoPunishments,
            settings);

        Assert.AreEqual(10, Player(ranking, Ana).Experience);
        Assert.AreEqual(10, Player(ranking, Bia).Experience);
    }

    private static List<RankingPlayer> Calculate(params RankingMatch[] matches)
    {
        return RankingCalculator.Calculate(matches, NoPunishments, Settings);
    }

    private static RankingPlayer Player(List<RankingPlayer> ranking, long communityId)
    {
        return ranking.Single(p => p.CommunityId == communityId);
    }

    private static RankingMatch Match(int id, params MatchPlayer[] players)
    {
        return Match(id, "Dark Carnival", players);
    }

    private static RankingMatch Match(int id, string campaign, params MatchPlayer[] players)
    {
        for (var index = 0; index < players.Length; index++)
            players[index].Order = index;

        return new RankingMatch(id, campaign, players);
    }

    private static MatchPlayer Winner(long communityId, int mvpSiDamage = 0, int mvpCommon = 0, string? name = null)
    {
        return new MatchPlayer
        {
            CommunityId = communityId,
            Name = name ?? communityId.ToString(),
            Win = true,
            TeamA = true,
            Participant = true,
            MvpSiDamage = mvpSiDamage,
            MvpCommon = mvpCommon
        };
    }

    private static MatchPlayer Loser(long communityId)
    {
        return new MatchPlayer { CommunityId = communityId, Name = communityId.ToString(), Loss = true, TeamB = true, Participant = true };
    }
}
