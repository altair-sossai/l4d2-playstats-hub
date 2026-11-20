using System.Net;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Web.Secrets;

[TestClass]
public class SecretsKeyTests
{
    private const string SteamApiKey = "STEAMKEY0123456789ABCDEF";
    private const string BackupPage = "/admin/backup";

    [TestMethod]
    public async Task TheSiteCreatesAKeyFileOutsideTheDataFolderWhenThereIsNone()
    {
        using var factory = new HubWebFactory();

        var logs = new LogCollector();

        using var application = WithLogs(factory, logs);

        var key = application.Services.GetRequiredService<SecretsKey>();
        var text = await File.ReadAllTextAsync(factory.SecretsKeyFile);

        Assert.IsTrue(key.IsAvailable);
        Assert.IsTrue(key.Created);
        Assert.AreEqual(factory.SecretsKeyFile, key.File);
        CollectionAssert.AreEqual(key.Value, Convert.FromBase64String(text));
        Assert.HasCount(CoreConsts.SecretKeyBytes, key.Value!);
        Assert.IsFalse(Path.GetFullPath(factory.SecretsKeyFile).StartsWith(Path.GetFullPath(factory.DataDirectory), StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(logs.Entries.Any(entry => entry.Level == LogLevel.Warning && entry.Message.Contains(factory.SecretsKeyFile)));
        Assert.IsFalse(logs.Entries.Any(entry => entry.Message.Contains(text)));

        if (!OperatingSystem.IsWindows())
            Assert.AreEqual(UnixFileMode.UserRead | UnixFileMode.UserWrite, File.GetUnixFileMode(factory.SecretsKeyFile));
    }

    [TestMethod]
    public async Task AnExistingKeyFileIsUsedAndKept()
    {
        using var factory = new HubWebFactory();

        var value = RandomNumberGenerator.GetBytes(CoreConsts.SecretKeyBytes);
        var text = Convert.ToBase64String(value) + "\n";

        Directory.CreateDirectory(Path.GetDirectoryName(factory.SecretsKeyFile)!);
        await File.WriteAllTextAsync(factory.SecretsKeyFile, text);

        await factory.SaveAsync(new SiteSettings { SteamApiKey = SteamApiKey });

        var raw = await RawSteamApiKeyAsync(factory);
        var key = factory.Services.GetRequiredService<SecretsKey>();

        Assert.IsFalse(key.Created);
        Assert.AreEqual(text, await File.ReadAllTextAsync(factory.SecretsKeyFile));
        Assert.IsTrue(SecretCipher.TryDecrypt(value, raw, out var plain));
        Assert.AreEqual(SteamApiKey, plain);
    }

    [TestMethod]
    public async Task WithAnInvalidKeyFileTheSiteStartsLogsTheProblemAndStoresNoPlainSecret()
    {
        using var factory = new HubWebFactory();

        Directory.CreateDirectory(Path.GetDirectoryName(factory.SecretsKeyFile)!);
        await File.WriteAllTextAsync(factory.SecretsKeyFile, "not a key");

        var logs = new LogCollector();

        using var application = WithLogs(factory, logs);
        using var client = application.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var ping = await client.GetAsync("/api/ping");
        var key = application.Services.GetRequiredService<SecretsKey>();

        await Run(application, services => services.GetRequiredService<ISettingsService>().SaveAsync(new SiteSettings { Name = "Site", SteamApiKey = SteamApiKey }));

        var site = await Run(application, services => services.GetRequiredService<ISettingsService>().GetAsync<SiteSettings>());
        var raw = await Run(application, async services => (await services.GetRequiredService<HubDbContext>().Settings.AsNoTracking().SingleAsync(setting => setting.Key == nameof(SiteSettings))).Value);

        Assert.AreEqual(HttpStatusCode.OK, ping.StatusCode);
        Assert.IsFalse(key.IsAvailable);
        Assert.AreEqual("not a key", await File.ReadAllTextAsync(factory.SecretsKeyFile));
        Assert.IsTrue(logs.Entries.Any(entry => entry.Level == LogLevel.Error && entry.Message.Contains(factory.SecretsKeyFile) && entry.Message.Contains("base64")));
        Assert.DoesNotContain(SteamApiKey, raw);
        Assert.AreEqual("Site", site.Name);
        Assert.IsNull(site.SteamApiKey);
        Assert.IsFalse(logs.Entries.Any(entry => entry.Message.Contains(SteamApiKey)));
    }

    [TestMethod]
    public async Task TheKeyCanComeFromTheSecretsKeySetting()
    {
        using var factory = new HubWebFactory();

        var value = RandomNumberGenerator.GetBytes(CoreConsts.SecretKeyBytes);

        using var application = factory.WithWebHostBuilder(builder => builder.UseSetting("Hub:SecretsKey", Convert.ToBase64String(value)));

        var key = application.Services.GetRequiredService<SecretsKey>();

        await Run(application, services => services.GetRequiredService<ISettingsService>().SaveAsync(new SiteSettings { SteamApiKey = SteamApiKey }));

        var raw = await Run(application, async services => (await services.GetRequiredService<HubDbContext>().Settings.AsNoTracking().SingleAsync(setting => setting.Key == nameof(SiteSettings))).Value);

        Assert.AreEqual("Hub__SecretsKey", key.Location);
        Assert.IsNull(key.File);
        Assert.IsFalse(File.Exists(factory.SecretsKeyFile));
        Assert.Contains(CoreConsts.SecretPrefix, raw);
        Assert.DoesNotContain(SteamApiKey, raw);
    }

    [TestMethod]
    public void AnInvalidSecretsKeySettingLeavesTheKeyUnavailable()
    {
        using var factory = new HubWebFactory();
        using var application = factory.WithWebHostBuilder(builder => builder.UseSetting("Hub:SecretsKey", "c2hvcnQ="));

        var key = application.Services.GetRequiredService<SecretsKey>();

        Assert.IsFalse(key.IsAvailable);
        Assert.AreEqual("Hub__SecretsKey", key.Location);
        Assert.IsNotNull(key.Error);
    }

    [TestMethod]
    public void ByDefaultTheKeyFileIsNextToTheDataFolder()
    {
        using var factory = new HubWebFactory();
        using var application = factory.WithWebHostBuilder(builder => builder.UseSetting("Hub:SecretsKeyFile", string.Empty));

        var paths = application.Services.GetRequiredService<HubPaths>();
        var expected = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(factory.DataDirectory))!, "secrets.key");

        Assert.AreEqual(expected, paths.SecretsKeyFile);
        Assert.IsTrue(File.Exists(expected));
    }

