using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Administrators;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace L4D2PlayStats.Hub.Tests.Support;

public class HubWebFactory : WebApplicationFactory<Program>
{
    public const long Administrator = 76561198000000100;
    public const long Player = 76561198000000200;
    public const string UserHeader = "X-Test-User";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.RemoveAll<DbContextOptions<HubDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<HubDbContext>>();
            services.AddDbContext<HubDbContext>(options => options.UseSqlite(_connection));

            services
                .AddAuthentication(TestAuthenticationHandler.Scheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.Scheme, _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultScheme = TestAuthenticationHandler.Scheme;
                options.DefaultAuthenticateScheme = TestAuthenticationHandler.Scheme;
                options.DefaultChallengeScheme = TestAuthenticationHandler.Scheme;
                options.DefaultForbidScheme = TestAuthenticationHandler.Scheme;
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _connection.Dispose();
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

public class TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string Scheme = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var user = Request.Headers[HubWebFactory.UserHeader].ToString();

        if (string.IsNullOrEmpty(user))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user), new Claim(ClaimTypes.Name, $"Player {user}") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme));

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme)));
    }
}
