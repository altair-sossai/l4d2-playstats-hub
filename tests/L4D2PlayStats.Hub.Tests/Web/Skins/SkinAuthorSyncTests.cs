using System.Net;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Skins;
using L4D2PlayStats.Hub.Web;
using L4D2PlayStats.Hub.Web.Infrastructure.BackgroundServices;
using L4D2PlayStats.Hub.Web.Models.Skins;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Skins;

[TestClass]
public class SkinAuthorSyncTests
{
    private const long SampleAuthor = 76561198000000001;
    private const long AnotherAuthor = 76561198000000002;

    [TestMethod]
    public async Task AnAuthorWhoChangedTheirNameOnSteamIsRenamedOnEverySkinOfTheirs()
    {
        using var factory = new HubWebFactory();
        using var application = WithSteam(factory, Profiles(new Dictionary<long, string> { [SampleAuthor] = "Renamed Author", [AnotherAuthor] = "Another Author" }));

        await factory.StartSkinShopAsync();

        var result = await SyncAsync(application, true);
        var names = await AuthorNamesAsync(application);

        Assert.AreEqual(2, result.Due);
        Assert.AreEqual(2, result.Checked);
        Assert.AreEqual(3, result.Renamed);
        Assert.AreEqual("Renamed Author", names[SampleAuthor].Single());
        Assert.AreEqual("Another Author", names[AnotherAuthor].Single());
    }

    [TestMethod]
    public async Task ImportingTheCatalogAgainKeepsTheNameFromSteam()
    {
        using var factory = new HubWebFactory();
        using var application = WithSteam(factory, Profiles(new Dictionary<long, string> { [SampleAuthor] = "Renamed Author", [AnotherAuthor] = "Another Author" }));

        await factory.StartSkinShopAsync();
        await SyncAsync(application, true);

        await RunAsync(application, services => services.GetRequiredService<ISkinCatalogService>().ImportAsync(SkinSamples.Catalog()));

        var names = await AuthorNamesAsync(application);

        Assert.AreEqual("Renamed Author", names[SampleAuthor].Single());
    }

    [TestMethod]
    public async Task AnAuthorIsCheckedAgainOnlyAfterADayAndOneWhoseLookupFailedIsTriedAgainSooner()
    {
        using var factory = new HubWebFactory();
        using var application = WithSteam(factory, uri =>
        {
            if (uri.AbsolutePath.Contains(AnotherAuthor.ToString()))
                throw new HttpRequestException("connection reset");

            return Xml("Sample Author");
        });

        await factory.StartSkinShopAsync();

        var first = await SyncAsync(application, false);
        var second = await SyncAsync(application, false);

        Assert.AreEqual(2, first.Due);
        Assert.AreEqual(1, first.Checked);
        Assert.AreEqual(1, second.Due);
        Assert.AreEqual(0, second.Checked);
    }