    [TestMethod]
    public async Task PlainSecretsLeftByAnOlderVersionAreEncryptedWhenTheSiteStarts()
    {
        using var factory = new HubWebFactory();

        await factory.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            dbContext.Settings.Add(new Setting { Key = nameof(SiteSettings), Value = $"{{\"Name\":\"Old site\",\"SteamApiKey\":\"{SteamApiKey}\"}}" });

            await dbContext.SaveChangesAsync();
        });

        var logs = new LogCollector();

        using var restarted = WithLogs(factory, logs);

        var raw = await Run(restarted, async services => (await services.GetRequiredService<HubDbContext>().Settings.AsNoTracking().SingleAsync(setting => setting.Key == nameof(SiteSettings))).Value);
        var site = await Run(restarted, services => services.GetRequiredService<ISettingsService>().GetAsync<SiteSettings>());

        Assert.DoesNotContain(SteamApiKey, raw);
        Assert.Contains(CoreConsts.SecretPrefix, raw);
        Assert.AreEqual(SteamApiKey, site.SteamApiKey);
        Assert.AreEqual("Old site", site.Name);
        Assert.IsTrue(logs.Entries.Any(entry => entry.Level == LogLevel.Information && entry.Message.Contains("Encrypted the secrets of 1 settings")));
    }

    [TestMethod]
    public async Task TheBackupPageWarnsThatTheKeyIsNotInTheZip()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();

        var html = await client.GetStringAsync(BackupPage);

        Assert.Contains("backup-secrets-key", html);
        Assert.Contains(HtmlEncoder.Default.Encode(factory.SecretsKeyFile), html);
        Assert.Contains("That key is not in the backup zip", html);
        Assert.DoesNotContain("backup-secrets-key-error", html);
    }

    [TestMethod]
    public async Task TheBackupPageSaysWhenTheKeyCouldNotBeLoaded()
    {
        using var factory = new HubWebFactory();

        Directory.CreateDirectory(Path.GetDirectoryName(factory.SecretsKeyFile)!);
        await File.WriteAllTextAsync(factory.SecretsKeyFile, "not a key");

        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();

        var html = await client.GetStringAsync(BackupPage);

        Assert.Contains("backup-secrets-key-error", html);
        Assert.Contains("could not be loaded", html);
    }

    private static WebApplicationFactory<Program> WithLogs(HubWebFactory factory, LogCollector logs)
    {
        return factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services => services.AddSingleton<ILoggerProvider>(logs)));
    }

    private static async Task<string> RawSteamApiKeyAsync(HubWebFactory factory)
    {
        var raw = await factory.RunAsync(async services => (await services.GetRequiredService<HubDbContext>().Settings.AsNoTracking().SingleAsync(setting => setting.Key == nameof(SiteSettings))).Value);
        return JsonNode.Parse(raw)![nameof(SiteSettings.SteamApiKey)]!.GetValue<string>();
    }

    private static async Task<T> Run<T>(WebApplicationFactory<Program> application, Func<IServiceProvider, Task<T>> action)
    {
        using var scope = application.Services.CreateScope();

        return await action(scope.ServiceProvider);
    }

    private static async Task Run(WebApplicationFactory<Program> application, Func<IServiceProvider, Task> action)
    {
        using var scope = application.Services.CreateScope();

        await action(scope.ServiceProvider);
    }
}
