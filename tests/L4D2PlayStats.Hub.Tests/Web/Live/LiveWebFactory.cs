using System.Text;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Web.Live;

public class LiveWebFactory : HubWebFactory
{
    public FakeClock Clock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services => services.AddSingleton<IClock>(Clock));
    }

    public void PassTheDelay()
    {
        Clock.Advance(TimeSpan.FromSeconds(CoreConsts.LiveDelayInSeconds));
    }

    public Task EnableAsync(bool liveServer = true, bool siteChat = true, bool remoteCommands = true)
    {
        return SaveAsync(new ModuleSettings { LiveServer = liveServer, SiteChat = siteChat, RemoteCommands = remoteCommands });
    }

    public static StringContent Json(string json)
    {
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
}