    [TestMethod]
    public async Task WithASteamKeyTheNamesComeFromTheWebApi()
    {
        using var factory = new HubWebFactory();
        var requested = new List<string>();

        using var application = WithSteam(factory, uri =>
        {
            requested.Add(uri.Host);

            var content = $$$"""{"response":{"players":[{"steamid":"{{{SampleAuthor}}}","personaname":"Api Name"},{"steamid":"{{{AnotherAuthor}}}","personaname":"Another Author"}]}}""";

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) };
        });

        await factory.StartSkinShopAsync();
        await factory.SaveAsync(new SiteSettings { SteamApiKey = "test-key" });

        var result = await SyncAsync(application, true);
        var names = await AuthorNamesAsync(application);

        Assert.AreEqual(3, result.Renamed);
        Assert.AreEqual("Api Name", names[SampleAuthor].Single());
        CollectionAssert.AreEqual(new[] { "api.steampowered.com" }, requested);
    }

    [TestMethod]
    public async Task AProfileWithoutANameKeepsTheCatalogNameAndCountsAsChecked()
    {
        using var factory = new HubWebFactory();
        using var application = WithSteam(factory, _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("<response><error>The specified profile could not be found.</error></response>") });

        await factory.StartSkinShopAsync();

        var result = await SyncAsync(application, true);
        var names = await AuthorNamesAsync(application);
        var synced = await RunAsync(application, services => services.GetRequiredService<HubDbContext>().SkinAuthors.CountAsync());

        Assert.AreEqual(2, result.Checked);
        Assert.AreEqual(0, result.Renamed);
        Assert.AreEqual("Sample Author", names[SampleAuthor].Single());
        Assert.AreEqual(2, synced);
    }

    [TestMethod]
    public async Task TheRoutineDoesNothingWhileTheSkinShopIsOff()
    {
        using var factory = new HubWebFactory();
        var calls = 0;

        using var application = WithSteam(factory, _ =>
        {
            calls++;

            return Xml("Renamed Author");
        });

        await factory.StartSkinShopAsync();
        await factory.SaveAsync(new ModuleSettings { SkinShop = false });

        var routine = ActivatorUtilities.CreateInstance<SkinAuthorSyncBackgroundService>(application.Services);
        await routine.RunAsync(CancellationToken.None);

        Assert.AreEqual(0, calls);
    }

    [TestMethod]
    public async Task TheAdministratorSeesTheStatusAndCanCheckTheNamesNow()
    {
        using var factory = new HubWebFactory();
        using var application = WithSteam(factory, Profiles(new Dictionary<long, string> { [SampleAuthor] = "Renamed Author", [AnotherAuthor] = "Another Author" }));

        await factory.StartSkinShopAsync();

        using var client = application.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add(HubWebFactory.UserHeader, HubWebFactory.Administrator.ToString());

        var page = await client.GetStringAsync(AppConsts.SkinCatalogUrl);
        var response = await HubWebFactory.PostFormAsync(client, AppConsts.SkinCatalogUrl, $"{AppConsts.SkinCatalogUrl}/authors", []);
        var after = await client.GetStringAsync(AppConsts.SkinCatalogUrl);
        var names = await AuthorNamesAsync(application);

        Assert.Contains("Author names", page);
        Assert.Contains($"action=\"{AppConsts.SkinCatalogUrl}/authors\"", page);
        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual("Renamed Author", names[SampleAuthor].Single());
        Assert.Contains("last check", after);
    }

    [TestMethod]
    public async Task AVisitorCannotStartTheCheck()
    {
        using var factory = new HubWebFactory();
        using var application = WithSteam(factory, Profiles([]));

        await factory.StartSkinShopAsync();

        using var client = application.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.PostAsync($"{AppConsts.SkinCatalogUrl}/authors", new FormUrlEncodedContent([]));
        var synced = await RunAsync(application, services => services.GetRequiredService<HubDbContext>().SkinAuthors.CountAsync());

        Assert.AreNotEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual(0, synced);
    }

    private static WebApplicationFactory<Program> WithSteam(HubWebFactory factory, Func<Uri, HttpResponseMessage> route)
    {
        var handler = new RoutingHttpMessageHandler(route);

        return factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddHttpClient(AppConsts.SteamHttpClient).ConfigurePrimaryHttpMessageHandler(() => handler)));
    }

    private static Func<Uri, HttpResponseMessage> Profiles(Dictionary<long, string> names)
    {
        return uri =>
        {
            var id = Regex.Match(uri.AbsolutePath, @"/profiles/(\d+)").Groups[1].Value;

            return long.TryParse(id, out var communityId) && names.TryGetValue(communityId, out var name)
                ? Xml(name)
                : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("<response><error>The specified profile could not be found.</error></response>") };
        };
    }

    private static HttpResponseMessage Xml(string name)
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"<profile><steamID><![CDATA[{name}]]></steamID></profile>") };
    }

    private static Task<SkinAuthorSyncResult> SyncAsync(WebApplicationFactory<Program> application, bool everyAuthor)
    {
        return RunAsync(application, services => services.GetRequiredService<ISkinAuthorSyncService>().SyncAsync(everyAuthor));
    }

    private static Task<Dictionary<long, List<string?>>> AuthorNamesAsync(WebApplicationFactory<Program> application)
    {
        return RunAsync(application, async services => (await services.GetRequiredService<HubDbContext>().Set<Skin>().AsNoTracking().Where(skin => skin.AuthorCommunityId != null).ToListAsync())
            .GroupBy(skin => skin.AuthorCommunityId!.Value)
            .ToDictionary(group => group.Key, group => group.Select(skin => skin.AuthorName).Distinct().ToList()));
    }

    private static async Task<T> RunAsync<T>(WebApplicationFactory<Program> application, Func<IServiceProvider, Task<T>> action)
    {
        using var scope = application.Services.CreateScope();

        return await action(scope.ServiceProvider);
    }
}