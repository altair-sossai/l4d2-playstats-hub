using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Core.DependencyInjection;

public static class CoreInjection
{
    public static IServiceCollection AddHubCore(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHttpClient();

        services.AddSingleton<HubState>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IGameServerPresence, GameServerPresence>();
        services.AddSingleton<Modules.Live.Services.ILiveServerStore, Modules.Live.Services.LiveServerStore>();

        services.Scan(scan => scan
            .FromAssemblyOf<HubState>()
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service", StringComparison.Ordinal)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
