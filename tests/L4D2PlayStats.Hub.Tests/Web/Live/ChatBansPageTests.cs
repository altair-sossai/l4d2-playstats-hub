using System.Net;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Live;

[TestClass]
public class ChatBansPageTests
{
    private const long Alice = 76561198000000001;
    private const string Page = "/admin/chat-bans";
    private const string Api = "/api/chat-bans";

    [TestMethod]
    public async Task AnAdministratorSetsTheLimitsOfTheSiteMessages()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var response = await HubWebFactory.PostFormAsync(client, Page, "/admin/chat-bans/limits", new Dictionary<string, string>
        {
            ["GlobalCooldownInSeconds"] = "5",
            ["UserCooldownInSeconds"] = "-3",
            ["MaxMessageLength"] = "999"
        });

        var page = await client.GetStringAsync(Page);
        var limits = await factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<SiteChatSettings>());

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual(5, limits.GlobalCooldownInSeconds);
        Assert.AreEqual(0, limits.UserCooldownInSeconds);
        Assert.AreEqual(200, limits.MaxMessageLength);
        Assert.Contains("name=\"GlobalCooldownInSeconds\" type=\"number\" min=\"0\" max=\"3600\" value=\"5\"", page);
    }

    [TestMethod]
    public async Task OnlyAnAdministratorSetsTheLimitsOfTheSiteMessages()
    {
        using var factory = new LiveWebFactory();
        using var player = factory.Client(HubWebFactory.Player);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var response = await player.PostAsync("/admin/chat-bans/limits", new FormUrlEncodedContent(new Dictionary<string, string> { ["GlobalCooldownInSeconds"] = "0" }));
        var limits = await factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<SiteChatSettings>());

        Assert.AreNotEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(15, limits.GlobalCooldownInSeconds);
    }

    [TestMethod]
    public async Task ThePageListsTheBansWithEverythingTheDetailsNeed()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();
        await factory.RunAsync(services => services.GetRequiredService<IChatBanService>().SaveAsync(Alice, "AliceTheSpammer", "Flooding the chat", HubWebFactory.Administrator, "TheAdmin"));

        var page = await client.GetStringAsync(Page);

        Assert.Contains("data-community-id=\"76561198000000001\"", page);
        Assert.Contains("data-name=\"AliceTheSpammer\"", page);
        Assert.Contains("data-avatar=\"/avatar/76561198000000001\"", page);
        Assert.Contains("data-steam-id=\"STEAM_1:1:19867136\"", page);
        Assert.Contains("data-profile-url=\"https://steamcommunity.com/profiles/76561198000000001\"", page);
        Assert.Contains("data-reason=\"Flooding the chat\"", page);
        Assert.Contains("data-banned-by=\"TheAdmin\"", page);
        Assert.Contains("by TheAdmin", page);
        Assert.Contains("players banned from the chat", page);
        Assert.Contains("id=\"cb-details-modal\"", page);
        Assert.Contains("id=\"cb-add-modal\"", page);
        Assert.Contains("id=\"pp-modal\"", page);
        Assert.Contains("Showing 1&#x2013;1 of 1", page);
        Assert.DoesNotContain("bootstrap.bundle", page);
    }

    [TestMethod]
    public async Task TheSearchLooksAtNameSteamIdAndReasonAndThePagesHaveTwentyBans()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        await factory.RunAsync(async services =>
        {
            var bans = services.GetRequiredService<IChatBanService>();

            for (var index = 0; index < 25; index++)
            {
                factory.Clock.Advance(TimeSpan.FromMinutes(1));

                await bans.SaveAsync(Alice + index, $"Banned{index:00}Player", index == 7 ? "Selling accounts" : null, HubWebFactory.Administrator, "TheAdmin");
            }
        });

        var first = await client.GetStringAsync(Page);
        var second = await client.GetStringAsync($"{Page}?page=2");
        var byName = await client.GetStringAsync($"{Page}?q=banned03");
        var byReason = await client.GetStringAsync($"{Page}?q=selling");
        var bySteamId = await client.GetStringAsync($"{Page}?q={Alice + 11}");
        var nothing = await client.GetStringAsync($"{Page}?q=nobody");

        Assert.Contains("Showing 1&#x2013;20 of 25", first);
        Assert.Contains("Banned24Player", first);
        Assert.DoesNotContain("Banned04Player", first);
        Assert.Contains("Showing 21&#x2013;25 of 25", second);
        Assert.Contains("Banned04Player", second);
        Assert.Contains("/admin/chat-bans?page=2", first);
        Assert.Contains("Banned03Player", byName);
        Assert.DoesNotContain("Banned04Player", byName);
        Assert.Contains("Banned07Player", byReason);
        Assert.Contains("Selling accounts", byReason);
        Assert.Contains("No reason given", byName);
        Assert.Contains("Banned11Player", bySteamId);
        Assert.DoesNotContain("Banned12Player", bySteamId);
        Assert.Contains("No chat bans found.", nothing);
        Assert.Contains("Clear filters", nothing);
    }

    [TestMethod]
    public async Task ABanAcceptsAnyWayOfWritingTheSteamId()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var byUrl = await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Post, Api, """{"communityId":"https://steamcommunity.com/profiles/76561198000000001/","name":"Alice","reason":"Spam"}""");
        var bySteamId = await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Post, Api, """{"communityId":"STEAM_1:0:19867137","name":"Bob"}""");
        var update = await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Post, Api, """{"communityId":"[U:1:39734273]","reason":"Insults"}""");
        var invalid = await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Post, Api, """{"communityId":"alice"}""");
        var empty = await LiveWebFactory.SendJsonAsync(client, Page, HttpMethod.Post, Api, """{"communityId":" "}""");

        var created = await byUrl.Content.ReadFromJsonAsync<JsonElement>();
        var error = await invalid.Content.ReadFromJsonAsync<JsonElement>();
        var list = await client.GetFromJsonAsync<JsonElement>(Api);
        var alice = await factory.RunAsync(services => services.GetRequiredService<IChatBanService>().FindAsync(Alice));

        Assert.AreEqual(HttpStatusCode.OK, byUrl.StatusCode);
        Assert.AreEqual("76561198000000001", created.GetProperty("communityId").GetString());
        Assert.AreEqual("/avatar/76561198000000001", created.GetProperty("avatarUrl").GetString());
        Assert.AreEqual(HttpStatusCode.OK, bySteamId.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, update.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.AreEqual("The Steam ID is invalid.", error.GetProperty("errorMessage").GetString());
        Assert.AreEqual(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.AreEqual(2, list.GetArrayLength());
        Assert.AreEqual("Alice", alice!.Name);
        Assert.AreEqual("Insults", alice.Reason);
        Assert.IsNotNull(alice.UpdatedAtUtc);
    }

    [TestMethod]
    public async Task TheRecentSendersSayWhoIsAlreadyBanned()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var credentials = await factory.CreateServerAsync();

        await factory.RunAsync(services => services.GetRequiredService<ISiteChatService>().SendAsync(credentials.Server, new SiteChatUser(Alice, "AliceTheChatter"), "buy my skins"));
        factory.Clock.Advance(TimeSpan.FromMinutes(1));
        await factory.RunAsync(services => services.GetRequiredService<ISiteChatService>().SendAsync(credentials.Server, new SiteChatUser(Alice + 1, "BobTheChatter"), "hello"));
        await factory.RunAsync(services => services.GetRequiredService<IChatBanService>().SaveAsync(Alice, "AliceTheChatter", null, HubWebFactory.Administrator, "TheAdmin"));

        var senders = await client.GetFromJsonAsync<JsonElement>($"{Api}/recent-senders");

        Assert.AreEqual(2, senders.GetArrayLength());
        Assert.AreEqual("BobTheChatter", senders[0].GetProperty("name").GetString());
        Assert.IsFalse(senders[0].GetProperty("alreadyBanned").GetBoolean());
        Assert.AreEqual("AliceTheChatter", senders[1].GetProperty("name").GetString());
        Assert.AreEqual("buy my skins", senders[1].GetProperty("message").GetString());
        Assert.AreEqual("76561198000000001", senders[1].GetProperty("communityId").GetString());
        Assert.AreEqual("/avatar/76561198000000001", senders[1].GetProperty("avatarUrl").GetString());
        Assert.IsTrue(senders[1].GetProperty("alreadyBanned").GetBoolean());
    }

    [TestMethod]
    public async Task OnlyAnAdministratorWithTheTokenOfThePageChangesTheBans()
    {
        using var factory = new LiveWebFactory();
        using var administrator = factory.Client(HubWebFactory.Administrator);
        using var player = factory.Client(HubWebFactory.Player);
        using var visitor = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var command = """{"communityId":"76561198000000001","name":"Alice"}""";

        var withoutToken = await administrator.PostAsync(Api, LiveWebFactory.Json(command));
        var asPlayer = await player.PostAsync(Api, LiveWebFactory.Json(command));
        var listAsPlayer = await player.GetAsync(Api);
        var pageAsPlayer = await player.GetAsync(Page);
        var asVisitor = await visitor.GetAsync($"{Api}/recent-senders");
        var bans = await factory.RunAsync(services => services.GetRequiredService<IChatBanService>().ListAsync());

        await factory.EnableAsync(true, false);

        var moduleOff = await administrator.GetAsync(Api);

        Assert.AreEqual(HttpStatusCode.BadRequest, withoutToken.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, asPlayer.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, listAsPlayer.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, pageAsPlayer.StatusCode);
        Assert.IsFalse(asVisitor.IsSuccessStatusCode);
        Assert.IsEmpty(bans);
        Assert.AreEqual(HttpStatusCode.NotFound, moduleOff.StatusCode);
    }
}