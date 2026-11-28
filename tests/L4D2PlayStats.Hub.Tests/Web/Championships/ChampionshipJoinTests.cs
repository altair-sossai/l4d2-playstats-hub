using System.Net;
using System.Text.Encodings.Web;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Championships;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Championships;

[TestClass]
public class ChampionshipJoinTests
{
    private const long Other = 76561198000000300;

    [TestMethod]
    public async Task APlayerAsksToJoinAndTheAdministrationApprovesTheRequest()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();
        using var admin = factory.Client(HubWebFactory.Administrator);
        using var player = factory.Client(HubWebFactory.Player);
        using var visitor = factory.Client();

        var (id, url) = await CreateAsync(factory, admin, "Copa Aberta", true);
        var page = "/championships/copa-aberta";
        var open = await player.GetStringAsync(page);

        Assert.Contains("id=\"join\"", open);
        Assert.Contains($"action=\"{page}/join\"", open);
        Assert.Contains("class=\"btn btn-warning championship-hero-cta\" href=\"#join\"", open);
        Assert.Contains("/login?returnUrl=%2Fchampionships%2Fcopa-aberta%23join", await visitor.GetStringAsync(page));

        var asked = await HubWebFactory.PostFormAsync(player, page, $"{page}/join", new Dictionary<string, string> { ["message"] = "I play every night." });

        Assert.AreEqual(HttpStatusCode.Redirect, asked.StatusCode);
        Assert.AreEqual($"{page}#join", asked.Headers.Location!.OriginalString);

        var waiting = await player.GetStringAsync(page);
        var participants = await admin.GetStringAsync($"{url}?tab=participants");

        Assert.Contains("Your request to join is waiting for the administration.", waiting);
        Assert.Contains($"action=\"{page}/join/cancel\"", waiting);
        Assert.DoesNotContain("championship-hero-cta", waiting);
        Assert.Contains("Requests to join", participants);
        Assert.Contains("I play every night.", participants);

