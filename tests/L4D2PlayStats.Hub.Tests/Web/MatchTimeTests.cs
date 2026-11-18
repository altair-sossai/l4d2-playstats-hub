using System.Globalization;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Matches.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class MatchTimeTests
{
    private const string SiteZone = "America/Sao_Paulo";
    private static readonly DateTime ServerClock = new(2026, 3, 10, 21, 0, 0);
    private static readonly CultureInfo English = new("en-US");

    [TestMethod]
    public async Task AMatchWithoutAUtcStartUsesTheFileDateAsTheGameServerClock()
    {
        using var factory = await FactoryAsync(true);

        var match = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.AsNoTracking().SingleAsync());
        var result = await factory.RunAsync(services => services.GetRequiredService<IMatchResultService>().FindAsync(match.Id));

        Assert.AreEqual(ServerClock, match.FirstFileDate.AddMilliseconds(-match.FirstFileDate.Millisecond));
        Assert.IsFalse(result!.MatchStartIsUtc);
        Assert.AreEqual(match.FirstFileDate, result.MatchStart);
        Assert.IsNull(result.MatchElapsed);
    }

    [TestMethod]
    public async Task TheCardShowsTheGameServerClockWhenTheMatchHasNoUtcStart()
    {
        using var factory = await FactoryAsync(true);
        using var client = factory.Client();

        var match = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.AsNoTracking().SingleAsync());
        var expected = Card(ServerClock);
        var shifted = Card(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(ServerClock, DateTimeKind.Utc), TimeZoneInfo.FindSystemTimeZoneById(SiteZone)));

        foreach (var page in new[] { "/matches", $"/matches/{match.Id}", $"/player/{StatisticsBuilder.PlayerA1}?period=all-time" })
        {
            var html = await client.GetStringAsync(page);

            Assert.Contains(expected, html, page);
            Assert.DoesNotContain(shifted, html, page);
            Assert.DoesNotContain("/imgs/icons/clock.png", html, page);
        }
    }

    [TestMethod]
    public async Task TheCardConvertsTheRoundTimeFromUtcToTheSiteTimeZone()
    {
        using var factory = await FactoryAsync(false);
        using var client = factory.Client();

        var match = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.AsNoTracking().SingleAsync());
        var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(match.StartedAtUtc!.Value, DateTimeKind.Utc), TimeZoneInfo.FindSystemTimeZoneById(SiteZone));
        var html = await client.GetStringAsync($"/matches/{match.Id}");

        Assert.Contains(Card(local), html);
        Assert.Contains("/imgs/icons/clock.png", html);
    }

    private static string Card(DateTime value)
    {
        return string.Format(English, "{0:d} at {0:t}", value);
    }

    private static async Task<HubWebFactory> FactoryAsync(bool withoutStart)
    {
        var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new SiteSettings { TimeZone = SiteZone });

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", ServerClock, 1, 300, 200));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", ServerClock.AddMinutes(15), 2, 600, 500));

        if (withoutStart)
            await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.ExecuteUpdateAsync(setters => setters.SetProperty(match => match.StartedAtUtc, (DateTime?)null)));

        return factory;
    }
}
