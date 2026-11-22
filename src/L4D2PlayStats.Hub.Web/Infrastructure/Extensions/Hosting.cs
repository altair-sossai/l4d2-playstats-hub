using System.Diagnostics;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Administrators.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Campaigns.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.GameModes.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Matches.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Secrets;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

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
        services.AddSingleton(SecretsKeyLoader.Load(configuration, paths));
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

        LogSecretsKey(app.Logger, services.GetRequiredService<SecretsKey>());

        var encrypted = await services.GetRequiredService<ISettingsEncryptionService>().EncryptPlainSecretsAsync();

        if (encrypted > 0)
            app.Logger.LogInformation("Encrypted the secrets of {Count} settings that were stored in plain text by an older version.", encrypted);

        await services.GetRequiredService<IForcedNameService>().LoadAsync();

        var started = Stopwatch.GetTimestamp();
        var registry = await services.GetRequiredService<IPlayerRegistryService>().BackfillAsync();

        if (registry.Created + registry.Updated > 0)
            app.Logger.LogInformation("Updated the names of {Updated} of {Profiles} players and created {Created} missing player profiles in {Elapsed} ms.", registry.Updated, registry.Profiles, registry.Created, (long)Stopwatch.GetElapsedTime(started).TotalMilliseconds);

        started = Stopwatch.GetTimestamp();
        var tankWipes = await services.GetRequiredService<IMatchBuildService>().BackfillTankWipesAsync();

        if (tankWipes > 0)
            app.Logger.LogInformation("Filled in the tank wipes of {Count} match players and statistics files in {Elapsed} ms.", tankWipes, (long)Stopwatch.GetElapsedTime(started).TotalMilliseconds);

        var ratings = services.GetRequiredService<ICommunityRatingService>();

        if (await ratings.NeedsRecalculationAsync())
        {
            var votes = await ratings.RecalculateAsync();

            await dbContext.SaveChangesAsync();
            app.Logger.LogInformation("Recalculated the community ratings from {Count} votes.", votes);
        }

        var token = await services.GetRequiredService<ISetupService>().EnsureTokenAsync();

        var paths = services.GetRequiredService<HubPaths>();

        if (string.IsNullOrEmpty(token))
        {
            File.Delete(paths.SetupCode);
            return;
        }

        await File.WriteAllTextAsync(paths.SetupCode, token);
        app.Logger.LogInformation("Setup is pending. Open /setup and use this setup code: {SetupToken}", token);
    }

    private static void LogSecretsKey(ILogger logger, SecretsKey key)
    {
        if (!key.IsAvailable)
        {
            logger.LogError("The secrets key from {Location} is not usable. {Problem} The site runs, but the secrets stored in the database (the Steam API key, the backup destination and the setup code) cannot be read or saved until the key is fixed. Set {FileSetting} to a key file the site can read, or {KeySetting} to the key itself, and restart.", key.Location, key.Error, AppConsts.SecretsKeyFileSetting, AppConsts.SecretsKeySettingName);
            return;
        }

        if (key.Created)
        {
            logger.LogWarning("Created a new secrets key in {Location}. The secrets stored in the database are encrypted with it, and it is not part of the backup zip: keep a copy of this file in a safe place.", key.Location);
            return;
        }

        logger.LogInformation("The secrets stored in the database are encrypted with the secrets key from {Location}.", key.Location);
    }
}