        var request = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().ChampionshipJoinRequests.AsNoTracking().SingleAsync());
        var approved = await HubWebFactory.PostFormAsync(admin, url, $"{url}/requests/{request.Id}/approve", []);
        var championship = await factory.RunAsync(services => services.GetRequiredService<IChampionshipLoadService>().LoadAsync(id, false));
        var joined = await player.GetStringAsync(page);
        var history = await admin.GetStringAsync($"{url}?tab=history");

        Assert.AreEqual(HttpStatusCode.Redirect, approved.StatusCode);
        Assert.IsTrue(championship!.Participants.Any(participant => participant.CommunityId == HubWebFactory.Player));
        Assert.Contains("You are a participant of this championship.", joined);
        Assert.Contains("asked to join", history);
        Assert.Contains("Approved the request of", history);
        Assert.DoesNotContain("I play every night.", await admin.GetStringAsync($"{url}?tab=participants"));
    }

    [TestMethod]
    public async Task ARefusedPlayerSeesItAndCannotAskAgainButTheAdministrationCanStillApprove()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();
        using var admin = factory.Client(HubWebFactory.Administrator);
        using var player = factory.Client(HubWebFactory.Player);

        var (id, url) = await CreateAsync(factory, admin, "Copa Recusada", true);
        var page = "/championships/copa-recusada";

        await HubWebFactory.PostFormAsync(player, page, $"{page}/join", []);

        var request = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().ChampionshipJoinRequests.AsNoTracking().SingleAsync());

        await HubWebFactory.PostFormAsync(admin, url, $"{url}/requests/{request.Id}/refuse", []);

        Assert.Contains(HtmlEncoder.Default.Encode("The administration refused your request to join this championship."), await player.GetStringAsync(page));
        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => factory.RunAsync(services => services.GetRequiredService<IChampionshipJoinService>().AskAsync(id, HubWebFactory.Player, null)));
        Assert.Contains("Refused the request of", await admin.GetStringAsync($"{url}?tab=history"));

        await HubWebFactory.PostFormAsync(admin, url, $"{url}/requests/{request.Id}/approve", []);

        var championship = await factory.RunAsync(services => services.GetRequiredService<IChampionshipLoadService>().LoadAsync(id, false));

        Assert.IsTrue(championship!.Participants.Any(participant => participant.CommunityId == HubWebFactory.Player));
    }

    [TestMethod]
    public async Task APlayerCancelsAPendingRequestAndCanAskAgain()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();
        using var admin = factory.Client(HubWebFactory.Administrator);
        using var player = factory.Client(HubWebFactory.Player);

        await CreateAsync(factory, admin, "Copa Cancela", true);

        var page = "/championships/copa-cancela";

        await HubWebFactory.PostFormAsync(player, page, $"{page}/join", []);

        var cancelled = await HubWebFactory.PostFormAsync(player, page, $"{page}/join/cancel", []);

        Assert.AreEqual(HttpStatusCode.Redirect, cancelled.StatusCode);
        Assert.AreEqual(0, await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().ChampionshipJoinRequests.CountAsync()));
        Assert.Contains($"action=\"{page}/join\"", await player.GetStringAsync(page));

        await HubWebFactory.PostFormAsync(player, page, $"{page}/join", []);

        Assert.AreEqual(1, await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().ChampionshipJoinRequests.CountAsync()));
    }

    [TestMethod]
    public async Task RequestsAreClosedWhenTheSettingIsOffTheStatusIsNotParticipantsOrThePlacesAreTaken()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();
        using var admin = factory.Client(HubWebFactory.Administrator);
        using var player = factory.Client(HubWebFactory.Player);

        var (closed, _) = await CreateAsync(factory, admin, "Copa Fechada", false);

        Assert.DoesNotContain("id=\"join\"", await player.GetStringAsync("/championships/copa-fechada"));
        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => factory.RunAsync(services => services.GetRequiredService<IChampionshipJoinService>().AskAsync(closed, HubWebFactory.Player, null)));

        var (running, _) = await CreateAsync(factory, admin, "Copa Andamento", true);

        await factory.RunAsync(services => services.GetRequiredService<IChampionshipService>().SetStatusAsync(running, ChampionshipStatus.TeamFormation, null, null));
        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => factory.RunAsync(services => services.GetRequiredService<IChampionshipJoinService>().AskAsync(running, HubWebFactory.Player, null)));

        var (full, _) = await CreateAsync(factory, admin, "Copa Cheia", true, 1);

        await factory.RunAsync(services => services.GetRequiredService<IChampionshipJoinService>().AskAsync(full, Other, null));
        await factory.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().AddParticipantsAsync(full, [HubWebFactory.Administrator], null));

        var page = await player.GetStringAsync("/championships/copa-cheia");
        var request = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().ChampionshipJoinRequests.AsNoTracking().SingleAsync(item => item.ChampionshipId == full));

        Assert.Contains("There are no places left in this championship.", page);
        Assert.DoesNotContain("championship-hero-cta", page);
        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => factory.RunAsync(services => services.GetRequiredService<IChampionshipJoinService>().AskAsync(full, HubWebFactory.Player, null)));
        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => factory.RunAsync(services => services.GetRequiredService<IChampionshipJoinService>().ApproveAsync(request.Id, null)));
    }

    [TestMethod]
    public async Task DeletingTheChampionshipDeletesItsRequests()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();
        using var admin = factory.Client(HubWebFactory.Administrator);

        var (id, _) = await CreateAsync(factory, admin, "Copa Some", true);

        await factory.RunAsync(services => services.GetRequiredService<IChampionshipJoinService>().AskAsync(id, Other, "hi"));
        await HubWebFactory.PostFormAsync(admin, "/admin/championships", $"/admin/championships/{id}/delete", []);

        Assert.AreEqual(0, await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().ChampionshipJoinRequests.CountAsync()));
    }

    private static async Task<(int Id, string Url)> CreateAsync(HubWebFactory factory, HttpClient admin, string name, bool requests, int? places = null)
    {
        var created = await HubWebFactory.PostFormAsync(admin, "/admin/championships", "/admin/championships", new Dictionary<string, string> { ["name"] = name, ["template"] = "Knockout" });
        var url = created.Headers.Location!.OriginalString;
        var id = int.Parse(url.Split('/').Last());

        await factory.RunAsync(async services =>
        {
            var championships = services.GetRequiredService<IChampionshipService>();

            await championships.UpdateAsync(id, new ChampionshipSettingsForm { Name = name, PlayersPerGame = 2, RosterSize = 2, JoinRequests = requests, MaxParticipants = places }, null);
            await championships.SetStatusAsync(id, ChampionshipStatus.Participants, null, null);
        });

        return (id, url);
    }
}
