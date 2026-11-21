using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Services;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
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
        services.AddSingleton<ILiveSettings, LiveSettings>();
        services.AddSingleton<ILiveServerStore, LiveServerStore>();
        services.AddSingleton<ISecretProtector, SecretProtector>();
        services.AddSingleton<IForcedNameStore, ForcedNameStore>();
        services.AddScoped<ISiteClock, SiteClock>();

        services.Scan(scan => scan
            .FromAssemblyOf<HubState>()
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service", StringComparison.Ordinal)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
