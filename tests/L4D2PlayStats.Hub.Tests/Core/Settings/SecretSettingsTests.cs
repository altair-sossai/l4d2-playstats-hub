using System.Security.Cryptography;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.DependencyInjection;
using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;
using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Core.Settings;

[TestClass]
public sealed class SecretSettingsTests : IDisposable
{
    private const string SteamApiKey = "STEAMKEY0123456789ABCDEF";
    private const string ContainerUrl = "https://account.blob.core.windows.net/backups?sv=2024&sig=SASSIGNATURE123";
    private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=account;AccountKey=ACCOUNTKEYSECRET==;EndpointSuffix=core.windows.net";
    private const string SetupToken = "0123456789abcdef0123456789abcdef";

    private readonly TestDatabase _database = new();
    private readonly SecretsKey _key = Key();

    public SecretSettingsTests()
    {
        using var dbContext = _database.CreateContext();

        dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _database.Dispose();
    }

    [TestMethod]
    public async Task TheSecretsAreEncryptedInTheDatabase()
    {
        await SaveAllAsync(_key);

        var site = await RawAsync(nameof(SiteSettings));
        var backup = await RawAsync(nameof(BackupSettings));
        var setup = await RawAsync(nameof(SetupSettings));

        Assert.DoesNotContain(SteamApiKey, site);
        Assert.DoesNotContain(ContainerUrl, backup);
        Assert.DoesNotContain("SASSIGNATURE123", backup);
        Assert.DoesNotContain("ACCOUNTKEYSECRET", backup);
        Assert.DoesNotContain(SetupToken, setup);
        Assert.Contains(CoreConsts.SecretPrefix, site);
        Assert.Contains(CoreConsts.SecretPrefix, backup);
        Assert.Contains(CoreConsts.SecretPrefix, setup);
        Assert.Contains("\"Name\":\"My site\"", site);
        Assert.Contains("\"ContainerName\":\"hub-backups\"", backup);
    }

    [TestMethod]
    public async Task TheSecretsComeBackAsTheyWereSaved()
    {
        await SaveAllAsync(_key);

        using var services = Services(_key);

        var site = await GetAsync<SiteSettings>(services);
        var backup = await GetAsync<BackupSettings>(services);
        var setup = await GetAsync<SetupSettings>(services);

        Assert.AreEqual(SteamApiKey, site.SteamApiKey);
        Assert.AreEqual("My site", site.Name);
        Assert.AreEqual(ContainerUrl, backup.ContainerUrl);
        Assert.AreEqual(ConnectionString, backup.ConnectionString);
        Assert.AreEqual(SetupToken, setup.Token);
    }

    [TestMethod]
    public async Task TheObjectGivenToSaveKeepsItsPlainValues()
    {
        using var services = Services(_key);

        var site = new SiteSettings { SteamApiKey = SteamApiKey };

        await SaveAsync(services, site);

        Assert.AreEqual(SteamApiKey, site.SteamApiKey);
        Assert.AreEqual(SteamApiKey, (await GetAsync<SiteSettings>(services)).SteamApiKey);
    }

    [TestMethod]
    public async Task EmptySecretsStayEmpty()
    {
        using var services = Services(_key);

        await SaveAsync(services, new SiteSettings { SteamApiKey = null });

        Assert.DoesNotContain(CoreConsts.SecretPrefix, await RawAsync(nameof(SiteSettings)));
        Assert.IsNull((await GetAsync<SiteSettings>(services)).SteamApiKey);
    }

    [TestMethod]
    public async Task ASecretCanBeRemoved()
    {
        await SaveAllAsync(_key);

        using var services = Services(_key);

        var site = await GetAsync<SiteSettings>(services);

        site.SteamApiKey = null;

        await SaveAsync(services, site);

        Assert.DoesNotContain(CoreConsts.SecretPrefix, await RawAsync(nameof(SiteSettings)));
        Assert.IsNull((await ReadAsync<SiteSettings>(_key)).SteamApiKey);
    }

    [TestMethod]
    public async Task PlainSecretsWrittenByOlderVersionsAreStillRead()
    {
        await WriteRawAsync(nameof(SiteSettings), $"{{\"Name\":\"Old site\",\"SteamApiKey\":\"{SteamApiKey}\"}}");

        using var services = Services(_key);

        var site = await GetAsync<SiteSettings>(services);

        Assert.AreEqual(SteamApiKey, site.SteamApiKey);
        Assert.AreEqual("Old site", site.Name);
    }

    [TestMethod]
    public async Task PlainSecretsAreEncryptedOnTheNextSave()
    {
        await WriteRawAsync(nameof(SiteSettings), $"{{\"Name\":\"Old site\",\"SteamApiKey\":\"{SteamApiKey}\"}}");

        using var services = Services(_key);

        var site = await GetAsync<SiteSettings>(services);

        site.Name = "Renamed";

        await SaveAsync(services, site);

        var raw = await RawAsync(nameof(SiteSettings));

        Assert.DoesNotContain(SteamApiKey, raw);
        Assert.Contains(CoreConsts.SecretPrefix, raw);
        Assert.AreEqual(SteamApiKey, (await ReadAsync<SiteSettings>(_key)).SteamApiKey);
    }

