using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Championships;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Championships;

[TestClass]
public class ChampionshipCaptainTests
{
    private const long FirstPlayer = 76561198300000000;
    private const long Stranger = 76561198399999999;
    private static readonly byte[] Png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

    [TestMethod]
    public async Task TheCaptainsPickTheirPlayersInTheDraftWhenItIsTheirTurn()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();

        var (championship, teamIds) = await SetupAsync(factory, ChampionshipDraftMethod.Snake, false);

        await factory.RunAsync(services => services.GetRequiredService<IChampionshipDraftService>().DrawOrderAsync(championship.Id, HubWebFactory.Administrator));

        var loaded = await ChampionshipWeb.LoadAsync(factory, championship.Id);
        var first = ChampionshipDraftService.OnTheClock(loaded)!;
        var second = loaded.Teams.Single(team => team.Id != first.Id);
        var area = $"/championships/{championship.Slug}/captain";

        using var firstCaptain = factory.Client(first.CaptainCommunityId);
        using var secondCaptain = factory.Client(second.CaptainCommunityId);
        using var admin = factory.Client(HubWebFactory.Administrator);

        var onTheClock = await firstCaptain.GetStringAsync($"{area}?tab=draft");
        var waiting = await secondCaptain.GetStringAsync($"{area}?tab=draft");

        Assert.Contains("It is the turn of your team.", onTheClock);
        Assert.Contains($"action=\"{area}/draft/pick\"", onTheClock);
        Assert.Contains($"Now it is the turn of {first.Name}.", waiting);
        Assert.DoesNotContain($"action=\"{area}/draft/pick\"", waiting);
        Assert.Contains("Things to do: 1", await firstCaptain.GetStringAsync($"/championships/{championship.Slug}"));
        Assert.Contains("It is the turn of your team in the draft", onTheClock);

        var free = ChampionshipDraftService.Free(loaded);
        var outOfTurn = await HubWebFactory.PostFormAsync(secondCaptain, area, $"{area}/draft/pick", new Dictionary<string, string> { ["communityId"] = free[0].ToString() });

        Assert.AreEqual($"{area}?tab=draft", outOfTurn.Headers.Location!.OriginalString);
        Assert.IsEmpty((await ChampionshipWeb.LoadAsync(factory, championship.Id)).DraftPicks);
        Assert.Contains("It is not the turn of your team.", await secondCaptain.GetStringAsync($"{area}?tab=draft"));

        await HubWebFactory.PostFormAsync(firstCaptain, area, $"{area}/draft/pick", new Dictionary<string, string> { ["communityId"] = free[0].ToString() });

        loaded = await ChampionshipWeb.LoadAsync(factory, championship.Id);

        Assert.HasCount(1, loaded.DraftPicks);
        Assert.IsTrue(loaded.Teams.Single(team => team.Id == first.Id).Members.Any(member => member.CommunityId == free[0]));
        Assert.AreEqual(second.Id, ChampionshipDraftService.OnTheClock(loaded)!.Id);
        Assert.Contains("It is the turn of your team.", await secondCaptain.GetStringAsync($"{area}?tab=draft"));

        await HubWebFactory.PostFormAsync(admin, $"/admin/championships/{championship.Id}?tab=draft", $"/admin/championships/{championship.Id}/draft/pick", new Dictionary<string, string> { ["communityId"] = free[1].ToString() });

        loaded = await ChampionshipWeb.LoadAsync(factory, championship.Id);

