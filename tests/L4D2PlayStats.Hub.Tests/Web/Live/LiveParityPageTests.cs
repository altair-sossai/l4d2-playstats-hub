using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Live;

[TestClass]
public class LiveParityPageTests
{
    private const string Alice = "76561198000000001";
    private const string Bob = "76561198000000002";

    [TestMethod]
    public async Task TheRoutesOfTheOldSiteLeadToTheNewOnes()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();
        await factory.CreateServerAsync();

        var server = await client.GetAsync("/server");
        var header = await client.GetAsync("/server/header");
        var players = await client.GetAsync("/server/players");
        var feed = await client.GetAsync("/server/feed?after=3");
        var other = await client.GetAsync("/server/anything");
        var chatBans = await client.GetAsync("/chat-bans?q=alice");
        var gameInfo = await client.GetAsync("/api/game-info");
        var scoreboard = await client.GetAsync("/api/game-info/scoreboard");
        var messages = await client.GetAsync("/api/game-info/messages?after=5");

        Assert.AreEqual(HttpStatusCode.MovedPermanently, server.StatusCode);
        Assert.AreEqual("/servers", server.Headers.Location!.OriginalString);
        Assert.AreEqual(HttpStatusCode.Redirect, header.StatusCode);
        Assert.AreEqual("/servers/test-server/header", header.Headers.Location!.OriginalString);
        Assert.AreEqual("/servers/test-server/players", players.Headers.Location!.OriginalString);
        Assert.AreEqual("/servers/test-server/feed?after=3", feed.Headers.Location!.OriginalString);
        Assert.AreEqual(HttpStatusCode.NotFound, other.StatusCode);
        Assert.AreEqual(HttpStatusCode.MovedPermanently, chatBans.StatusCode);
        Assert.AreEqual("/admin/chat-bans?q=alice", chatBans.Headers.Location!.OriginalString);
        Assert.AreEqual(HttpStatusCode.Redirect, gameInfo.StatusCode);
        Assert.AreEqual("/api/servers/test-server/game-info", gameInfo.Headers.Location!.OriginalString);
        Assert.AreEqual("/api/servers/test-server/game-info/scoreboard", scoreboard.Headers.Location!.OriginalString);
        Assert.AreEqual("/api/servers/test-server/game-info/messages?after=5", messages.Headers.Location!.OriginalString);
    }

    [TestMethod]
    public async Task WithoutAServerTheOldRoutesAreNotFound()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var header = await client.GetAsync("/server/header");
        var gameInfo = await client.GetAsync("/api/game-info");
        var list = await client.GetStringAsync("/servers");

        Assert.AreEqual(HttpStatusCode.NotFound, header.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, gameInfo.StatusCode);
        Assert.Contains("No game server available yet.", list);
    }

    [TestMethod]
    public async Task ThePickerRefreshesItselfWithAPartial()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.CreateServerAsync("First");
        await factory.RunAsync(services => services.GetRequiredService<IGameServerService>().CreateAsync("Second", "play.example.com:27015"));

        var page = await client.WithScriptsAsync("/servers");
        var cards = await client.GetStringAsync("/servers?cards=true");

        Assert.Contains("<html", page);
        Assert.Contains("id=\"server-cards\"", page);
        Assert.Contains("+ '?cards=true'", page);
        Assert.DoesNotContain("<html", cards);
        Assert.Contains("id=\"server-card-first\"", cards);
        Assert.Contains("id=\"server-card-second\"", cards);
        Assert.Contains("connect play.example.com:27015", cards);
        Assert.DoesNotContain("htmx", page);
        Assert.DoesNotContain("bootstrap.bundle", page);
    }

    [TestMethod]
    public async Task TheLivePageShowsEveryIconAndLabelOfThePlayers()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();

        await factory.RunAsync(services =>
        {
            var live = services.GetRequiredService<ILiveDataService>();

            live.UpdateScoreboard(credentials.Server, new LiveScoreboard { SurvivorScore = 812, InfectedScore = 640 });
            live.UpdatePlayers(credentials.Server, new PlayersCommand
            {
                Survivors =
                [
                    new LiveSurvivor
                    {
                        CommunityId = Alice, Name = "AliceTheSurvivor", IsAdmin = true, IsPlayerAlive = true, Character = SurvivorCharacter.Zoey, Latency = 0.04m,
                        PermanentHealth = 30, TemporaryHealth = 20, Progress = 0.5m, BlackAndWhite = true,
                        PrimaryWeapon = Weapon.Smg, SecondaryWeapon = Weapon.Melee, MeleeWeapon = MeleeWeapon.Katana, SlotNumber3 = Weapon.Molotov, SlotNumber4 = Weapon.FirstAidKit, SlotNumber5 = Weapon.PainPills
                    },
                    new LiveSurvivor { CommunityId = Bob, Name = "BobTheDead", IsPlayerAlive = false, Character = SurvivorCharacter.Nick, Latency = 0.25m, PrimaryWeapon = Weapon.Rifle }
                ],
                Infecteds =
                [
                    new LiveInfected { CommunityId = "76561198000000003", Name = "CarolTheHunter", IsPlayerAlive = true, Type = InfectedType.Hunter, Health = 125, MaxHealth = 250, Damage = 77, Latency = 0.15m },
                    new LiveInfected { CommunityId = "76561198000000004", Name = "DaveTheGhost", IsInfectedGhost = true }
                ],
                Spectators = [new LivePlayer { CommunityId = "76561198000000005", Name = "ErinTheSpectator" }]
            });

            return Task.CompletedTask;
        });

        factory.PassTheDelay();

        var players = await client.GetStringAsync("/servers/test-server/players");

        Assert.Contains("/imgs/survivor-character/zoey.png", players);
        Assert.Contains("/imgs/survivor-character/nick.png", players);
        Assert.Contains("/imgs/weapon/weapon_smg-dark.png", players);
        Assert.Contains("/imgs/melee-weapon/katana-dark.png", players);
        Assert.Contains("/imgs/weapon/weapon_molotov-dark.png", players);
        Assert.Contains("/imgs/weapon/weapon_first_aid_kit-dark.png", players);
        Assert.Contains("/imgs/weapon/weapon_pain_pills-dark.png", players);
        Assert.DoesNotContain("/imgs/weapon/weapon_melee-dark.png", players);
        Assert.DoesNotContain("/imgs/weapon/weapon_rifle-dark.png", players);
        Assert.Contains("/imgs/latency/low.png", players);
        Assert.Contains("/imgs/latency/medium.png", players);
        Assert.Contains("/imgs/latency/high.png", players);
        Assert.Contains("/imgs/infected-type/hunter.jpg", players);
        Assert.Contains("/imgs/infected-type/none.jpg", players);
        Assert.Contains("progress-bar-warning", players);
        Assert.Contains("width: 30.0%", players);
        Assert.Contains("width: 20.0%", players);
        Assert.Contains("125 / 250", players);
        Assert.Contains("Black and white", players);
        Assert.Contains("Spawning", players);
        Assert.Contains("label-primary", players);
        Assert.Contains("img-gray", players);
        Assert.Contains($"/avatar/{Alice}", players);
        Assert.Contains($"/player/{Alice}", players);
        Assert.Contains("id=\"tr0-survivor-0\"", players);
        Assert.Contains("id=\"tr1-infected-1\"", players);
        Assert.Contains("id=\"spectator-76561198000000005\"", players);
        Assert.Contains("+172", players);
        Assert.Contains("<script id=\"survivors\" type=\"application/json\">", players);
        Assert.IsLessThan(players.IndexOf("AliceTheSurvivor", StringComparison.Ordinal), players.IndexOf("BobTheDead", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task TheLightThemeUsesTheLightIcons()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        client.DefaultRequestHeaders.Add("Cookie", "hub_theme=light");

        await factory.CompleteSetupAsync();

        var credentials = await factory.RunAsync(services => services.GetRequiredService<IGameServerService>().CreateAsync("Test Server", "200.100.50.1:27015"));

        await factory.RunAsync(services =>
        {
            services.GetRequiredService<ILiveDataService>().UpdatePlayers(credentials.Server, new PlayersCommand
            {
                Survivors = [new LiveSurvivor { CommunityId = Alice, Name = "AliceTheSurvivor", IsPlayerAlive = true, PrimaryWeapon = Weapon.Smg }]
            });

            return Task.CompletedTask;
        });

        factory.PassTheDelay();

        var page = await client.GetStringAsync("/servers");

        Assert.Contains("bootstrap.light.min.css", page);
        Assert.Contains("/imgs/weapon/weapon_smg-light.png", page);
        Assert.Contains("/imgs/icons/copy-light.png", page);
        Assert.DoesNotContain("copy-dark.png", page);
    }

    [TestMethod]
    public async Task TheHeaderShowsTheNameAndTheSlotsSentByTheGameServer()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new SiteSettings { Name = "Zone Brasil" });

        var credentials = await factory.RunAsync(services => services.GetRequiredService<IGameServerService>().CreateAsync("Test Server", "200.100.50.1:27015"));
        using var server = factory.ServerClient(credentials);

        await factory.RunAsync(services => services.GetRequiredService<IGameServerService>().SetAlternativeAddressAsync(credentials.Server.Id, "server.example.com"));

        var before = await client.GetStringAsync("/servers/test-server/header");

        await server.PostAsJsonAsync("/api/servers/heartbeat", new { hostname = "Zone | Competitive", map = "c2m1_highway", maxPlayers = 30 });
        await server.PutAsync("/api/game-info/configuration", LiveWebFactory.Json("""{"teamSize":4,"name":"ZoneMod v2.9"}"""));
        await server.PutAsync("/api/game-info/round", LiveWebFactory.Json("""{"isInReady":true,"antiCheatStatus":1,"tankPercent":0.55,"witchPercent":0,"maxChapterProgressPoints":400}"""));
        await server.PutAsync("/api/game-info/players", LiveWebFactory.Json($$"""{"survivors":[{"communityId":"{{Alice}}","name":"AliceTheSurvivor","isPlayerAlive":true}]}"""));

        factory.PassTheDelay();

        var header = await client.GetStringAsync("/servers/test-server/header");
        var page = await client.WithScriptsAsync("/servers");
        var badge = await client.GetFromJsonAsync<JsonElement>("/api/server");
        var status = factory.Services.GetRequiredService<IGameServerPresence>().GetStatus(credentials.Server);

        Assert.Contains("Test Server", before);
        Assert.Contains("0 / 8", before);
        Assert.Contains("Zone | Competitive - ZoneMod v2.9 - 4v4", header);
        Assert.Contains("1 / 30", header);
        Assert.Contains("Dark Carnival - c2m1_highway - 1st round", header);
        Assert.Contains("Starting...", header);
        Assert.Contains("Anti-Cheat on", header);
        Assert.Contains("glyphicon-lock", header);
        Assert.DoesNotContain("/anti-cheat", header);
        Assert.Contains("55%", header);
        Assert.Contains("Disabled", header);
        Assert.Contains("copyTextToClipboard('connect 200.100.50.1:27015', this)", header);
        Assert.Contains("data-label=\"Progress points:\"", header);
        Assert.AreEqual(30, status.Signal!.MaxPlayers);
        Assert.AreEqual(1, badge.GetProperty("players").GetInt32());
        Assert.AreEqual(30, badge.GetProperty("max_players").GetInt32());
        Assert.AreEqual("Zone | Competitive", badge.GetProperty("name").GetString());
        Assert.Contains("Follow the Zone Brasil server, map Dark Carnival, and its 1 connected players.", page);
        Assert.Contains("/imgs/campaigns/dark-carnival.jpg", page);
        Assert.Contains("<code>connect 200.100.50.1:27015</code>", page);
        Assert.Contains("<code>connect server.example.com</code>", page);
        Assert.Contains("steam://connect/200.100.50.1:27015", page);
        Assert.Contains("id=\"game-info-chat\"", page);
        Assert.Contains("id=\"server-info-modal\"", page);
        Assert.Contains("diffDOM.DiffDOM", page);
    }

    [TestMethod]
    public async Task TheCampaignsOfTheTerminalComeFromTheCatalog()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var campaigns = await client.GetFromJsonAsync<JsonElement>("/api/campaigns");
        var first = campaigns.EnumerateArray().First(campaign => campaign.GetProperty("name").GetString() == "Dead Center");

        Assert.AreEqual("c1m1_hotel", first.GetProperty("maps")[0].GetString());
    }

    [TestMethod]
    public async Task OnlyAnAdministratorWithRemoteCommandsOnSeesTheTerminalButton()
    {
        using var factory = new LiveWebFactory();
        using var administrator = factory.Client(HubWebFactory.Administrator);
        using var player = factory.Client(HubWebFactory.Player);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();
        await factory.CreateServerAsync();

        var asAdministrator = await administrator.GetStringAsync("/servers");
        var asPlayer = await player.GetStringAsync("/servers");

        await factory.EnableAsync(true, true, false);

        var withoutCommands = await administrator.GetStringAsync("/servers");

        Assert.Contains("onclick=\"terminal.show()\"", asAdministrator);
        Assert.DoesNotContain("onclick=\"terminal.show()\"", asPlayer);
        Assert.Contains("name=\"message\"", asPlayer);
        Assert.DoesNotContain("onclick=\"terminal.show()\"", withoutCommands);
    }

    [TestMethod]
    public async Task ABannedPlayerWhoStillPostsIsRefusedWithTheReason()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Player);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var credentials = await factory.CreateServerAsync();
        var page = await client.GetStringAsync("/servers/test-server");
        var token = Regex.Match(page, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;

        await factory.RunAsync(services => services.GetRequiredService<IChatBanService>().SaveAsync(HubWebFactory.Player, "Player", "Too much spam", HubWebFactory.Administrator, "Admin"));

        var response = await client.PostAsync("/servers/test-server/chat", new FormUrlEncodedContent(new Dictionary<string, string> { ["message"] = "hello", ["__RequestVerificationToken"] = token }));
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var messages = factory.Services.GetRequiredService<ILiveServerStore>().For(credentials.Server.Id).SiteMessages;

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.IsFalse(body.GetProperty("success").GetBoolean());
        Assert.AreEqual("You have been banned from the chat. Reason: Too much spam", body.GetProperty("errorMessage").GetString());
        Assert.IsEmpty(messages);
    }

    [TestMethod]
    public async Task AMessageOfTheSiteAppearsInTheFeedAsExternalWithoutWaiting()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var credentials = await factory.CreateServerAsync();

        await factory.RunAsync(services => services.GetRequiredService<ISiteChatService>().SendAsync(credentials.Server, new SiteChatUser(long.Parse(Alice), "AliceFromTheSite"), "hello from the site"));
        await factory.RunAsync(services =>
        {
            services.GetRequiredService<ILiveDataService>().AddMessage(credentials.Server, new ChatMessageCommand { CommunityId = Bob, Name = "BobInTheGame", Team = LiveTeam.Infected, Public = true, IsAdmin = true, Message = "hello from the game" });

            return Task.CompletedTask;
        });

        var first = await client.GetStringAsync("/servers/test-server/feed?after=0");

        factory.PassTheDelay();

        var second = await client.GetStringAsync("/servers/test-server/feed?after=1");

        Assert.Contains("AliceFromTheSite", first);
        Assert.Contains("(External)", first);
        Assert.Contains("text-warning", first);
        Assert.Contains("data-feed-kind=\"message\"", first);
        Assert.DoesNotContain("BobInTheGame", first);
        Assert.Contains("BobInTheGame", second);
        Assert.Contains("text-danger", second);
        Assert.Contains("(Admin)", second);
        Assert.DoesNotContain("(Infected)", second);
        Assert.DoesNotContain("AliceFromTheSite", second);
    }
}