    [TestMethod]
    public async Task TheStartupMigrationEncryptsThePlainSecretsOnce()
    {
        await WriteRawAsync(nameof(SiteSettings), $"{{\"Name\":\"Old site\",\"SteamApiKey\":\"{SteamApiKey}\"}}");
        await WriteRawAsync(nameof(BackupSettings), $"{{\"Enabled\":true,\"Destination\":0,\"ContainerUrl\":\"{ContainerUrl}\",\"ConnectionString\":null,\"Keep\":30}}");
        await WriteRawAsync(nameof(SetupSettings), "{\"Token\":null,\"Completed\":true}");

        using var services = Services(_key);

        Assert.AreEqual(SteamApiKey, (await GetAsync<SiteSettings>(services)).SteamApiKey);

        var first = await MigrateAsync(services);
        var second = await MigrateAsync(services);

        var site = await RawAsync(nameof(SiteSettings));
        var backup = await RawAsync(nameof(BackupSettings));

        Assert.AreEqual(2, first);
        Assert.AreEqual(0, second);
        Assert.DoesNotContain(SteamApiKey, site);
        Assert.DoesNotContain(ContainerUrl, backup);
        Assert.Contains("\"Name\":\"Old site\"", site);
        Assert.Contains("\"Keep\":30", backup);
        Assert.AreEqual("{\"Token\":null,\"Completed\":true}", await RawAsync(nameof(SetupSettings)));
        Assert.AreEqual(SteamApiKey, (await GetAsync<SiteSettings>(services)).SteamApiKey);
        Assert.AreEqual(ContainerUrl, (await ReadAsync<BackupSettings>(_key)).ContainerUrl);
    }

    [TestMethod]
    public async Task WithAnotherKeyTheSecretsAreEmptyAndAWarningIsLogged()
    {
        await SaveAllAsync(_key);

        var logs = new LogCollector();

        using var services = Services(Key(), logs);

        var site = await GetAsync<SiteSettings>(services);
        var backup = await GetAsync<BackupSettings>(services);

        Assert.IsNull(site.SteamApiKey);
        Assert.AreEqual("My site", site.Name);
        Assert.IsNull(backup.ContainerUrl);
        Assert.IsNull(backup.ConnectionString);
        Assert.AreEqual("hub-backups", backup.ContainerName);

        var warnings = logs.Entries.Where(entry => entry.Level == LogLevel.Warning).Select(entry => entry.Message).ToList();

        Assert.IsTrue(warnings.Any(message => message.Contains("SteamApiKey") && message.Contains("SiteSettings")));
        Assert.IsTrue(warnings.Any(message => message.Contains("ContainerUrl")));
        AssertNoSecretIn(logs);
    }

    [TestMethod]
    public async Task SavingWithAnotherKeyKeepsTheSecretsItCannotRead()
    {
        await SaveAllAsync(_key);

        using (var other = Services(Key()))
        {
            var site = await GetAsync<SiteSettings>(other);

            site.Name = "Renamed";

            await SaveAsync(other, site);
        }

        var restored = await ReadAsync<SiteSettings>(_key);

        Assert.AreEqual("Renamed", restored.Name);
        Assert.AreEqual(SteamApiKey, restored.SteamApiKey);
    }

    [TestMethod]
    public async Task ANewSecretSavedWithAnotherKeyReplacesTheOldOne()
    {
        await SaveAllAsync(_key);

        var other = Key();

        using (var services = Services(other))
            await SaveAsync(services, new SiteSettings { SteamApiKey = "NEWSTEAMKEY" });

        Assert.AreEqual("NEWSTEAMKEY", (await ReadAsync<SiteSettings>(other)).SteamApiKey);
        Assert.IsNull((await ReadAsync<SiteSettings>(_key)).SteamApiKey);
    }

    [TestMethod]
    public async Task WithoutAKeyEncryptedSecretsAreEmptyAndNewOnesAreNotStoredInPlainText()
    {
        await SaveAllAsync(_key);

        var logs = new LogCollector();

        using var services = Services(Missing(), logs);

        var site = await GetAsync<SiteSettings>(services);

        Assert.IsNull(site.SteamApiKey);

        site.SteamApiKey = "TYPEDWITHOUTAKEY";

        await SaveAsync(services, site);

        Assert.DoesNotContain("TYPEDWITHOUTAKEY", await RawAsync(nameof(SiteSettings)));
        Assert.AreEqual(SteamApiKey, (await ReadAsync<SiteSettings>(_key)).SteamApiKey);
        Assert.IsTrue(logs.Entries.Any(entry => entry.Level == LogLevel.Error && entry.Message.Contains("SteamApiKey")));
        Assert.IsFalse(logs.Entries.Any(entry => entry.Message.Contains("TYPEDWITHOUTAKEY")));
        AssertNoSecretIn(logs);
    }

