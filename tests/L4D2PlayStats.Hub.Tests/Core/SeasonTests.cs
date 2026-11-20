using L4D2PlayStats.Hub.Core;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Seasons;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class SeasonTests
{
    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(2, 1)]
    [DataRow(3, 3)]
    [DataRow(4, 3)]
    [DataRow(5, 5)]
    [DataRow(6, 5)]
    [DataRow(7, 7)]
    [DataRow(8, 7)]
    [DataRow(9, 9)]
    [DataRow(10, 9)]
    [DataRow(11, 11)]
    [DataRow(12, 11)]
    public void TwoMonthSeasonsStartOnOddMonths(int month, int expectedStartMonth)
    {
        var start = SeasonCalendar.AlignedStart(new DateTime(2026, month, 15), 2);

        Assert.AreEqual(new DateTime(2026, expectedStartMonth, 1), start);
    }

    [TestMethod]
    public void FirstSeasonIsAlignedToTheCalendar()
    {
        var missing = SeasonCalendar.Missing([], new DateTime(2026, 11, 15, 22, 30, 0), 2);

        Assert.HasCount(1, missing);
        Assert.AreEqual(new DateTime(2026, 11, 1), missing[0].StartsAt);
        Assert.AreEqual(new DateTime(2027, 1, 1), missing[0].EndsAt);
    }

    [TestMethod]
    public void LaterDatesExtendForwardWithoutGaps()
    {
        var existing = new List<Season> { new() { StartsAt = new DateTime(2026, 11, 1), EndsAt = new DateTime(2027, 1, 1) } };

        var missing = SeasonCalendar.Missing(existing, new DateTime(2027, 6, 10), 2);

        Assert.HasCount(3, missing);
        Assert.AreEqual(new DateTime(2027, 1, 1), missing[0].StartsAt);
        Assert.AreEqual(new DateTime(2027, 5, 1), missing[2].StartsAt);
        Assert.IsTrue(missing[2].Contains(new DateTime(2027, 6, 10)));
    }

    [TestMethod]
    public void EarlierDatesExtendBackwardWithoutGaps()
    {
        var existing = new List<Season> { new() { StartsAt = new DateTime(2026, 11, 1), EndsAt = new DateTime(2027, 1, 1) } };

        var missing = SeasonCalendar.Missing(existing, new DateTime(2026, 6, 30, 23, 59, 0), 2);

        Assert.HasCount(3, missing);
        Assert.AreEqual(new DateTime(2026, 9, 1), missing[0].StartsAt);
        Assert.AreEqual(new DateTime(2026, 5, 1), missing[2].StartsAt);
        Assert.AreEqual(new DateTime(2026, 7, 1), missing[2].EndsAt);
    }

    [TestMethod]
    public void ChangingTheLengthOnlyAffectsNewSeasons()
    {
        var existing = new List<Season> { new() { StartsAt = new DateTime(2026, 11, 1), EndsAt = new DateTime(2027, 1, 1) } };

        var missing = SeasonCalendar.Missing(existing, new DateTime(2027, 2, 1), 3);

        Assert.HasCount(1, missing);
        Assert.AreEqual(new DateTime(2027, 1, 1), missing[0].StartsAt);
        Assert.AreEqual(new DateTime(2027, 4, 1), missing[0].EndsAt);
    }

    [TestMethod]
    public void TheEndOfASeasonIsExclusive()
    {
        var season = new Season { StartsAt = new DateTime(2026, 11, 1), EndsAt = new DateTime(2027, 1, 1) };

        Assert.IsTrue(season.Contains(new DateTime(2026, 11, 1)));
        Assert.IsTrue(season.Contains(new DateTime(2026, 12, 31, 23, 59, 59)));
        Assert.IsFalse(season.Contains(new DateTime(2027, 1, 1)));
    }

    [TestMethod]
    public async Task TheServiceCreatesEachSeasonOnlyOnce()
    {
        using var hub = new TestHub();

        var first = await hub.RunAsync(services => services.GetRequiredService<ISeasonService>().GetOrCreateAsync(new DateTime(2026, 11, 15)));
        var second = await hub.RunAsync(services => services.GetRequiredService<ISeasonService>().GetOrCreateAsync(new DateTime(2026, 12, 20)));
        var seasons = await hub.RunAsync(services => services.GetRequiredService<ISeasonService>().ListAsync());

        Assert.AreEqual(first.Id, second.Id);
        Assert.HasCount(1, seasons);
    }

    [TestMethod]
    public async Task TheCurrentSeasonFollowsTheSiteTimeZone()
    {
        using var hub = new TestHub();

        hub.Clock.UtcNow = new DateTime(2027, 1, 1, 1, 30, 0, DateTimeKind.Utc);

        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new SiteSettings { TimeZone = "America/Sao_Paulo" }));

        var season = await hub.RunAsync(services => services.GetRequiredService<ISeasonService>().GetCurrentAsync());

        Assert.AreEqual(new DateTime(2026, 11, 1), season.StartsAt);
    }

    [TestMethod]
    public async Task TheSeasonLengthComesFromTheSettings()
    {
        using var hub = new TestHub();

        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new SeasonSettings { LengthInMonths = 6 }));

        var season = await hub.RunAsync(services => services.GetRequiredService<ISeasonService>().GetOrCreateAsync(new DateTime(2026, 11, 15)));

        Assert.AreEqual(new DateTime(2026, 7, 1), season.StartsAt);
        Assert.AreEqual(new DateTime(2027, 1, 1), season.EndsAt);
    }

    [TestMethod]
    public async Task ASeasonCreatedWhileAnotherRequestWaitsIsReusedInsteadOfCreatedTwice()
    {
        using var hub = new TestHub();

        var date = new DateTime(2026, 11, 15);
        var hubState = hub.Scope().ServiceProvider.GetRequiredService<HubState>();

        await hubState.SeasonLock.WaitAsync();

        using var waiting = hub.Scope();
        var pending = waiting.ServiceProvider.GetRequiredService<ISeasonService>().GetOrCreateAsync(date);

        await Task.Delay(200);

        await hub.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            dbContext.Seasons.AddRange(SeasonCalendar.Missing([], date, AppConsts.DefaultSeasonLengthInMonths));

            return await dbContext.SaveChangesAsync();
        });

        hubState.SeasonLock.Release();

        var season = await pending;
        var seasons = await hub.RunAsync(services => services.GetRequiredService<HubDbContext>().Seasons.CountAsync());

        Assert.IsTrue(season.Contains(date));
        Assert.AreEqual(1, seasons);
    }
}
