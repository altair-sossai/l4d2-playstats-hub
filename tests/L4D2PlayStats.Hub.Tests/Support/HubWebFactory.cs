using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Administrators.Services;
using L4D2PlayStats.Hub.Core.Modules.Servers.Models;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.RegularExpressions;

namespace L4D2PlayStats.Hub.Tests.Support;

public class HubWebFactory : WebApplicationFactory<Program>
{
    public const long Administrator = 76561198000000100;
    public const long Player = 76561198000000200;
    public const string UserHeader = "X-Test-User";

    private readonly TestDatabase _database = new();
    private readonly string _dataDirectory = Path.Combine(Path.GetTempPath(), "l4d2-playstats-hub-tests", Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Hub:DataDirectory", _dataDirectory);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.RemoveAll<DbContextOptions<HubDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<HubDbContext>>();
            services.AddDbContext<HubDbContext>(_database.Configure);

            services
                .AddAuthentication(TestAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.SchemeName, _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultScheme = TestAuthenticationHandler.SchemeName;
                options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
                options.DefaultForbidScheme = TestAuthenticationHandler.SchemeName;
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _database.Dispose();

        if (Directory.Exists(_dataDirectory))
            Directory.Delete(_dataDirectory, true);
    }

    public HttpClient Client(long? user = null)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        if (user != null)
            client.DefaultRequestHeaders.Add(UserHeader, user.Value.ToString());

        return client;
    }

    public async Task<T> RunAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = Services.CreateScope();

        return await action(scope.ServiceProvider);
    }

    public async Task RunAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = Services.CreateScope();

        await action(scope.ServiceProvider);
    }

    public async Task CompleteSetupAsync()
    {
        await RunAsync(async services =>
        {
            await services.GetRequiredService<IAdministratorService>().AddAsync(Administrator, "Admin");
            await services.GetRequiredService<ISetupService>().CompleteAsync();
        });
    }

    public Task<GameServerCredentials> CreateServerAsync(string name = "Test Server")
    {
        return RunAsync(services => services.GetRequiredService<IGameServerService>().CreateAsync(name, null));
    }

    public Task SaveAsync<T>(T settings) where T : class, new()
    {
        return RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(settings));
    }

    public HttpClient ServerClient(GameServerCredentials credentials)
    {
        var client = Client();

        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", credentials.Token);

        return client;
    }

    public static Task<HttpResponseMessage> UploadAsync(HttpClient serverClient, StatisticsBuilder builder)
    {
        return serverClient.PostAsJsonAsync("/api/statistics", new { fileName = builder.FileName, content = builder.Content });
    }

    public static async Task<HttpResponseMessage> PostFormAsync(HttpClient client, string formPage, string action, Dictionary<string, string> fields)
    {
        var html = await client.GetStringAsync(formPage);
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"");

        var form = new Dictionary<string, string>(fields) { ["__RequestVerificationToken"] = match.Groups[1].Value };

        return await client.PostAsync(action, new FormUrlEncodedContent(form));
    }
}