    [TestMethod]
    public async Task WithoutAKeyThePlainSecretsAreLeftAsTheyWere()
    {
        const string plain = "{\"Name\":\"Old site\",\"SteamApiKey\":\"" + SteamApiKey + "\"}";

        await WriteRawAsync(nameof(SiteSettings), plain);

        using var services = Services(Missing());

        Assert.AreEqual(0, await MigrateAsync(services));
        Assert.AreEqual(plain, await RawAsync(nameof(SiteSettings)));
        Assert.AreEqual(SteamApiKey, (await GetAsync<SiteSettings>(services)).SteamApiKey);
    }

    [TestMethod]
    public void EachEncryptionUsesANewNonceAndATamperedValueIsRejected()
    {
        var key = _key.Value!;
        var first = SecretCipher.Encrypt(key, SteamApiKey);
        var second = SecretCipher.Encrypt(key, SteamApiKey);
        var payload = Convert.FromBase64String(first[CoreConsts.SecretPrefix.Length..]);

        payload[^1] ^= 1;

        var tampered = CoreConsts.SecretPrefix + Convert.ToBase64String(payload);

        Assert.AreNotEqual(first, second);
        Assert.IsTrue(SecretCipher.TryDecrypt(key, first, out var plain));
        Assert.AreEqual(SteamApiKey, plain);
        Assert.IsFalse(SecretCipher.TryDecrypt(key, tampered, out _));
        Assert.IsFalse(SecretCipher.TryDecrypt(key, CoreConsts.SecretPrefix + "not base64!", out _));
        Assert.IsFalse(SecretCipher.TryDecrypt(key, CoreConsts.SecretPrefix + "AAAA", out _));
        Assert.IsFalse(SecretCipher.TryDecrypt(Key().Value!, first, out _));
    }

    [TestMethod]
    public void OnlyTheMarkedPropertiesAreSecrets()
    {
        CollectionAssert.AreEquivalent(new[] { "SteamApiKey" }, SecretProperties.Of(typeof(SiteSettings)).ToList());
        CollectionAssert.AreEquivalent(new[] { "ContainerUrl", "ConnectionString" }, SecretProperties.Of(typeof(BackupSettings)).ToList());
        CollectionAssert.AreEquivalent(new[] { "Token" }, SecretProperties.Of(typeof(SetupSettings)).ToList());
        CollectionAssert.AreEquivalent(new[] { typeof(SiteSettings), typeof(BackupSettings), typeof(SetupSettings) }, SecretProperties.SettingTypes().ToList());
    }

    private static SecretsKey Key()
    {
        return new SecretsKey(RandomNumberGenerator.GetBytes(CoreConsts.SecretKeyBytes), "test key", null, false, null);
    }

    private static SecretsKey Missing()
    {
        return new SecretsKey(null, "missing key", null, false, "missing");
    }

    private static void AssertNoSecretIn(LogCollector logs)
    {
        foreach (var entry in logs.Entries)
            foreach (var secret in new[] { SteamApiKey, ContainerUrl, "SASSIGNATURE123", "ACCOUNTKEYSECRET", SetupToken })
                Assert.DoesNotContain(secret, entry.Message);
    }

    private ServiceProvider Services(SecretsKey key, LogCollector? logs = null)
    {
        var services = new ServiceCollection();

        services.AddLogging(logging =>
        {
            if (logs != null)
                logging.AddProvider(logs);
        });
        services.AddHubCore();
        services.AddSingleton(key);
        services.AddDbContext<HubDbContext>(_database.Configure);

        return services.BuildServiceProvider();
    }

    private async Task SaveAllAsync(SecretsKey key)
    {
        using var services = Services(key);

        await SaveAsync(services, new SiteSettings { Name = "My site", SteamApiKey = SteamApiKey });
        await SaveAsync(services, new BackupSettings { Destination = BackupDestination.ConnectionString, ContainerUrl = ContainerUrl, ConnectionString = ConnectionString, ContainerName = "hub-backups" });
        await SaveAsync(services, new SetupSettings { Token = SetupToken });
    }

    private static async Task SaveAsync<T>(ServiceProvider services, T settings) where T : class, new()
    {
        using var scope = services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<ISettingsService>().SaveAsync(settings);
    }

    private static async Task<T> GetAsync<T>(ServiceProvider services) where T : class, new()
    {
        using var scope = services.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<ISettingsService>().GetAsync<T>();
    }

    private async Task<T> ReadAsync<T>(SecretsKey key) where T : class, new()
    {
        await using var services = Services(key);

        return await GetAsync<T>(services);
    }

    private static async Task<int> MigrateAsync(ServiceProvider services)
    {
        using var scope = services.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<ISettingsEncryptionService>().EncryptPlainSecretsAsync();
    }

    private async Task<string> RawAsync(string key)
    {
        await using var dbContext = _database.CreateContext();

        return (await dbContext.Settings.AsNoTracking().SingleAsync(setting => setting.Key == key)).Value;
    }

    private async Task WriteRawAsync(string key, string value)
    {
        await using var dbContext = _database.CreateContext();

        dbContext.Settings.Add(new Setting { Key = key, Value = value });

        await dbContext.SaveChangesAsync();
    }
}
