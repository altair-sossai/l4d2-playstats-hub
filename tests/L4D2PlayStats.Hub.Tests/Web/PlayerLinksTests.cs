using System.Net;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Web.Live;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class PlayerLinksTests
{
    private const string Alice = "76561198000000001";
    private const string Bob = "76561198000000002";
    private const string Carol = "76561198000000003";

    [TestMethod]
    public async Task TheLivePageLinksThePlayersToTheSitePageEvenWithoutTheRanking()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = false, LiveServer = true });

        var credentials = await factory.CreateServerAsync();

        await factory.RunAsync(services =>
        {
            var live = services.GetRequiredService<ILiveDataService>();
            var alice = new LivePlayer { CommunityId = Alice, Name = "AliceTheSurvivor" };
            var bob = new LivePlayer { CommunityId = Bob, Name = "BobTheHunter" };

            live.UpdatePlayers(credentials.Server, new PlayersCommand
            {
                Survivors = [new LiveSurvivor { CommunityId = Alice, Name = "AliceTheSurvivor", IsPlayerAlive = true }],
                Infecteds = [new LiveInfected { CommunityId = Bob, Name = "BobTheHunter", IsPlayerAlive = true, Health = 250, MaxHealth = 250 }],
                Spectators = [new LivePlayer { CommunityId = Carol, Name = "CarolTheSpectator" }]
            });
            live.AddEvent(credentials.Server, new SkeetEvent { Actor = alice, Hunter = bob });
            live.AddMessage(credentials.Server, new ChatMessageCommand { CommunityId = Bob, Name = "BobTheHunter", Message = "nice skeet" });

            return Task.CompletedTask;
        });

        factory.PassTheDelay();

        var players = await client.GetStringAsync("/servers/test-server/players");
        var feed = await client.GetStringAsync("/servers/test-server/feed");

        foreach (var communityId in new[] { Alice, Bob, Carol })
            Assert.Contains($"href=\"/player/{communityId}\"", players, communityId);

        Assert.Contains($"href=\"/player/{Alice}\"", feed);
        Assert.Contains($"href=\"/player/{Bob}\"", feed);
        Assert.Contains("nice skeet", feed);
        Assert.DoesNotContain("href=\"https://steamcommunity.com/profiles", players);
        Assert.DoesNotContain("href=\"https://steamcommunity.com/profiles", feed);
    }

    [TestMethod]
    public async Task TheAdministrationLinksThePlayersToTheSitePage()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = false, LiveServer = true, SiteChat = true });

        var administrators = await client.GetStringAsync("/admin/administrators");
        var chatBans = await client.GetStringAsync("/admin/chat-bans");

        Assert.Contains($"href=\"/player/{HubWebFactory.Administrator}\"", administrators);
        Assert.DoesNotContain($"href=\"https://steamcommunity.com/profiles/{HubWebFactory.Administrator}\"", administrators);
        Assert.Contains("var playerUrl = \"/player/\";", chatBans);
    }

    [TestMethod]
    public async Task TheSignedInPlayerReachesTheirOwnPageEvenWithoutTheRanking()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Player);

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = false, LiveServer = true });

        var home = await client.GetStringAsync("/servers");
        var page = await client.GetAsync($"/player/{HubWebFactory.Player}");

        Assert.Contains($"href=\"/player/{HubWebFactory.Player}\"", home);
        Assert.AreEqual(HttpStatusCode.OK, page.StatusCode);
    }
}
