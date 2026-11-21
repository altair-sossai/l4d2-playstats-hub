using System.Net;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Players;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Web;
using L4D2PlayStats.Hub.Web.Infrastructure.BackgroundServices;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class SteamDataTests
{
    private const long Open = 76561198000000701;
    private const long Hidden = 76561198000000702;
    private const long Stranger = 76561198000000703;
    private const string Key = "STEAMKEY0123456789ABCDEF";

    [TestMethod]
    public async Task TheJobStoresTheSteamDataAndLeavesThePrivateDetailsEmpty()
    {
        using var factory = new HubWebFactory();

        var handler = SteamHandler();
        var application = WithSteam(factory, handler);

        await AddProfilesAsync(application, Open, Hidden);
        await SaveKeyAsync(application, Key);

        var service = ActivatorUtilities.CreateInstance<SteamDataBackgroundService>(application.Services);
        var refreshed = await service.RefreshAsync(CancellationToken.None);
        var calls = handler.Requests.Count;
        var again = await service.RefreshAsync(CancellationToken.None);
        var open = await ProfileAsync(application, Open);
        var hidden = await ProfileAsync(application, Hidden);
        var store = application.Services.GetRequiredService<IDisplayNameStore>();

        Assert.AreEqual(2, refreshed);
        Assert.AreEqual(0, again);
        Assert.AreEqual(calls, handler.Requests.Count);
        Assert.AreEqual(1, handler.Requests.Count(uri => uri.AbsolutePath.Contains("GetPlayerSummaries")));
        Assert.AreEqual(1, handler.Requests.Count(uri => uri.AbsolutePath.Contains("GetPlayerBans")));
        Assert.AreEqual(2, handler.Requests.Count(uri => uri.AbsolutePath.Contains("GetOwnedGames")));
        Assert.AreEqual(2, handler.Requests.Count(uri => uri.AbsolutePath.Contains("GetSteamLevel")));
        Assert.IsTrue(handler.Requests.Where(uri => uri.AbsolutePath.Contains("GetOwnedGames")).All(uri => Uri.UnescapeDataString(uri.Query).Contains("appids_filter[0]=550") && uri.Query.Contains("include_played_free_games=1")));
        Assert.IsTrue(handler.Requests.All(uri => uri.Query.Contains($"key={Key}")));

        Assert.AreEqual("Open Persona", open.SteamName);
        Assert.AreEqual(new DateTime(2010, 5, 1, 0, 0, 0, DateTimeKind.Utc), open.AccountCreatedAtUtc);
        Assert.AreEqual(0, open.VacBans);
        Assert.AreEqual(0, open.GameBans);
        Assert.AreEqual(6000, open.Left4Dead2Minutes);
        Assert.AreEqual(90, open.Left4Dead2RecentMinutes);
        Assert.AreEqual(42, open.SteamLevel);
        Assert.IsNotNull(open.SteamUpdatedAtUtc);
        Assert.AreEqual("Open Persona", open.DisplayName);

        Assert.AreEqual("Hidden Persona", hidden.SteamName);
        Assert.IsNull(hidden.AccountCreatedAtUtc);
        Assert.AreEqual(2, hidden.VacBans);
        Assert.AreEqual(1, hidden.GameBans);
        Assert.AreEqual(30, hidden.DaysSinceLastBan);
        Assert.IsNull(hidden.Left4Dead2Minutes);
        Assert.IsNull(hidden.Left4Dead2RecentMinutes);
        Assert.IsNull(hidden.SteamLevel);
        Assert.IsNotNull(hidden.SteamUpdatedAtUtc);
        Assert.AreEqual("Hidden Persona", store.Find(Hidden));
    }

    [TestMethod]
    public async Task WithoutASteamKeyTheJobCallsNothing()
    {
        using var factory = new HubWebFactory();

        var handler = SteamHandler();
        var application = WithSteam(factory, handler);

        await AddProfilesAsync(application, Open, Hidden);

        var service = ActivatorUtilities.CreateInstance<SteamDataBackgroundService>(application.Services);
        var refreshed = await service.RefreshAsync(CancellationToken.None);
        var open = await ProfileAsync(application, Open);

        Assert.AreEqual(0, refreshed);
        Assert.IsEmpty(handler.Requests);
        Assert.IsNull(open.SteamUpdatedAtUtc);
    }

    [TestMethod]
    public async Task AFailingSteamCallIsTriedAgainLaterWithoutBreakingTheJob()
    {
        using var factory = new HubWebFactory();

        var handler = new RoutingHttpMessageHandler(_ => throw new HttpRequestException("connection reset"));
        var application = WithSteam(factory, handler);

        await AddProfilesAsync(application, Open);
        await SaveKeyAsync(application, Key);

        var service = ActivatorUtilities.CreateInstance<SteamDataBackgroundService>(application.Services);
        var refreshed = await service.RefreshAsync(CancellationToken.None);
        var open = await ProfileAsync(application, Open);

        Assert.AreEqual(0, refreshed);
        Assert.IsNotEmpty(handler.Requests);
        Assert.IsNull(open.SteamUpdatedAtUtc);
    }

    [TestMethod]
    public async Task ThePlayerPageShowsTheSteamDataOrNotAvailable()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        var other = await StartAsync(factory);

        await SetSteamAsync(factory, StatisticsBuilder.PlayerA1);

        var page = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}");
        var empty = await client.GetStringAsync($"/player/{other}");

        Assert.Contains("Hours in Left 4 Dead 2", page);
        Assert.Contains("<span class=\"steam-hours\">100.0 h</span>", page);
        Assert.Contains("1.5 h in the last 2 weeks", page);
        Assert.Contains("<span class=\"steam-level\">42</span>", page);
        Assert.Contains("Steam account created", page);
        Assert.Contains("<span class=\"steam-created\">5/1/2010</span>", page);
        Assert.Contains("years)", page);
        Assert.Contains("VAC bans", page);
        Assert.Contains("<span class=\"steam-bans text-danger\">1</span>", page);
        Assert.Contains("last ban 100 days ago", page);
        Assert.Contains("Game bans", page);
        Assert.DoesNotContain("Not available", page);
        Assert.AreEqual(5, Count(empty, "Not available"));
    }

    [TestMethod]
    public async Task TheComparisonShowsTheSteamDataOfBothPlayers()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        var other = await StartAsync(factory);

        await SetSteamAsync(factory, StatisticsBuilder.PlayerA1);

        var page = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}/{other}");

        Assert.Contains("<span class=\"steam-hours\">100.0 h</span>", page);
        Assert.IsTrue(Regex.IsMatch(page, @"<span class=""second"">\s*<span class=""text-muted steam-not-available"">Not available</span>"));
        Assert.AreEqual(5, Count(page, "Not available"));
    }

    [TestMethod]
    public async Task APlayerWithoutStatisticsStillShowsTheSteamData()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            dbContext.PlayerProfiles.Add(new PlayerProfile { CommunityId = Stranger, Name = "Visitor", FirstSeenAtUtc = DateTime.UtcNow, LastSeenAtUtc = DateTime.UtcNow, SteamLevel = 7, VacBans = 0, GameBans = 0 });

            await dbContext.SaveChangesAsync();
        });

        var response = await client.GetAsync($"/player/{Stranger}");
        var page = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<span class=\"steam-level\">7</span>", page);
        Assert.Contains("<span class=\"steam-bans \">0</span>", page);
        Assert.AreEqual(2, Count(page, "Not available"));
    }

    private static RoutingHttpMessageHandler SteamHandler()
    {
        return new RoutingHttpMessageHandler(uri =>
        {
            var path = uri.AbsolutePath;
            var query = Uri.UnescapeDataString(uri.Query);

            if (path.Contains("GetPlayerSummaries"))
                return Json($$$"""{"response":{"players":[{"steamid":"{{{Open}}}","personaname":"Open Persona","timecreated":1272672000},{"steamid":"{{{Hidden}}}","personaname":"Hidden Persona"}]}}""");

            if (path.Contains("GetPlayerBans"))
                return Json($$$"""{"players":[{"SteamId":"{{{Open}}}","NumberOfVACBans":0,"NumberOfGameBans":0,"DaysSinceLastBan":0},{"SteamId":"{{{Hidden}}}","NumberOfVACBans":2,"NumberOfGameBans":1,"DaysSinceLastBan":30}]}""");

            if (path.Contains("GetOwnedGames"))
                return Json(query.Contains($"steamid={Open}") ? """{"response":{"game_count":1,"games":[{"appid":550,"playtime_forever":6000,"playtime_2weeks":90}]}}""" : """{"response":{}}""");

            if (path.Contains("GetSteamLevel"))
                return Json(query.Contains($"steamid={Open}") ? """{"response":{"player_level":42}}""" : """{"response":{}}""");

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });
    }

    private static HttpResponseMessage Json(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };
    }

    private static WebApplicationFactory<Program> WithSteam(HubWebFactory factory, RoutingHttpMessageHandler handler)
    {
        return factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddHttpClient(AppConsts.SteamHttpClient).ConfigurePrimaryHttpMessageHandler(() => handler)));
    }

    private static async Task<long> StartAsync(HubWebFactory factory)
    {
        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        var now = DateTime.UtcNow;
        var night = new DateTime(now.Year, now.Month, Math.Min(now.Day, 20), 12, 0, 0);

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", night, 1, 300, 200));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", night.AddMinutes(15), 2, 600, 500));

        var ranking = await factory.RunAsync(services => services.GetRequiredService<IRankingService>().CurrentAsync());

        return ranking.First(player => player.CommunityId != StatisticsBuilder.PlayerA1).CommunityId;
    }

    private static Task SetSteamAsync(HubWebFactory factory, long communityId)
    {
        return factory.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();
            var profile = await dbContext.PlayerProfiles.SingleAsync(p => p.CommunityId == communityId);

            profile.Left4Dead2Minutes = 6000;
            profile.Left4Dead2RecentMinutes = 90;
            profile.AccountCreatedAtUtc = new DateTime(2010, 5, 1, 12, 0, 0);
            profile.SteamLevel = 42;
            profile.VacBans = 1;
            profile.GameBans = 0;
            profile.DaysSinceLastBan = 100;
            profile.SteamUpdatedAtUtc = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();
        });
    }

    private static Task AddProfilesAsync(WebApplicationFactory<Program> application, params long[] communityIds)
    {
        return RunAsync(application, async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            foreach (var communityId in communityIds)
                dbContext.PlayerProfiles.Add(new PlayerProfile { CommunityId = communityId, FirstSeenAtUtc = DateTime.UtcNow, LastSeenAtUtc = DateTime.UtcNow });

            return await dbContext.SaveChangesAsync();
        });
    }

    private static Task<bool> SaveKeyAsync(WebApplicationFactory<Program> application, string key)
    {
        return RunAsync(application, async services =>
        {
            var settings = services.GetRequiredService<ISettingsService>();
            var site = await settings.GetAsync<SiteSettings>();

            site.SteamApiKey = key;
            await settings.SaveAsync(site);

            return true;
        });
    }

    private static Task<PlayerProfile> ProfileAsync(WebApplicationFactory<Program> application, long communityId)
    {
        return RunAsync(application, services => services.GetRequiredService<HubDbContext>().PlayerProfiles.AsNoTracking().SingleAsync(p => p.CommunityId == communityId));
    }

    private static async Task<T> RunAsync<T>(WebApplicationFactory<Program> application, Func<IServiceProvider, Task<T>> action)
    {
        using var scope = application.Services.CreateScope();

        return await action(scope.ServiceProvider);
    }

    private static int Count(string text, string value)
    {
        return text.Split(value).Length - 1;
    }
}
