using System.Net;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Live;

[TestClass]
public class SiteChatPageTests
{
    private const string LivePage = "/servers/test-server";
    private const string ChatAction = "/servers/test-server/chat";

    [TestMethod]
    public async Task ASignedInPlayerWritesToTheServer()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Player);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var credentials = await factory.CreateServerAsync();
        var response = await HubWebFactory.PostFormAsync(client, LivePage, ChatAction, new Dictionary<string, string> { ["message"] = "hello from the site" });
        var messages = factory.Services.GetRequiredService<ILiveServerStore>().For(credentials.Server.Id).SiteMessages;
        var page = await client.GetStringAsync(LivePage);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"success\":true", await response.Content.ReadAsStringAsync());
        Assert.HasCount(1, messages);
        Assert.AreEqual("hello from the site", messages[0].Text);
        Assert.AreEqual(HubWebFactory.Player.ToString(), messages[0].CommunityId);
        Assert.Contains("hello from the site", page);
    }

    [TestMethod]
    public async Task APlayerSignedInWithoutANameWritesWithTheirKnownName()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Player);

        client.DefaultRequestHeaders.Add(HubWebFactory.NamelessHeader, "1");

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();
        await factory.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            dbContext.PlayerProfiles.Add(new PlayerProfile { CommunityId = HubWebFactory.Player, Name = "Altair" });

            await dbContext.SaveChangesAsync();
        });

        var credentials = await factory.CreateServerAsync();
        var response = await HubWebFactory.PostFormAsync(client, LivePage, ChatAction, new Dictionary<string, string> { ["message"] = "hello" });
        var messages = factory.Services.GetRequiredService<ILiveServerStore>().For(credentials.Server.Id).SiteMessages;

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.HasCount(1, messages);
        Assert.AreEqual("Altair", messages[0].Name);
    }

    [TestMethod]
    public async Task ThePageAnswersTheFormInPlaceAndKeepsTheCooldown()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Player);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var credentials = await factory.CreateServerAsync();

        var first = await HubWebFactory.PostFormAsync(client, LivePage, ChatAction, new Dictionary<string, string> { ["message"] = "one" });
        var second = await HubWebFactory.PostFormAsync(client, LivePage, ChatAction, new Dictionary<string, string> { ["message"] = "two" });

        factory.Clock.Advance(TimeSpan.FromMinutes(1));

        var third = await HubWebFactory.PostFormAsync(client, LivePage, ChatAction, new Dictionary<string, string> { ["message"] = "three" });
        var messages = factory.Services.GetRequiredService<ILiveServerStore>().For(credentials.Server.Id).SiteMessages;

        Assert.AreEqual(HttpStatusCode.OK, first.StatusCode);
        Assert.Contains("\"success\":true", await first.Content.ReadAsStringAsync());
        Assert.AreEqual(HttpStatusCode.OK, second.StatusCode);
        Assert.Contains("Too many messages.", await second.Content.ReadAsStringAsync());
        Assert.Contains("\"success\":true", await third.Content.ReadAsStringAsync());
        Assert.HasCount(2, messages);
    }

    [TestMethod]
    public async Task AVisitorWhoIsNotSignedInCannotWrite()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();

        var credentials = await factory.CreateServerAsync();
        var page = await client.GetStringAsync(LivePage);
        var response = await HubWebFactory.PostFormAsync(client, LivePage, ChatAction, new Dictionary<string, string> { ["message"] = "hello" });
        var messages = factory.Services.GetRequiredService<ILiveServerStore>().For(credentials.Server.Id).SiteMessages;

        Assert.Contains("Sign in with Steam", page);
        Assert.DoesNotContain("name=\"message\"", page);
        Assert.IsFalse(response.IsSuccessStatusCode);
        Assert.IsEmpty(messages);
    }

    [TestMethod]
    public async Task ABannedPlayerSeesTheBanInsteadOfTheForm()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Player);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();
        await factory.CreateServerAsync();
        await factory.RunAsync(services => services.GetRequiredService<IChatBanService>().SaveAsync(HubWebFactory.Player, "Player", "Too much spam", HubWebFactory.Administrator, "Admin"));

        var page = await client.GetStringAsync(LivePage);

        Assert.Contains("You have been banned from the chat.", page);
        Assert.Contains("Too much spam", page);
        Assert.DoesNotContain("name=\"message\"", page);
    }

    [TestMethod]
    public async Task WhileTheSiteChatIsOffThePageHasNoChatAndPostingIsNotFound()
    {
        using var factory = new LiveWebFactory();
        using var client = factory.Client(HubWebFactory.Player);

        await factory.CompleteSetupAsync();
        await factory.EnableAsync();
        await factory.CreateServerAsync();

        var before = await client.GetStringAsync(LivePage);
        var token = Regex.Match(before, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;

        await factory.EnableAsync(true, false);

        var page = await client.GetAsync(LivePage);
        var body = await page.Content.ReadAsStringAsync();
        var response = await client.PostAsync(ChatAction, new FormUrlEncodedContent(new Dictionary<string, string> { ["message"] = "hello", ["__RequestVerificationToken"] = token }));

        Assert.Contains("name=\"message\"", before);
        Assert.AreEqual(HttpStatusCode.OK, page.StatusCode);
        Assert.DoesNotContain("name=\"message\"", body);
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