        Assert.HasCount(2, loaded.DraftPicks);
        Assert.AreEqual(second.Id, loaded.DraftPicks.Single(pick => pick.Number == 2).TeamId);
        Assert.HasCount(2, teamIds);
    }

    [TestMethod]
    public async Task APendingFlagIsNotServedByTheAddressOfTheChampionshipImages()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();

        var (championship, teamIds) = await SetupAsync(factory, ChampionshipDraftMethod.Manual, true);
        var area = $"/championships/{championship.Slug}/captain";

        using var captain = factory.Client(FirstPlayer);
        using var stranger = factory.Client(Stranger);
        using var visitor = factory.Client();

        await PostMultipartAsync(captain, area, $"{area}/identity", new Dictionary<string, string> { ["name"] = "Os Lobos", ["tag"] = "LOB" }, Png, "flag.png");

        var team = await TeamAsync(factory, teamIds[0]);
        var address = $"/championships/images/{championship.Id}/{team.PendingFlag}";

        Assert.IsNotNull(team.PendingFlag);
        Assert.AreEqual(HttpStatusCode.NotFound, (await visitor.GetAsync(address)).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await stranger.GetAsync(address)).StatusCode);
    }

    [TestMethod]
    public async Task AFlagLargerThanTheLimitIsRefusedBeforeItIsSaved()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();

        var (championship, teamIds) = await SetupAsync(factory, ChampionshipDraftMethod.Manual, true);
        var area = $"/championships/{championship.Slug}/captain";

        using var captain = factory.Client(FirstPlayer);

        var huge = new byte[5 * 1024 * 1024];

        Png.CopyTo(huge, 0);

        var response = await PostMultipartAsync(captain, area, $"{area}/identity", new Dictionary<string, string> { ["name"] = "Os Lobos", ["tag"] = "LOB" }, huge, "flag.png");
        var team = await TeamAsync(factory, teamIds[0]);

        Assert.IsTrue(response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.RequestEntityTooLarge, $"A flag of 5 MB answered {response.StatusCode}");
        Assert.IsNull(team.PendingFlag);
        Assert.IsNull(team.PendingName);
    }

    [TestMethod]
    public async Task TheNameTagAndFlagOfATeamWaitForTheApprovalOfTheAdministration()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();

        var (championship, teamIds) = await SetupAsync(factory, ChampionshipDraftMethod.Manual, true);
        var teamId = teamIds[0];
        var area = $"/championships/{championship.Slug}/captain";
        var adminUrl = $"/admin/championships/{championship.Id}";

        using var captain = factory.Client(FirstPlayer);
        using var admin = factory.Client(HubWebFactory.Administrator);
        using var stranger = factory.Client(Stranger);
        using var visitor = factory.Client();

        var sent = await PostMultipartAsync(captain, area, $"{area}/identity", new Dictionary<string, string> { ["name"] = "Os Lobos", ["tag"] = "LOB" }, Png, "flag.png");

        Assert.AreEqual($"{area}?tab=team", sent.Headers.Location!.OriginalString);

        var team = await TeamAsync(factory, teamId);

        Assert.AreEqual("Team A", team.Name);
        Assert.AreEqual("Os Lobos", team.PendingName);
        Assert.AreEqual("LOB", team.PendingTag);
        Assert.IsNotNull(team.PendingFlag);
        Assert.IsNull(team.Flag);
        Assert.Contains("Waiting for the approval of the administration of the site.", await captain.GetStringAsync(area));
        Assert.DoesNotContain("Os Lobos", await visitor.GetStringAsync($"/championships/{championship.Slug}/teams"));
        Assert.AreEqual(HttpStatusCode.NotFound, (await stranger.GetAsync($"/championship-flags/{teamId}/pending")).StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, (await visitor.GetAsync($"/championship-flags/{teamId}")).StatusCode);

        var pending = await captain.GetAsync($"/championship-flags/{teamId}/pending");

        Assert.AreEqual(HttpStatusCode.OK, pending.StatusCode);
        Assert.AreEqual("image/png", pending.Content.Headers.ContentType!.MediaType);
        Assert.AreEqual(HttpStatusCode.OK, (await admin.GetAsync($"/championship-flags/{teamId}/pending")).StatusCode);

        var teamsTab = await admin.GetStringAsync($"{adminUrl}?tab=teams");

        Assert.Contains($"data-target=\"#team-review-modal-{teamId}\"", teamsTab);
        Assert.Contains("Waiting for approval", teamsTab);
        Assert.Contains("Os Lobos", teamsTab);

        var approved = await HubWebFactory.PostFormAsync(admin, $"{adminUrl}?tab=teams", $"{adminUrl}/teams/{teamId}/identity/approve", []);

        Assert.AreEqual($"{adminUrl}?tab=teams", approved.Headers.Location!.OriginalString);

        team = await TeamAsync(factory, teamId);

        Assert.AreEqual("Os Lobos", team.Name);
        Assert.AreEqual("LOB", team.Tag);
        Assert.IsNotNull(team.Flag);
        Assert.IsFalse(team.HasPendingIdentity);
        Assert.AreEqual(HttpStatusCode.OK, (await visitor.GetAsync($"/championship-flags/{teamId}")).StatusCode);

        var teams = await visitor.GetStringAsync($"/championships/{championship.Slug}/teams");

        Assert.Contains("Os Lobos", teams);
        Assert.Contains($"/championship-flags/{teamId}?v=", teams);

        await PostMultipartAsync(captain, area, $"{area}/identity", new Dictionary<string, string> { ["name"] = "Os Lobos", ["tag"] = "LOB" }, Encoding.UTF8.GetBytes("not an image"), "flag.png");

        Assert.IsFalse((await TeamAsync(factory, teamId)).HasPendingIdentity);
        Assert.Contains("The flag must be a PNG, JPG, WEBP or GIF image.", await captain.GetStringAsync(area));

        await PostMultipartAsync(captain, area, $"{area}/identity", new Dictionary<string, string> { ["name"] = "Team B", ["tag"] = "" }, null, null);

        Assert.Contains("There is already a team with this name.", await captain.GetStringAsync(area));

        await PostMultipartAsync(captain, area, $"{area}/identity", new Dictionary<string, string> { ["name"] = "Lobos Maus", ["tag"] = "LOB" }, null, null);

        Assert.AreEqual("Lobos Maus", (await TeamAsync(factory, teamId)).PendingName);

        await HubWebFactory.PostFormAsync(admin, $"{adminUrl}?tab=teams", $"{adminUrl}/teams/{teamId}/identity/refuse", new Dictionary<string, string> { ["reason"] = "Choose a friendlier name" });

        team = await TeamAsync(factory, teamId);

        Assert.AreEqual("Os Lobos", team.Name);
        Assert.IsFalse(team.HasPendingIdentity);
        Assert.AreEqual("Choose a friendlier name", team.RejectionReason);

        var refused = await captain.GetStringAsync(area);

        Assert.Contains("The administration of the site refused the last change of the team.", refused);
        Assert.Contains("Choose a friendlier name", refused);
        Assert.Contains("The administration refused the new name, tag or flag", refused);

        await HubWebFactory.PostFormAsync(captain, area, $"{area}/flag/remove", []);

        Assert.IsNull((await TeamAsync(factory, teamId)).Flag);
        Assert.AreEqual(HttpStatusCode.NotFound, (await visitor.GetAsync($"/championship-flags/{teamId}")).StatusCode);

        var updated = await PostMultipartAsync(admin, $"{adminUrl}?tab=teams", $"{adminUrl}/teams/{teamId}", new Dictionary<string, string> { ["name"] = "Os Lobos", ["tag"] = "LOB", ["seed"] = "1", ["captain"] = FirstPlayer.ToString(), ["viceCaptain"] = "" }, Png, "flag.png");

        Assert.AreEqual(HttpStatusCode.Redirect, updated.StatusCode);
        Assert.IsNotNull((await TeamAsync(factory, teamId)).Flag);
        Assert.AreEqual(HttpStatusCode.OK, (await visitor.GetAsync($"/championship-flags/{teamId}")).StatusCode);
        Assert.MatchesRegex($"<img src=\"/championship-flags/{teamId}[^\"]*\" alt=\"\" class=\"championship-team-badge\"", await visitor.GetStringAsync($"/championships/{championship.Slug}"));

        var stageId = (await ChampionshipWeb.LoadAsync(factory, championship.Id)).Stages.Single().Id;

        await factory.RunAsync(services => services.GetRequiredService<IChampionshipStageService>().GenerateAsync(stageId, HubWebFactory.Administrator));

        var series = (await ChampionshipWeb.LoadAsync(factory, championship.Id)).Stages.Single().Series.Single();
        var flag = $"/championship-flags/{teamId}?v=";

        Assert.Contains(flag, await visitor.GetStringAsync($"/championships/{championship.Slug}/series/{series.Id}"));
        Assert.Contains(flag, await visitor.GetStringAsync($"/championships/{championship.Slug}/statistics"));
        Assert.IsGreaterThan(1, Regex.Count(await captain.GetStringAsync("/championships/" + championship.Slug), Regex.Escape(flag)));
        Assert.Contains(flag, await admin.GetStringAsync($"{adminUrl}?tab=participants"));

        await factory.RunAsync(async services =>
        {
            var game = series.Games.Single();

            await services.GetRequiredService<IChampionshipSeriesService>().SetManualResultAsync(game.Id, series.TeamAId == teamId ? 600 : 300, series.TeamAId == teamId ? 300 : 600, null, HubWebFactory.Administrator);
            await services.GetRequiredService<IChampionshipService>().SetStatusAsync(championship.Id, ChampionshipStatus.Finished, null, HubWebFactory.Administrator);
        });

        Assert.Contains(flag, await visitor.GetStringAsync("/championships"));
        Assert.Contains(flag, await admin.GetStringAsync("/admin/championships"));
    }

    [TestMethod]
    public async Task OnlyTheCaptainChoosesTheViceCaptain()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();

        var (championship, teamIds) = await SetupAsync(factory, ChampionshipDraftMethod.Manual, true);
        var area = $"/championships/{championship.Slug}/captain";

        using var captain = factory.Client(FirstPlayer);
        using var vice = factory.Client(FirstPlayer + 1);
        using var stranger = factory.Client(Stranger);

        var page = await captain.GetStringAsync(area);

        Assert.Contains("Make vice-captain", page);
        Assert.Contains("Remove as vice-captain", page);
        Assert.DoesNotContain("Make vice-captain", await vice.GetStringAsync(area));

        await HubWebFactory.PostFormAsync(captain, area, $"{area}/vice", new Dictionary<string, string> { ["communityId"] = (FirstPlayer + 2).ToString() });

        Assert.AreEqual(FirstPlayer + 2, (await TeamAsync(factory, teamIds[0])).ViceCaptainCommunityId);

        using var newVice = factory.Client(FirstPlayer + 2);

        await HubWebFactory.PostFormAsync(newVice, area, $"{area}/vice", new Dictionary<string, string> { ["communityId"] = (FirstPlayer + 3).ToString() });

        Assert.AreEqual(FirstPlayer + 2, (await TeamAsync(factory, teamIds[0])).ViceCaptainCommunityId);
        Assert.Contains("Only the captain chooses the vice-captain.", await newVice.GetStringAsync(area));
        Assert.AreEqual(HttpStatusCode.NotFound, (await vice.GetAsync(area)).StatusCode);

        await HubWebFactory.PostFormAsync(captain, area, $"{area}/vice", new Dictionary<string, string> { ["communityId"] = (FirstPlayer + 4).ToString() });

        Assert.AreEqual(FirstPlayer + 2, (await TeamAsync(factory, teamIds[0])).ViceCaptainCommunityId);
        Assert.Contains("Choose a player of your team.", await captain.GetStringAsync(area));
        Assert.AreEqual(HttpStatusCode.NotFound, (await stranger.GetAsync(area)).StatusCode);

        await HubWebFactory.PostFormAsync(captain, area, $"{area}/vice", []);

        Assert.IsNull((await TeamAsync(factory, teamIds[0])).ViceCaptainCommunityId);
    }

    private static Task<(Championship Championship, List<int> Teams)> SetupAsync(HubWebFactory factory, ChampionshipDraftMethod method, bool members)
    {
        return factory.RunAsync(async services =>
        {
            var championships = services.GetRequiredService<IChampionshipService>();
            var teamService = services.GetRequiredService<IChampionshipTeamService>();
            var championship = await championships.CreateAsync("Copa Capitães", [new ChampionshipStageTemplate("Final", ChampionshipStageType.Knockout, 1, 0, 1, ChampionshipCampaignRule.Manual, ChampionshipSeeding.Seeds)], HubWebFactory.Administrator);

            await championships.UpdateAsync(championship.Id, new ChampionshipSettingsForm { Name = "Copa Capitães", PlayersPerGame = 4, RosterSize = 4, DraftMethod = method }, null);
            await championships.SetStatusAsync(championship.Id, ChampionshipStatus.TeamFormation, null, null);
            await teamService.AddParticipantsAsync(championship.Id, Enumerable.Range(0, 8).Select(index => FirstPlayer + index).ToList(), null);

            var teamA = await teamService.CreateTeamAsync(championship.Id, "Team A", null, null);
            var teamB = await teamService.CreateTeamAsync(championship.Id, "Team B", null, null);

            if (members)
            {
                for (var index = 0; index < 4; index++)
                {
                    await teamService.AddMemberAsync(teamA.Id, FirstPlayer + index, null);
                    await teamService.AddMemberAsync(teamB.Id, FirstPlayer + 4 + index, null);
                }

                await teamService.SetLeadersAsync(teamA.Id, FirstPlayer, FirstPlayer + 1, null);
                await teamService.SetLeadersAsync(teamB.Id, FirstPlayer + 4, FirstPlayer + 5, null);
            }
            else
            {
                await teamService.SetLeadersAsync(teamA.Id, FirstPlayer, null, null);
                await teamService.SetLeadersAsync(teamB.Id, FirstPlayer + 4, null, null);
            }

            return (championship, new List<int> { teamA.Id, teamB.Id });
        });
    }

    private static Task<ChampionshipTeam> TeamAsync(HubWebFactory factory, int teamId)
    {
        return factory.RunAsync(services => services.GetRequiredService<HubDbContext>().ChampionshipTeams.AsNoTracking().SingleAsync(team => team.Id == teamId));
    }

    private static Task<HttpResponseMessage> PostMultipartAsync(HttpClient client, string formPage, string action, Dictionary<string, string> fields, byte[]? file, string? fileName)
    {
        return HubWebFactory.PostMultipartAsync(client, formPage, action, fields.Select(field => (field.Key, field.Value)), file == null ? [] : [("flag", fileName!, file)]);
    }
}
