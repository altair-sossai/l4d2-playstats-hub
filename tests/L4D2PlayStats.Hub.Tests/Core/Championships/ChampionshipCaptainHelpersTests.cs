using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Matches;

namespace L4D2PlayStats.Hub.Tests.Core.Championships;

[TestClass]
public class ChampionshipCaptainHelpersTests
{
    private static readonly DateTime Now = new(2026, 12, 1, 20, 0, 0, DateTimeKind.Utc);

    [TestMethod]
    public void TheSidesOfAMatchFollowWhereMostPlayersOfEachTeamPlayed()
    {
        var blue = new HashSet<long> { 1, 2, 3, 4 };
        var red = new HashSet<long> { 5, 6, 7, 8 };

        Assert.IsTrue(ChampionshipMatchSides.Detect(Players([1, 2, 3, 4], [5, 6, 7, 8]), blue, red, 4));
        Assert.IsFalse(ChampionshipMatchSides.Detect(Players([5, 6, 7, 8], [1, 2, 3, 4]), blue, red, 4));
        Assert.IsTrue(ChampionshipMatchSides.Detect(Players([1, 2, 90, 91], [5, 6, 92, 93]), blue, red, 4));
        Assert.IsNull(ChampionshipMatchSides.Detect(Players([1, 90, 91, 92], [5, 6, 7, 8]), blue, red, 4));
        Assert.IsNull(ChampionshipMatchSides.Detect(Players([1, 2, 5, 6], [3, 4, 7, 8]), blue, red, 4));
        Assert.IsNull(ChampionshipMatchSides.Detect(Players([90, 91, 92, 93], [94, 95, 96, 97]), blue, red, 4));
        Assert.IsTrue(ChampionshipMatchSides.Detect(Players([1], [5]), blue, red, 1));
    }

    [TestMethod]
    public void TheTasksOfACaptainFollowTheDraftTheVetoesThePicksAndTheDates()
    {
        var (championship, teamA, teamB, series) = Running();

        var tasks = CaptainTasks.Of(championship, teamA, Now);

        CollectionAssert.AreEqual(new[] { ChampionshipCaptainTaskKind.Veto, ChampionshipCaptainTaskKind.ProposeDate }, tasks.Select(task => task.Kind).ToArray());

        series.TeamAVetoCampaignId = 1;
        series.TeamBVetoCampaignId = 2;

        tasks = CaptainTasks.Of(championship, teamA, Now);

        CollectionAssert.AreEqual(new[] { ChampionshipCaptainTaskKind.Pick, ChampionshipCaptainTaskKind.ProposeDate }, tasks.Select(task => task.Kind).ToArray());
        Assert.AreEqual(1, tasks[0].GameNumber);
        CollectionAssert.AreEqual(new[] { ChampionshipCaptainTaskKind.Pick, ChampionshipCaptainTaskKind.ProposeDate }, CaptainTasks.Of(championship, teamB, Now).Select(task => task.Kind).ToArray());
        Assert.AreEqual(2, CaptainTasks.Of(championship, teamB, Now)[0].GameNumber);

        series.Games[0].CampaignId = 3;
        series.Proposals.Add(new ChampionshipDateProposal { Id = 1, TeamId = teamA.Id, DateUtc = Now.AddDays(1), TeamAAnswer = ChampionshipAnswer.Accepted });

        Assert.IsEmpty(CaptainTasks.Of(championship, teamA, Now));
        CollectionAssert.AreEqual(new[] { ChampionshipCaptainTaskKind.Pick, ChampionshipCaptainTaskKind.AnswerDate }, CaptainTasks.Of(championship, teamB, Now).Select(task => task.Kind).ToArray());
        Assert.IsEmpty(CaptainTasks.Of(championship, teamA, Now.AddDays(2)).Where(task => task.Kind == ChampionshipCaptainTaskKind.AnswerDate).ToList());

        teamA.RejectionReason = string.Empty;

        Assert.AreEqual(ChampionshipCaptainTaskKind.IdentityRejected, CaptainTasks.Of(championship, teamA, Now)[0].Kind);

        teamA.PendingName = "New";

        Assert.IsEmpty(CaptainTasks.Of(championship, teamA, Now));

        championship.Status = ChampionshipStatus.Finished;

        Assert.IsEmpty(CaptainTasks.Of(championship, teamB, Now));
    }

    [TestMethod]
    public void TheTeamOnTheClockOfTheDraftHasATask()
    {
        var championship = new Championship { Id = 1, Name = "Draft", Slug = "draft", Status = ChampionshipStatus.TeamFormation, DraftMethod = ChampionshipDraftMethod.Snake, RosterSize = 2 };
        var teamA = new ChampionshipTeam { Id = 1, Name = "A", DraftPosition = 1, CaptainCommunityId = 10, Members = [new ChampionshipTeamMember { CommunityId = 10 }] };
        var teamB = new ChampionshipTeam { Id = 2, Name = "B", DraftPosition = 2, CaptainCommunityId = 20, Members = [new ChampionshipTeamMember { CommunityId = 20 }] };

        championship.Teams.AddRange([teamA, teamB]);
        championship.Participants.AddRange(new long[] { 10, 20, 30, 40 }.Select(id => new ChampionshipParticipant { CommunityId = id }));

        Assert.AreEqual(ChampionshipCaptainTaskKind.DraftTurn, CaptainTasks.Of(championship, teamA, Now).Single().Kind);
        Assert.IsEmpty(CaptainTasks.Of(championship, teamB, Now));

        championship.Status = ChampionshipStatus.Draft;

        Assert.IsEmpty(CaptainTasks.Of(championship, teamA, Now));
    }

    private static (Championship, ChampionshipTeam, ChampionshipTeam, ChampionshipSeries) Running()
    {
        var championship = new Championship { Id = 1, Name = "Cup", Slug = "cup", Status = ChampionshipStatus.Running };
        var teamA = new ChampionshipTeam { Id = 1, Name = "A", CaptainCommunityId = 10 };
        var teamB = new ChampionshipTeam { Id = 2, Name = "B", CaptainCommunityId = 20 };
        var series = new ChampionshipSeries { Id = 1, Round = 1, Position = 1, TeamAId = 1, TeamBId = 2, BestOf = 3 };
        var stage = new ChampionshipStage { Id = 1, Order = 1, Name = "Final", Type = ChampionshipStageType.Knockout, CampaignRule = ChampionshipCampaignRule.Picks, Veto = true, Generated = true };

        series.Games.AddRange(Enumerable.Range(1, 3).Select(number => new ChampionshipGame { Id = number, Number = number, Status = ChampionshipGameStatus.Pending }));
        stage.Series.Add(series);
        championship.Stages.Add(stage);
        championship.Teams.AddRange([teamA, teamB]);

        return (championship, teamA, teamB, series);
    }

    private static List<MatchPlayer> Players(long[] teamA, long[] teamB)
    {
        return teamA.Select(id => new MatchPlayer { CommunityId = id, TeamA = true }).Concat(teamB.Select(id => new MatchPlayer { CommunityId = id, TeamB = true })).ToList();
    }
}
