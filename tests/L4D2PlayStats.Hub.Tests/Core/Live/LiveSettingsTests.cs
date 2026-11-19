using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Live.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Core.Live;

[TestClass]
public class LiveSettingsTests
{
    [TestMethod]
    public async Task TheDelayComesFromTheSettings()
    {
        using var hub = new TestHub();

        await hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new LiveServerSettings { DelayInSeconds = 4 }));

        using var scope = hub.Scope();

        var delay = scope.ServiceProvider.GetRequiredService<ILiveSettings>().Current.DelayInSeconds;

        Assert.AreEqual(4, delay);
    }

    [TestMethod]
    public async Task ANewDelayAppliesWithoutARestart()
    {
        using var hub = new TestHub();
        using var scope = hub.Scope();

        var live = scope.ServiceProvider.GetRequiredService<ILiveDataService>();
        var settings = scope.ServiceProvider.GetRequiredService<ILiveSettings>();
        var credentials = await hub.CreateServerAsync();

        settings.Set(new LiveServerSettings { DelayInSeconds = 0 });
        live.UpdateScoreboard(credentials.Server, new LiveScoreboard { SurvivorScore = 10, InfectedScore = 0 });

        var immediate = live.Snapshot(credentials.Server).Scoreboard?.SurvivorScore;

        settings.Set(new LiveServerSettings { DelayInSeconds = 30 });
        live.UpdateScoreboard(credentials.Server, new LiveScoreboard { SurvivorScore = 20, InfectedScore = 0 });

        var held = live.Snapshot(credentials.Server).Scoreboard?.SurvivorScore;

        hub.Clock.Advance(TimeSpan.FromSeconds(30));

        var released = live.Snapshot(credentials.Server).Scoreboard?.SurvivorScore;

        Assert.AreEqual(10, immediate);
        Assert.AreEqual(10, held);
        Assert.AreEqual(20, released);
    }

    [TestMethod]
    public void TheLiveSettingsStayWithinTheirRanges()
    {
        var settings = new LiveServerSettings { DelayInSeconds = 99999, RefreshInMilliseconds = 10, OnlineWindowInSeconds = 1, RemoteCommandExpirationInMinutes = 999 };

        settings.Normalize();

        Assert.AreEqual(CoreConsts.LiveMaxDelayInSeconds, settings.DelayInSeconds);
        Assert.AreEqual(CoreConsts.LiveMinRefreshInMilliseconds, settings.RefreshInMilliseconds);
        Assert.AreEqual(CoreConsts.ServerMinOnlineWindowInSeconds, settings.OnlineWindowInSeconds);
        Assert.AreEqual(CoreConsts.RemoteCommandMaxExpirationInMinutes, settings.RemoteCommandExpirationInMinutes);
        Assert.AreEqual(CoreConsts.LiveDelayInSeconds, new LiveServerSettings().DelayInSeconds);
    }
}
