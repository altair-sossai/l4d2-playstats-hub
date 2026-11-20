using System.Text.Json;
using L4D2PlayStats.Hub.Tests.Community.Support;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Community;
using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;

namespace L4D2PlayStats.Hub.Tests.Community;

[TestClass]
public class CommunityWebTests
{
    private const string NotOfferedMessage = "That pair is not the one waiting for your answer.";

    [TestMethod]
    public async Task TheCommunityPagesAreNotFoundWhileTheModuleIsOff()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 }, false);

        using var anonymous = site.Factory.Client();
        using var voter = site.Factory.Client(site.Voter);
        using var administrator = site.Factory.Client(HubWebFactory.Administrator);

        var home = await anonymous.GetStringAsync("/");
        var administration = await administrator.GetStringAsync("/admin");

        Assert.AreEqual(HttpStatusCode.NotFound, (await anonymous.GetAsync("/community")).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await anonymous.GetAsync(CommunityWebSite.VotePage)).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await voter.GetAsync(CommunityWebSite.VotePage)).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await administrator.GetAsync("/admin/community")).StatusCode);
        Assert.DoesNotContain("href=\"/community\"", home);
        Assert.DoesNotContain("href=\"/admin/community\"", administration);
    }

    [TestMethod]
    public async Task TheBallotHasOnlyTheFourAnswers()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);

        var page = await voter.GetStringAsync(CommunityWebSite.VotePage);
        var answers = Regex.Matches(page, @"name=""Outcome"" value=""(\w+)""").Select(match => match.Groups[1].Value).ToList();

        CollectionAssert.AreEqual(new[] { nameof(CommunityVoteOutcome.Left), nameof(CommunityVoteOutcome.Right), nameof(CommunityVoteOutcome.SameLevel), nameof(CommunityVoteOutcome.UnknownOne) }, answers);
        Assert.Contains("ArrowDown: 'SameLevel', ArrowUp: 'UnknownOne'", page);
        Assert.Contains("class=\"ballot-photo\"", page);
    }

    [TestMethod]
    public async Task ThePostsAreNotFoundWhileTheModuleIsOff()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);
        using var administrator = site.Factory.Client(HubWebFactory.Administrator);

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;
        var form = new Dictionary<string, string> { ["Left"] = pair.Left.ToString(), ["Right"] = pair.Right.ToString(), ["Outcome"] = nameof(CommunityVoteOutcome.Left) };

        var vote = await PostWhileOffAsync(site, voter, CommunityWebSite.VotePage, CommunityWebSite.VotePage, form);
        var rebuild = await PostWhileOffAsync(site, administrator, "/admin/community", "/admin/community/rebuild", new Dictionary<string, string>());

        Assert.AreEqual(HttpStatusCode.NotFound, vote.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, rebuild.StatusCode);
        Assert.IsEmpty(await site.VotesAsync());
    }

    [TestMethod]
    public async Task TheCommunityPagesOpenWhenTheModuleIsOn()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var anonymous = site.Factory.Client();
        using var voter = site.Factory.Client(site.Voter);
        using var administrator = site.Factory.Client(HubWebFactory.Administrator);

        var home = await anonymous.GetStringAsync("/");
        var administration = await administrator.GetStringAsync("/admin");

        Assert.AreEqual(HttpStatusCode.OK, (await anonymous.GetAsync("/community")).StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, (await anonymous.GetAsync(CommunityWebSite.VotePage)).StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, (await voter.GetAsync("/community")).StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, (await voter.GetAsync(CommunityWebSite.VotePage)).StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, (await administrator.GetAsync("/admin/community")).StatusCode);
        Assert.Contains("href=\"/community\"", home);
        Assert.Contains("href=\"/admin/community\"", administration);
    }

    [TestMethod]
    public async Task TheCommunityAdministrationIsOnlyForAdministrators()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var anonymous = site.Factory.Client();
        using var voter = site.Factory.Client(site.Voter);

        Assert.AreEqual(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/admin/community")).StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, (await voter.GetAsync("/admin/community")).StatusCode);
    }

    [TestMethod]
    public async Task AVisitorWhoIsNotSignedInIsAskedToSignInAndCannotVote()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var anonymous = site.Factory.Client();
        using var voter = site.Factory.Client(site.Voter);

        var page = await anonymous.GetStringAsync(CommunityWebSite.VotePage);
        var offered = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;
        var response = await CommunityWebSite.PostVoteAsync(anonymous, offered, nameof(CommunityVoteOutcome.Left));

        Assert.Contains("Sign in with Steam to vote.", page);
        Assert.Contains("href=\"/login?returnUrl=%2Fcommunity%2Fvote\"", page);
        Assert.IsNull(CommunityWebSite.Pair(page));
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsEmpty(await site.VotesAsync());
    }

    [TestMethod]
    public async Task APlayerWithTooFewMatchesIsToldSo()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 3 });

        using var voter = site.Factory.Client(site.Voter);

        var page = await voter.GetStringAsync(CommunityWebSite.VotePage);

        Assert.Contains("You need to play more matches before you can vote.", page);
        Assert.Contains("<strong>1</strong> / 3", page);
        Assert.IsNull(CommunityWebSite.Pair(page));
    }

    [TestMethod]
    public async Task TheVotePageShowsThePair()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = 7 });

        using var voter = site.Factory.Client(site.Voter);

        var page = await voter.GetStringAsync(CommunityWebSite.VotePage);
        var pair = CommunityWebSite.Pair(page)!;
        var names = await site.Factory.RunAsync(services => services.GetRequiredService<HubDbContext>().PlayerProfiles.AsNoTracking().ToDictionaryAsync(p => p.CommunityId, p => p.Name));

        Assert.AreNotEqual(site.Voter, pair.Left);
        Assert.AreNotEqual(site.Voter, pair.Right);
        Assert.Contains($"src=\"/avatar/{pair.Left}\"", page);
        Assert.Contains($"src=\"/avatar/{pair.Right}\"", page);
        Assert.Contains(HtmlEncoder.Default.Encode(names[pair.Left]!), page);
        Assert.Contains(HtmlEncoder.Default.Encode(names[pair.Right]!), page);
        Assert.Contains("<strong>7</strong> / 7", page);
        Assert.Contains("hx-post=\"/community/vote\"", page);
        Assert.Contains("action=\"/community/vote\"", page);
    }

    [TestMethod]
    public async Task AVoteWithoutJavaScriptLeadsToTheNextPair()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = 7 });

        using var voter = site.Factory.Client(site.Voter);

        var first = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;
        var response = await CommunityWebSite.PostVoteAsync(voter, first, nameof(CommunityVoteOutcome.Left));
        var page = await voter.GetStringAsync(CommunityWebSite.VotePage);
        var second = CommunityWebSite.Pair(page)!;
        var vote = (await site.VotesAsync()).Single();
        var ratings = (await site.RatingsAsync()).ToDictionary(r => r.CommunityId);

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual(CommunityWebSite.VotePage, response.Headers.Location!.OriginalString);
        Assert.AreNotEqual(first.Normalized, second.Normalized);
        Assert.Contains("<strong>6</strong> / 7", page);
        Assert.AreEqual((site.Voter, first.Left, first.Right, CommunityVoteOutcome.Left), (vote.VoterId, vote.LeftPlayerId, vote.RightPlayerId, vote.Outcome));
        Assert.AreEqual(1, ratings[first.Left].Wins);
        Assert.AreEqual(1, ratings[first.Right].Losses);
    }

    [TestMethod]
    public async Task AVoteThroughHtmxAnswersWithOnlyTheNextPair()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);

        voter.DefaultRequestHeaders.Add(CommunityWebSite.HtmxHeader, "true");

        var first = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;
        var response = await CommunityWebSite.PostVoteAsync(voter, first, nameof(CommunityVoteOutcome.SameLevel));
        var fragment = await response.Content.ReadAsStringAsync();
        var second = CommunityWebSite.Pair(fragment)!;
        var again = await CommunityWebSite.PostVoteAsync(voter, second, nameof(CommunityVoteOutcome.Right));
        var votes = await site.VotesAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("<div id=\"community-ballot\">", fragment.TrimStart());
        Assert.DoesNotContain("<html", fragment);
        Assert.DoesNotContain("navbar", fragment);
        Assert.Contains("__RequestVerificationToken", fragment);
        Assert.AreNotEqual(first.Normalized, second.Normalized);
        Assert.AreEqual(HttpStatusCode.OK, again.StatusCode);
        Assert.HasCount(2, votes);
        Assert.AreEqual(CommunityVoteOutcome.SameLevel, votes[0].Outcome);
        Assert.AreEqual(CommunityVoteOutcome.Right, votes[1].Outcome);
    }

    [TestMethod]
    public async Task AVoteForAPairThatWasNotOfferedIsRefusedOnThePage()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);

        var offered = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;
        var forged = new CommunityPair(offered.Right, offered.Left);
        var response = await CommunityWebSite.PostVoteAsync(voter, forged, nameof(CommunityVoteOutcome.Left));
        var page = await voter.GetStringAsync(CommunityWebSite.VotePage);

        voter.DefaultRequestHeaders.Add(CommunityWebSite.HtmxHeader, "true");

        var htmx = await CommunityWebSite.PostVoteAsync(voter, new CommunityPair(site.Voter, offered.Left), nameof(CommunityVoteOutcome.Left));
        var fragment = await htmx.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains(NotOfferedMessage, page);
        Assert.AreEqual(offered, CommunityWebSite.Pair(page));
        Assert.AreEqual(HttpStatusCode.OK, htmx.StatusCode);
        Assert.Contains(NotOfferedMessage, fragment);
        Assert.AreEqual(offered, CommunityWebSite.Pair(fragment));
        Assert.IsEmpty(await site.VotesAsync());
        Assert.IsEmpty(await site.RatingsAsync());
    }

    [TestMethod]
    public async Task AnAnswerThatDoesNotExistIsRefusedOnThePage()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);

        var offered = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;
        var response = await CommunityWebSite.PostVoteAsync(voter, offered, "Best");
        var page = await voter.GetStringAsync(CommunityWebSite.VotePage);

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("Choose one of the answers.", page);
        Assert.AreEqual(offered, CommunityWebSite.Pair(page));
        Assert.IsEmpty(await site.VotesAsync());
    }

    [TestMethod]
    public async Task AVoteWithoutTheAntiforgeryTokenIsRefused()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);

        var offered = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        var response = await voter.PostAsync(CommunityWebSite.VotePage, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Left"] = offered.Left.ToString(),
            ["Right"] = offered.Right.ToString(),
            ["Outcome"] = nameof(CommunityVoteOutcome.Left)
        }));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsEmpty(await site.VotesAsync());
    }

    [TestMethod]
    public async Task WhenTheVotesOfTheDayAreUsedThePageSaysSo()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = 1 });

        using var voter = site.Factory.Client(site.Voter);

        var offered = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, offered, nameof(CommunityVoteOutcome.Right));

        var page = await voter.GetStringAsync(CommunityWebSite.VotePage);

        Assert.Contains("You have used all your votes for today.", page);
        Assert.IsNull(CommunityWebSite.Pair(page));
        Assert.HasCount(1, await site.VotesAsync());
    }

    [TestMethod]
    public async Task WhenNobodyIsLeftToCompareThePageSaysSo()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumMatchesToBeCandidate = 2 });

        using var voter = site.Factory.Client(site.Voter);

        var page = await voter.GetStringAsync(CommunityWebSite.VotePage);

        Assert.Contains("There is nobody left for you to compare right now.", page);
        Assert.IsNull(CommunityWebSite.Pair(page));
    }

    [TestMethod]
    public async Task NotKnowingAPlayerThroughThePageHidesThatPlayer()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, VotesPerDay = 1000 });

        using var voter = site.Factory.Client(site.Voter);

        var first = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, first, nameof(CommunityVoteOutcome.UnknownLeft));

        var shown = new List<CommunityPair>();

        while (CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage)) is { } pair)
        {
            shown.Add(pair);

            await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.SameLevel));
        }

        Assert.HasCount(15, shown);
        Assert.IsFalse(shown.Any(pair => pair.Contains(first.Left)));
        Assert.HasCount(6, await site.RatingsAsync());
    }

    [TestMethod]
    public async Task TheRankingPageListsThePlayersAndLinksToTheirPages()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumComparisonsToBeListed = 1 });

        using var voter = site.Factory.Client(site.Voter);
        using var anonymous = site.Factory.Client();

        var empty = await anonymous.GetStringAsync("/community");
        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        var ranking = await anonymous.GetStringAsync("/community");

        Assert.Contains("Nobody has enough comparisons to be listed yet.", empty);
        Assert.Contains($"href=\"/player/{pair.Left}\"", ranking);
        Assert.Contains($"href=\"/player/{pair.Right}\"", ranking);
        Assert.Contains($"src=\"/avatar/{pair.Left}\"", ranking);
        Assert.Contains("1,516", ranking);
        Assert.Contains("1,484", ranking);
        Assert.IsLessThan(ranking.IndexOf($"/player/{pair.Right}", StringComparison.Ordinal), ranking.IndexOf($"/player/{pair.Left}", StringComparison.Ordinal));
        Assert.Contains("href=\"/community/vote\"", ranking);
    }

    [TestMethod]
    public async Task TheRankingPageDoesNotListPlayersBelowTheMinimumOfComparisons()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumComparisonsToBeListed = 2 });

        using var voter = site.Factory.Client(site.Voter);

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        var ranking = await voter.GetStringAsync("/community");

        Assert.Contains("Nobody has enough comparisons to be listed yet.", ranking);
        Assert.DoesNotContain("<table", ranking);
    }

    [TestMethod]
    public async Task WithoutTheStatisticsModuleTheRankingStillLinksToThePlayerPages()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumComparisonsToBeListed = 1 }, ranking: false);

        using var voter = site.Factory.Client(site.Voter);

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        var ranking = await voter.GetStringAsync("/community");

        Assert.Contains($"src=\"/avatar/{pair.Left}\"", ranking);
        Assert.Contains($"href=\"/player/{pair.Left}\"", ranking);
        Assert.Contains($"data-profile=\"/player/{pair.Left}\"", ranking);
    }

    [TestMethod]
    public async Task AnAdministratorChangesTheSettings()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings());

        using var administrator = site.Factory.Client(HubWebFactory.Administrator);

        var response = await HubWebFactory.PostFormAsync(administrator, "/admin/community", "/admin/community", new Dictionary<string, string>
        {
            ["MinimumMatchesToVote"] = "25",
            ["VotesPerDay"] = "0",
            ["MinimumMatchesToBeCandidate"] = "4",
            ["CandidatePlayedWithinDays"] = "90",
            ["MaximumPlayersListed"] = "-1",
            ["MinimumComparisonsToBeListed"] = "-3",
            ["KFactor"] = "999",
            ["InitialRating"] = "1200"
        });

        var page = await administrator.GetStringAsync("/admin/community");
        var settings = await site.Factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<CommunityRankingSettings>());

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("name=\"MinimumMatchesToVote\" type=\"number\" min=\"0\" value=\"25\"", page);
        Assert.AreEqual(25, settings.MinimumMatchesToVote);
        Assert.AreEqual(1, settings.VotesPerDay);
        Assert.AreEqual(4, settings.MinimumMatchesToBeCandidate);
        Assert.AreEqual(90, settings.CandidatePlayedWithinDays);
        Assert.AreEqual(0, settings.MaximumPlayersListed);
        Assert.Contains("name=\"CandidatePlayedWithinDays\" type=\"number\" min=\"0\" value=\"90\"", page);
        Assert.AreEqual(0, settings.MinimumComparisonsToBeListed);
        Assert.AreEqual(200, settings.KFactor);
        Assert.AreEqual(1200, settings.InitialRating);
    }

    [TestMethod]
    public void TheSettingsStartWithTheDefaults()
    {
        var settings = new CommunityRankingSettings();

        Assert.AreEqual(10, settings.MinimumMatchesToVote);
        Assert.AreEqual(20, settings.VotesPerDay);
        Assert.AreEqual(1, settings.MinimumMatchesToBeCandidate);
        Assert.AreEqual(0, settings.CandidatePlayedWithinDays);
        Assert.AreEqual(100, settings.MaximumPlayersListed);
        Assert.AreEqual(5, settings.MinimumComparisonsToBeListed);
        Assert.AreEqual(32, settings.KFactor);
        Assert.AreEqual(1500, settings.InitialRating);
    }

    [TestMethod]
    public async Task AnAdministratorRebuildsTheRatings()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);
        using var administrator = site.Factory.Client(HubWebFactory.Administrator);

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        var voted = CommunityTestHub.Snapshot(await site.RatingsAsync());

        await site.Factory.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();
            var rating = await dbContext.Set<CommunityRating>().FirstAsync(r => r.CommunityId == pair.Left);

            rating.Rating = 9999;
            rating.Wins = 50;

            await dbContext.SaveChangesAsync();
        });

        var response = await HubWebFactory.PostFormAsync(administrator, "/admin/community", "/admin/community/rebuild", new Dictionary<string, string>());
        var page = await administrator.GetStringAsync("/admin/community");
        var rebuilt = CommunityTestHub.Snapshot(await site.RatingsAsync());

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("Ratings rebuilt. Votes replayed: 1", page);
        Assert.AreEqual(voted, rebuilt);
    }

    [TestMethod]
    public async Task EverybodySeesWhoVotedForWhom()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);
        using var visitor = site.Factory.Client();

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        var page = await visitor.GetStringAsync("/community/votes");
        var ofPlayer = await visitor.GetStringAsync($"/community/votes?player={pair.Right}");
        var ranking = await visitor.GetStringAsync("/community");
        var ofVoter = await visitor.GetStringAsync($"/community/votes?voter={site.Voter}");
        var ofOther = await visitor.GetStringAsync($"/community/votes?voter={pair.Left}");

        Assert.Contains($"/avatar/{site.Voter}", page);
        Assert.Contains($"href=\"/community/votes?player={pair.Left}\"", page);
        Assert.Contains($"href=\"/community/votes?voter={site.Voter}\"", page);
        Assert.Contains("vf-player vf-win", page);
        Assert.Contains("vf-player vf-right vf-lose", page);
        Assert.Contains($"/avatar/{pair.Right}", ofPlayer);
        Assert.Contains("class=\"vf-row\"", ofVoter);
        Assert.DoesNotContain("class=\"vf-row\"", ofOther);
        Assert.Contains("href=\"/community/votes\"", ranking);
    }

    [TestMethod]
    public async Task TheFiltersOfTheLatestVotesSearchThePlayersOfTheVoting()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);
        using var visitor = site.Factory.Client();

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        var page = await visitor.GetStringAsync("/community/votes");
        var voters = JsonDocument.Parse(await visitor.GetStringAsync("/api/community/players?role=voter")).RootElement;
        var compared = JsonDocument.Parse(await visitor.GetStringAsync("/api/community/players")).RootElement;
        var filtered = JsonDocument.Parse(await visitor.GetStringAsync($"/api/community/players?q={pair.Left}")).RootElement;

        Assert.Contains("data-player-picker=\"#vf-voter\"", page);
        Assert.Contains("data-player-picker=\"#vf-player\"", page);
        Assert.Contains("id=\"pp-modal\"", page);
        Assert.AreEqual(1, voters.GetProperty("total").GetInt32());
        Assert.AreEqual(site.Voter.ToString(), voters.GetProperty("players")[0].GetProperty("communityId").GetString());
        Assert.AreEqual(2, compared.GetProperty("total").GetInt32());
        Assert.AreEqual(1, filtered.GetProperty("total").GetInt32());
        Assert.AreEqual(pair.Left.ToString(), filtered.GetProperty("players")[0].GetProperty("communityId").GetString());
    }

    [TestMethod]
    public async Task TheSearchOfVotersIsEmptyWhenTheVotersAreHidden()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, ShowVoterNames = false });

        using var voter = site.Factory.Client(site.Voter);
        using var visitor = site.Factory.Client();

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        var voters = JsonDocument.Parse(await visitor.GetStringAsync("/api/community/players?role=voter")).RootElement;
        var page = await visitor.GetStringAsync("/community/votes");

        Assert.AreEqual(0, voters.GetProperty("total").GetInt32());
        Assert.DoesNotContain("data-player-picker=\"#vf-voter\"", page);
    }

    [TestMethod]
    public async Task TheVotersCanBeHiddenFromTheLatestVotes()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, ShowVoterNames = false });

        using var voter = site.Factory.Client(site.Voter);
        using var visitor = site.Factory.Client();

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.SameLevel));

        var page = await visitor.GetStringAsync("/community/votes");

        Assert.DoesNotContain($"/avatar/{site.Voter}\"", page);
        Assert.Contains("A player voted", page);
        Assert.Contains("vf-player vf-same", page);
    }

    [TestMethod]
    public async Task AnAdministratorResetsTheCommunityRanking()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);
        using var administrator = site.Factory.Client(HubWebFactory.Administrator);

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        var before = await administrator.GetStringAsync("/admin/community");
        var response = await HubWebFactory.PostFormAsync(administrator, "/admin/community", "/admin/community/reset", new Dictionary<string, string>());
        var page = await administrator.GetStringAsync("/admin/community");
        var left = await site.Factory.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            return (await dbContext.Set<CommunityVote>().CountAsync(), await dbContext.Set<CommunityOffer>().CountAsync(), await dbContext.Set<CommunityRating>().CountAsync());
        });

        Assert.Contains("id=\"community-reset-modal\"", before);
        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("The community ranking was reset. Votes deleted: 1", page);
        Assert.AreEqual((0, 0, 0), left);
        Assert.IsNotNull(CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage)));
    }

    [TestMethod]
    public async Task OnlyAnAdministratorCanResetTheCommunityRanking()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        await HubWebFactory.PostFormAsync(voter, CommunityWebSite.VotePage, "/admin/community/reset", new Dictionary<string, string>());

        var votes = await site.Factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Set<CommunityVote>().CountAsync());

        Assert.AreEqual(1, votes);
    }

    [TestMethod]
    public async Task AnAdministratorRemovesTheVotesOfAVoter()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var voter = site.Factory.Client(site.Voter);
        using var administrator = site.Factory.Client(HubWebFactory.Administrator);

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        var listed = await administrator.GetStringAsync("/admin/community");
        var invalid = await HubWebFactory.PostFormAsync(administrator, "/admin/community", "/admin/community/voters/delete", new Dictionary<string, string> { ["communityId"] = "12" });
        var refused = await administrator.GetStringAsync("/admin/community");
        var kept = (await site.VotesAsync()).Count;
        var response = await HubWebFactory.PostFormAsync(administrator, "/admin/community", "/admin/community/voters/delete", new Dictionary<string, string> { ["communityId"] = site.Voter.ToString() });
        var page = await administrator.GetStringAsync("/admin/community");

        Assert.Contains($"name=\"communityId\" value=\"{site.Voter}\"", listed);
        Assert.AreEqual(HttpStatusCode.Redirect, invalid.StatusCode);
        Assert.Contains("That is not a valid SteamID64.", refused);
        Assert.AreEqual(1, kept);
        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("Votes removed: 1", page);
        Assert.Contains("Nobody has voted yet.", page);
        Assert.IsEmpty(await site.VotesAsync());
        Assert.IsEmpty(await site.RatingsAsync());
    }

    private static async Task<HttpResponseMessage> PostWhileOffAsync(CommunityWebSite site, HttpClient client, string formPage, string action, Dictionary<string, string> fields)
    {
        var html = await client.GetStringAsync(formPage);
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
        var form = new Dictionary<string, string>(fields) { ["__RequestVerificationToken"] = token };

        await site.Factory.SaveAsync(new ModuleSettings { CommunityRanking = false });

        var response = await client.PostAsync(action, new FormUrlEncodedContent(form));

        await site.Factory.SaveAsync(new ModuleSettings { CommunityRanking = true });

        return response;
    }
}
