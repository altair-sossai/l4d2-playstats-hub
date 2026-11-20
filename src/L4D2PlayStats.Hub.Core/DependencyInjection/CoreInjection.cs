using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services;
using Microsoft.Extensions.DependencyInjection;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;

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
        services.AddSingleton<Modules.Live.Services.Contracts.ILiveSettings, Modules.Live.Services.LiveSettings>();
        services.AddSingleton<Modules.Live.Services.Contracts.ILiveServerStore, Modules.Live.Services.LiveServerStore>();
        services.AddSingleton<Modules.Settings.Services.Contracts.ISecretProtector, Modules.Settings.Services.SecretProtector>();

        services.Scan(scan => scan
            .FromAssemblyOf<HubState>()
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service", StringComparison.Ordinal)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
