using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Administrators.Services;
using L4D2PlayStats.Hub.Core.Modules.Campaigns.Services;
using L4D2PlayStats.Hub.Core.Modules.GameModes.Services;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Extensions;

public static class Hosting
{
    public static IServiceCollection AddHubDatabase(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var paths = new HubPaths(configuration, environment);
        var section = configuration.GetSection(CoreConsts.DatabaseSection);
        var provider = section["Provider"] ?? CoreConsts.SqliteProvider;
        var connectionString = HubDatabase.PrepareConnectionString(provider, section["ConnectionString"] ?? CoreConsts.DefaultConnectionString, paths.DataDirectory);

        services.AddSingleton(paths);
        services.AddDataProtection().SetApplicationName(AppConsts.ApplicationName).PersistKeysToFileSystem(new DirectoryInfo(paths.Keys));
        services.AddDbContext<HubDbContext>(options => HubDatabase.Configure(options, provider, connectionString));

        return services;
    }

    public static async Task InitializeHubAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<HubDbContext>();

        if (dbContext.Database.GetMigrations().Any())
            await dbContext.Database.MigrateAsync();
        else
            await dbContext.Database.EnsureCreatedAsync();

        await services.GetRequiredService<ICampaignService>().SeedAsync();
        await services.GetRequiredService<IGameModeService>().SeedAsync();
        await services.GetRequiredService<ISkinCatalogService>().SeedAsync();

        var token = await services.GetRequiredService<ISetupService>().EnsureTokenAsync();

        var paths = services.GetRequiredService<HubPaths>();

        if (string.IsNullOrEmpty(token))
        {
            File.Delete(paths.SetupCode);
            return;
        }

        await File.WriteAllTextAsync(paths.SetupCode, token);
        app.Logger.LogWarning("Setup is pending. Open /setup and use this setup code: {SetupToken}", token);
    }
}
