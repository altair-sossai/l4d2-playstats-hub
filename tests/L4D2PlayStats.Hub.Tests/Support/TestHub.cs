using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.DependencyInjection;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Campaigns.Services;
using L4D2PlayStats.Hub.Core.Modules.GameModes.Services;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Servers.Models;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Models;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Support;

public sealed class TestHub : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly ServiceProvider _services;

    public TestHub()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddHubCore();
        services.AddSingleton<IClock>(Clock);
        services.AddSingleton(new SecretsKey(RandomNumberGenerator.GetBytes(CoreConsts.SecretKeyBytes), "test", null, false, null));
        services.AddDbContext<HubDbContext>(_database.Configure);

        _services = services.BuildServiceProvider();

        using var scope = Scope();

        scope.ServiceProvider.GetRequiredService<HubDbContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<ICampaignService>().SeedAsync().GetAwaiter().GetResult();
        scope.ServiceProvider.GetRequiredService<IGameModeService>().SeedAsync().GetAwaiter().GetResult();
    }

    public FakeClock Clock { get; } = new();

    public void Dispose()
    {
        _services.Dispose();
        _database.Dispose();
    }

    public IServiceScope Scope()
    {
        return _services.CreateScope();
    }

    public async Task<T> RunAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = Scope();

        return await action(scope.ServiceProvider);
    }

    public async Task RunAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = Scope();

        await action(scope.ServiceProvider);
    }

    public Task<GameServerCredentials> CreateServerAsync(string name = "Test Server")
    {
        return RunAsync(services => services.GetRequiredService<IGameServerService>().CreateAsync(name, null));
    }

    public Task<UploadResult> UploadAsync(GameServer server, StatisticsBuilder builder)
    {
        return RunAsync(async services =>
        {
            var gameServer = await services.GetRequiredService<IGameServerService>().FindAsync(server.Id);

            return await services.GetRequiredService<IStatisticsService>().UploadAsync(gameServer!, builder.FileName, builder.Content);
        });
    }
}
