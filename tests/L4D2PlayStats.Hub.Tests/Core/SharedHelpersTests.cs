using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class SharedHelpersTests
{
    [TestMethod]
    [DataRow(null, null)]
    [DataRow("", null)]
    [DataRow("   ", null)]
    [DataRow("  Ellis  ", "Ellis")]
    [DataRow("Rochelle", "Roche")]
    [DataRow("  Coach Bill  ", "Coach")]
    public void ClippingTrimsDropsEmptyTextAndCutsTheRest(string? value, string? expected)
    {
        Assert.AreEqual(expected, Text.Clip(value, 5));
    }

    [TestMethod]
    [DataRow("https://example.com", true, true)]
    [DataRow("http://example.com", false, true)]
    [DataRow("ftp://example.com", false, false)]
    [DataRow("javascript:alert(1)", false, false)]
    [DataRow("/relative/path", false, false)]
    [DataRow("", false, false)]
    [DataRow(null, false, false)]
    public void TheSchemesOfAnAddressAreChecked(string? value, bool https, bool http)
    {
        Assert.AreEqual(https, SafeUrl.IsHttps(value));
        Assert.AreEqual(http, SafeUrl.IsHttp(value));
    }

    [TestMethod]
    public void TheCleanAddressesAreTrimmedOrDropped()
    {
        Assert.AreEqual("https://example.com", SafeUrl.Https("  https://example.com  "));
        Assert.IsNull(SafeUrl.Https("http://example.com"));
        Assert.AreEqual("http://example.com", SafeUrl.Http(" http://example.com"));
        Assert.IsNull(SafeUrl.Http("example.com"));
    }

    [TestMethod]
    public async Task TheWriteLockIsHeldUntilItIsDisposed()
    {
        var state = new HubState();

        await using (await state.LockAsync())
        {
            Assert.AreEqual(0, state.WriteLock.CurrentCount);
            Assert.IsNull(await state.TryLockAsync(TimeSpan.FromMilliseconds(10)));
        }

        Assert.AreEqual(1, state.WriteLock.CurrentCount);

        var second = await state.TryLockAsync(TimeSpan.FromMilliseconds(10));

        Assert.IsNotNull(second);

        await second.DisposeAsync();
        await second.DisposeAsync();

        Assert.AreEqual(1, state.WriteLock.CurrentCount);
    }

    [TestMethod]
    public async Task TheWriteLockIsReleasedWhenTheWorkFails()
    {
        var state = new HubState();

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
        {
            await using (await state.LockAsync())
                throw new InvalidOperationException();
        });

        Assert.AreEqual(1, state.WriteLock.CurrentCount);
    }

    [TestMethod]
    public async Task TheGatesAndTheSeasonLockAreIndependent()
    {
        var state = new HubState();

        await using (await state.SeasonLockAsync())
        await using (await state.GateAsync("ranking"))
        {
            Assert.AreEqual(0, state.SeasonLock.CurrentCount);
            Assert.AreEqual(0, state.Gate("ranking").CurrentCount);
            Assert.AreEqual(1, state.WriteLock.CurrentCount);
        }

        Assert.AreEqual(1, state.SeasonLock.CurrentCount);
        Assert.AreEqual(1, state.Gate("ranking").CurrentCount);
    }

    [TestMethod]
    public async Task TheSiteClockUsesTheTimeZoneOfTheSite()
    {
        using var hub = new TestHub();
        var utc = new DateTime(2026, 7, 1, 15, 0, 0, DateTimeKind.Utc);

        var before = await hub.RunAsync(services => services.GetRequiredService<ISiteClock>().ToLocalAsync(utc));

        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new SiteSettings { TimeZone = "America/Sao_Paulo" }));

        var zone = await hub.RunAsync(services => services.GetRequiredService<ISiteClock>().ZoneAsync());
        var after = await hub.RunAsync(services => services.GetRequiredService<ISiteClock>().ToLocalAsync(utc));

        Assert.AreEqual(utc.Hour, before.Hour);
        Assert.AreEqual("America/Sao_Paulo", zone.Id);
        Assert.AreEqual(12, after.Hour);
    }

    [TestMethod]
    public async Task AnUnknownTimeZoneFallsBackToUtc()
    {
        using var hub = new TestHub();

        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new SiteSettings { TimeZone = "Nowhere/Unknown" }));

        var zone = await hub.RunAsync(services => services.GetRequiredService<ISiteClock>().ZoneAsync());

        Assert.AreEqual(TimeZoneInfo.Utc, zone);
    }
}
