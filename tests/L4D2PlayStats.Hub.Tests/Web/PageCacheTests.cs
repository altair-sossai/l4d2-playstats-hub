using System.Net;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Community.Support;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Web.Infrastructure.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;
using WebConsts = L4D2PlayStats.Hub.Web.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class PageCacheTests
{
    private const long Stranger = 76561198000000999;
    private const string AuthenticationCookie = ".AspNetCore.Cookies=signed-in";
    private const string TempDataCookie = ".AspNetCore.Mvc.CookieTempDataProvider=message";

    [TestMethod]
    public async Task AnAnonymousPageIsServedFromTheCacheWithoutSettingCookies()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await StartAsync(factory);

        foreach (var path in new[] { "/", "/players", $"/player/{StatisticsBuilder.PlayerA1}", "/matches", "/history", "/ingame" })
        {
            var first = await GetAsync(client, path);
            var second = await GetAsync(client, path);

            Assert.AreEqual(HttpStatusCode.OK, first.Status, path);
            Assert.IsFalse(first.Cached, path);
            Assert.IsTrue(second.Cached, path);
            Assert.AreEqual(first.Html, second.Html, path);
            Assert.IsFalse(first.SetsCookies, path);
        }
    }

    [TestMethod]
    public async Task ASignedInPlayerNeverGetsTheCachedAnonymousPageAndAnonymousVisitorsNeverGetHis()
    {
        using var factory = new HubWebFactory();
        using var anonymous = factory.Client();
        using var signedIn = factory.Client(StatisticsBuilder.PlayerA1);

        await StartAsync(factory);

        await GetAsync(anonymous, "/");
        var cachedAnonymous = await GetAsync(anonymous, "/");
        var player = await GetAsync(signedIn, "/");
        var playerAgain = await GetAsync(signedIn, "/");
        var anonymousAfterPlayer = await GetAsync(anonymous, "/");

        Assert.IsTrue(cachedAnonymous.Cached);
        Assert.IsFalse(player.Cached);
        Assert.IsFalse(playerAgain.Cached);
        Assert.Contains("logout-form", player.Html);
        Assert.Contains("it-is-me\">", player.Html);
        Assert.Contains("logout-form", playerAgain.Html);
        Assert.IsTrue(anonymousAfterPlayer.Cached);
        Assert.DoesNotContain("logout-form", anonymousAfterPlayer.Html);
        Assert.DoesNotContain("it-is-me\">", anonymousAfterPlayer.Html);
    }

    [TestMethod]
    public async Task ThePageOfASignedInPlayerIsNeverStoredForAnonymousVisitors()
    {
        using var factory = new HubWebFactory();
        using var anonymous = factory.Client();
        using var signedIn = factory.Client(StatisticsBuilder.PlayerA1);

        await StartAsync(factory);

        await GetAsync(signedIn, "/players");
        await GetAsync(signedIn, "/players");

        var visitor = await GetAsync(anonymous, "/players");

        Assert.IsFalse(visitor.Cached);
        Assert.DoesNotContain("logout-form", visitor.Html);
        Assert.DoesNotContain("it-is-me\">", visitor.Html);
    }

    [TestMethod]
    public async Task RequestsWithTheLoginOrMessageCookiesSkipTheCache()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await StartAsync(factory);

        await GetAsync(client, "/players");
        await GetAsync(client, "/players");

        var login = await GetAsync(client, "/players", AuthenticationCookie);
        var message = await GetAsync(client, "/players", TempDataCookie);
        var anonymous = await GetAsync(client, "/players");

        Assert.IsFalse(login.Cached);
        Assert.IsFalse(message.Cached);
        Assert.IsTrue(anonymous.Cached);
    }

    [TestMethod]
    public async Task EachLanguageAndThemeHasItsOwnCachedPage()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await StartAsync(factory);

        await GetAsync(client, "/players");

        var english = await GetAsync(client, "/players");
        var light = await GetAsync(client, "/players", "hub_theme=light");
        var lightAgain = await GetAsync(client, "/players", "hub_theme=light");
        var portuguese = await GetAsync(client, "/players", ".AspNetCore.Culture=c%3Dpt-BR%7Cuic%3Dpt-BR");
        var spanish = await GetAsync(client, "/players", null, "es-ES");
        var spanishAgain = await GetAsync(client, "/players", null, "es-ES");

        Assert.IsTrue(english.Cached);
        Assert.Contains("<html lang=\"en-US\">", english.Html);
        Assert.DoesNotContain("bootstrap.light.min.css", english.Html);
        Assert.IsFalse(light.Cached);
        Assert.Contains("bootstrap.light.min.css", light.Html);
        Assert.IsTrue(lightAgain.Cached);
        Assert.Contains("bootstrap.light.min.css", lightAgain.Html);
        Assert.IsFalse(portuguese.Cached);
        Assert.Contains("<html lang=\"pt-BR\">", portuguese.Html);
        Assert.IsFalse(spanish.Cached);
        Assert.Contains("<html lang=\"es-ES\">", spanish.Html);
        Assert.IsTrue(spanishAgain.Cached);
        Assert.Contains("<html lang=\"es-ES\">", spanishAgain.Html);
    }

    [TestMethod]
    public async Task TheInGamePanelKeepsOnePagePerPlayer()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await StartAsync(factory);

        var player = await GetAsync(client, $"/ingame/player?steamid={StatisticsBuilder.PlayerA1}");
        var stranger = await GetAsync(client, $"/ingame/player?steamid={Stranger}");
        var playerAgain = await GetAsync(client, $"/ingame/player?steamid={StatisticsBuilder.PlayerA1}");

        Assert.AreEqual(HttpStatusCode.OK, player.Status);
        Assert.IsFalse(stranger.Cached);
        Assert.AreNotEqual(player.Html, stranger.Html);
        Assert.IsTrue(playerAgain.Cached);
        Assert.AreEqual(player.Html, playerAgain.Html);
    }

    [TestMethod]
    public async Task AStatisticsUploadChangesTheCachedPagesRightAway()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        await GetAsync(client, "/players");
        var empty = await GetAsync(client, "/players");

        await UploadAsync(server);

        var filled = await GetAsync(client, "/players");

        Assert.IsTrue(empty.Cached);
        Assert.DoesNotContain($"/player/{StatisticsBuilder.PlayerA1}\"", empty.Html);
        Assert.IsFalse(filled.Cached);
        Assert.Contains($"/player/{StatisticsBuilder.PlayerA1}\"", filled.Html);
    }

    [TestMethod]
    public async Task ASettingsSaveChangesTheCachedPagesRightAway()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await StartAsync(factory);
        await factory.SaveAsync(new SiteSettings { Name = "Before Hub" });

        await GetAsync(client, "/");
        var before = await GetAsync(client, "/");

        await factory.SaveAsync(new SiteSettings { Name = "After Hub" });

        var after = await GetAsync(client, "/");

        Assert.IsTrue(before.Cached);
        Assert.Contains("Before Hub", before.Html);
        Assert.IsFalse(after.Cached);
        Assert.Contains("After Hub", after.Html);
        Assert.DoesNotContain("Before Hub", after.Html);
    }

    [TestMethod]
    public async Task ADataChangeEvictsTheCachedPages()
    {
        using var factory = new HubWebFactory();

        await StartAsync(factory);

        using var recorded = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services => services.Decorate<IOutputCacheStore, RecordingOutputCacheStore>()));
        using var client = recorded.CreateClient();

        await GetAsync(client, "/matches");
        var cached = await GetAsync(client, "/matches");
        var store = (RecordingOutputCacheStore)recorded.Services.GetRequiredService<IOutputCacheStore>();
        var key = store.StoredKeys.Single();
        var before = await store.GetAsync(key, CancellationToken.None);

        recorded.Services.GetRequiredService<HubState>().DataChanged();

        var after = await store.GetAsync(key, CancellationToken.None);
        var fresh = await GetAsync(client, "/matches");

        Assert.IsTrue(cached.Cached);
        Assert.IsNotNull(before);
        Assert.IsNull(after);
        CollectionAssert.Contains(store.EvictedTags.ToList(), WebConsts.PageCacheTag);
        Assert.IsFalse(fresh.Cached);
    }

    [TestMethod]
    public async Task ACommunityVoteChangesTheCachedCommunityRankingRightAway()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumComparisonsToBeListed = 1 });

        using var voter = site.Factory.Client(site.Voter);
        using var anonymous = site.Factory.Client();

        await GetAsync(anonymous, "/community");
        var empty = await GetAsync(anonymous, "/community");
        var players = await GetAsync(anonymous, "/players");
        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        var ranking = await GetAsync(anonymous, "/community");
        var playersAfterVote = await GetAsync(anonymous, "/players");

        Assert.IsTrue(empty.Cached);
        Assert.IsFalse(ranking.Cached);
        Assert.Contains($"href=\"/player/{pair.Left}\"", ranking.Html);
        Assert.IsTrue(playersAfterVote.Cached);
        Assert.AreEqual(players.Html, playersAfterVote.Html);
    }

    [TestMethod]
    public async Task RenamingAGameServerChangesTheCachedPagesRightAway()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync("Alpha Arena");
        using var server = factory.ServerClient(credentials);

        await UploadAsync(server);
        await factory.CreateServerAsync("Beta Arena");

        await GetAsync(client, "/matches");
        var before = await GetAsync(client, "/matches");

        await factory.RunAsync(services => services.GetRequiredService<IGameServerService>().UpdateAsync(credentials.Server.Id, "Gamma Arena", null, true));

        var after = await GetAsync(client, "/matches");

        Assert.IsTrue(before.Cached);
        Assert.Contains("Alpha Arena", before.Html);
        Assert.IsFalse(after.Cached);
        Assert.Contains("Gamma Arena", after.Html);
    }

    [TestMethod]
    public async Task PagesWithAnAntiforgeryTokenForbidCaching()
    {
        using var factory = new HubWebFactory();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        await StartAsync(factory);

        using var response = await administrator.GetAsync($"/player/{StatisticsBuilder.PlayerA1}");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains(WebConsts.AntiforgeryHeader, html);
        Assert.IsTrue(response.Headers.CacheControl?.NoStore);
        Assert.IsFalse(PageCachePolicy.IsStorable(Response(StatusCodes.Status200OK, null, response.Headers.CacheControl?.ToString())));
    }

    [TestMethod]
    public void ResponsesThatSetCookiesOrForbidCachingAreNeverStored()
    {
        Assert.IsTrue(PageCachePolicy.IsStorable(Response(StatusCodes.Status200OK, null, null)));
        Assert.IsFalse(PageCachePolicy.IsStorable(Response(StatusCodes.Status200OK, "a=b", null)));
        Assert.IsFalse(PageCachePolicy.IsStorable(Response(StatusCodes.Status200OK, null, "no-cache, no-store")));
        Assert.IsFalse(PageCachePolicy.IsStorable(Response(StatusCodes.Status200OK, null, "private")));
        Assert.IsFalse(PageCachePolicy.IsStorable(Response(StatusCodes.Status404NotFound, null, null)));
        Assert.IsFalse(PageCachePolicy.IsStorable(Response(StatusCodes.Status302Found, null, null)));
    }

    private static HttpResponse Response(int status, string? setCookie, string? cacheControl)
    {
        var context = new DefaultHttpContext();
        var response = context.Response;

        response.StatusCode = status;

        if (setCookie != null)
            response.Headers.SetCookie = setCookie;

        if (cacheControl != null)
            response.Headers.CacheControl = cacheControl;

        return response;
    }

    private static async Task<CachedPageResponse> GetAsync(HttpClient client, string path, string? cookie = null, string? language = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);

        if (cookie != null)
            request.Headers.Add("Cookie", cookie);

        if (language != null)
            request.Headers.Add("Accept-Language", language);

        using var response = await client.SendAsync(request);

        return new CachedPageResponse(response.StatusCode, await response.Content.ReadAsStringAsync(), response.Headers.Age != null, response.Headers.Contains("Set-Cookie"));
    }

    private static async Task StartAsync(HubWebFactory factory)
    {
        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        await UploadAsync(server);
    }

    private static async Task UploadAsync(HttpClient server)
    {
        var night = RecentNight();

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", night, 1, 300, 200));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", night.AddMinutes(15), 2, 600, 500));
    }

    private static DateTime RecentNight()
    {
        var now = DateTime.UtcNow;

        return new DateTime(now.Year, now.Month, Math.Min(now.Day, 20), 12, 0, 0);
    }
}
