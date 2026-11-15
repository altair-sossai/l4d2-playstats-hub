using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.DependencyInjection;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Campaigns;
using L4D2PlayStats.Hub.Core.Modules.GameModes;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Support;

public class FakeClock : IClock
{
    public DateTime UtcNow { get; set; } = new(2026, 11, 15, 12, 0, 0, DateTimeKind.Utc);

    public void Advance(TimeSpan time)
    {
        UtcNow = UtcNow.Add(time);
    }
}

public sealed class TestHub : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _services;

    public TestHub()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddHubCore();
        services.AddSingleton<IClock>(Clock);
        services.AddDbContext<HubDbContext>(options => options.UseSqlite(_connection));

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
        _connection.Dispose();
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
