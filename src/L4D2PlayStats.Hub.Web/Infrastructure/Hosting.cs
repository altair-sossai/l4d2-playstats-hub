using System.Globalization;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Administrators;
using L4D2PlayStats.Hub.Core.Modules.Campaigns;
using L4D2PlayStats.Hub.Core.Modules.GameModes;
using L4D2PlayStats.Hub.Core.Modules.Seasons;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using Microsoft.AspNetCore.Localization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Infrastructure;

public static class Hosting
{
    public static IServiceCollection AddHubDatabase(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var section = configuration.GetSection(CoreConsts.DatabaseSection);
        var provider = section["Provider"] ?? CoreConsts.SqliteProvider;
        var connectionString = section["ConnectionString"] ?? CoreConsts.DefaultConnectionString;

        if (!provider.Equals(CoreConsts.SqliteProvider, StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Database provider '{provider}' is not supported yet.");

        var builder = new SqliteConnectionStringBuilder(connectionString);

        if (!string.IsNullOrEmpty(builder.DataSource) && builder.DataSource != ":memory:" && !Path.IsPathRooted(builder.DataSource))
            builder.DataSource = Path.Combine(environment.ContentRootPath, builder.DataSource);

        var directory = Path.GetDirectoryName(builder.DataSource);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        services.AddDbContext<HubDbContext>(options => options.UseSqlite(builder.ConnectionString, sqlite => sqlite.MigrationsAssembly("L4D2PlayStats.Hub.Migrations.Sqlite")));

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

        var token = await services.GetRequiredService<ISetupService>().EnsureTokenAsync();

        if (!string.IsNullOrEmpty(token))
            app.Logger.LogWarning("Setup is pending. Open /setup and use this setup code: {SetupToken}", token);
    }
}

public class SiteCultureProvider : RequestCultureProvider
{
    public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var settingsService = httpContext.RequestServices.GetRequiredService<ISettingsService>();
        var site = await settingsService.GetAsync<SiteSettings>(httpContext.RequestAborted);

        return AppConsts.SupportedCultures.Contains(site.DefaultCulture) ? new ProviderCultureResult(site.DefaultCulture) : null;
    }
}

public class SeasonBackgroundService(IServiceProvider serviceProvider, ILogger<SeasonBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(AppConsts.SeasonCheckIntervalInMinutes));

        do
        {
            try
            {
                using var scope = serviceProvider.CreateScope();

                await scope.ServiceProvider.GetRequiredService<ISeasonService>().GetCurrentAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Could not ensure the current season");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}

public static class Cultures
{
    public static List<CultureInfo> Supported()
    {
        return [.. AppConsts.SupportedCultures.Select(name => new CultureInfo(name))];
    }
}
