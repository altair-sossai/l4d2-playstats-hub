using System.Net;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;
using L4D2PlayStats.Hub.Core.Modules.Backups.Models;
using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using L4D2PlayStats.Hub.Core.Modules.Backups.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;

namespace L4D2PlayStats.Hub.Tests.Web.Backups;

[TestClass]
public class BackupAdminPageTests
{
    private const string Page = "/admin/backup";
    private const string Signature = "sv=2024-11-04&sp=racwdl&sig=c2VjcmV0LXNpZw%3D%3D";
    private const string ContainerUrl = "https://account.blob.core.windows.net/backups?" + Signature;
    private const string AccountKey = "c2VjcmV0LWFjY291bnQta2V5LXZhbHVlLWZvci10ZXN0cw==";
    private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=account;AccountKey=" + AccountKey + ";EndpointSuffix=core.windows.net";
    private const string NoDatabaseWarning = "This site does not use SQLite";

    [TestMethod]
    public async Task ThePageShowsTheDefaultsAndExplainsWhatTheBackupContains()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();

        var html = await PageAsync(client);

        Assert.Contains("<h3>Backup</h3>", html);
        Assert.Contains("id=\"IntervalInHours\" name=\"IntervalInHours\" type=\"number\" min=\"1\" max=\"168\" value=\"8\"", html);
        Assert.Contains("id=\"Keep\" name=\"Keep\" type=\"number\" min=\"1\" max=\"1000\" value=\"30\"", html);
        Assert.Contains("id=\"Prefix\" name=\"Prefix\" value=\"hub-backup\"", html);
        Assert.Contains("No backup was made yet.", html);
        Assert.Contains("<code>hub.db</code>", html);
        Assert.Contains("<code>keys/</code>", html);
        Assert.Contains("<code>branding/</code>", html);
        Assert.Contains("hub.env", html);
        Assert.Contains("<a href=\"/admin/backup\"", html);
    }

    [TestMethod]
    public async Task OnlyAdministratorsOpenThePage()
    {
        using var factory = new HubWebFactory();
        using var player = factory.Client(HubWebFactory.Player);

        await factory.CompleteSetupAsync();

        var page = await player.GetAsync(Page);
        var run = await player.PostAsync("/admin/backup/run", new FormUrlEncodedContent([]));

        Assert.AreEqual(HttpStatusCode.Forbidden, page.StatusCode);
        Assert.AreNotEqual(HttpStatusCode.Redirect, run.StatusCode);
        Assert.IsEmpty(factory.Backups.Created);
    }

    [TestMethod]
    public async Task TheWarningAboutTheDatabaseOnlyShowsWithoutSqlite()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();

        var html = await PageAsync(client);
        var isSqlite = await factory.RunAsync(services => Task.FromResult(services.GetRequiredService<HubDbContext>().Database.IsSqlite()));

        Assert.AreEqual(!isSqlite, html.Contains(NoDatabaseWarning));
    }

    [TestMethod]
    public async Task SavingAContainerAddressKeepsItAndThePageNeverShowsItsToken()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();

        var response = await SaveAsync(client, new Dictionary<string, string>
        {
            ["Enabled"] = "true",
            ["Destination"] = "ContainerUrl",
            ["ContainerUrl"] = ContainerUrl,
            ["IntervalInHours"] = "6",
            ["Keep"] = "10",
            ["Prefix"] = "l4d2.com.br"
        });

        var settings = await SettingsAsync(factory);
        var html = await PageAsync(client);

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.IsTrue(settings.Enabled);
        Assert.AreEqual(ContainerUrl, settings.ContainerUrl);
        Assert.AreEqual(6, settings.IntervalInHours);
        Assert.AreEqual(10, settings.Keep);
        Assert.AreEqual("l4d2.com.br", settings.Prefix);
        Assert.DoesNotContain("sig=", html);
        Assert.DoesNotContain("c2VjcmV0LXNpZw", html);
        Assert.Contains("placeholder=\"https://account.blob.core.windows.net/backups?••••••••\"", html);
        Assert.Contains("Saved.", html);
    }

    [TestMethod]
    public async Task AnEmptySecretFieldKeepsTheSavedSecret()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new BackupSettings { Enabled = true, ContainerUrl = ContainerUrl, ConnectionString = ConnectionString, ContainerName = "backups" });

        await SaveAsync(client, new Dictionary<string, string>
        {
            ["Enabled"] = "false",
            ["Destination"] = "ContainerUrl",
            ["ContainerUrl"] = "",
            ["ConnectionString"] = " ",
            ["ContainerName"] = "backups",
            ["IntervalInHours"] = "8",
            ["Keep"] = "30",
            ["Prefix"] = "hub-backup"
        });

        var settings = await SettingsAsync(factory);

        Assert.IsFalse(settings.Enabled);
        Assert.AreEqual(ContainerUrl, settings.ContainerUrl);
        Assert.AreEqual(ConnectionString, settings.ConnectionString);
    }

    [TestMethod]
    public async Task AnInvalidContainerAddressIsNotSavedAndThePageSaysWhy()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new BackupSettings { ContainerUrl = ContainerUrl });

        await SaveAsync(client, Form(new Dictionary<string, string> { ["ContainerUrl"] = "http://account.blob.core.windows.net/backups" }));

        var settings = await SettingsAsync(factory);
        var html = await PageAsync(client);

        Assert.AreEqual(ContainerUrl, settings.ContainerUrl);
        Assert.Contains("The container address was not changed", html);
        Assert.DoesNotContain("Saved.", html);
    }

    [TestMethod]
    public async Task AConnectionStringIsSavedAndShownWithoutItsKey()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();

        await SaveAsync(client, Form(new Dictionary<string, string>
        {
            ["Destination"] = "ConnectionString",
            ["ConnectionString"] = ConnectionString,
            ["ContainerName"] = "hub-backups"
        }));

        var settings = await SettingsAsync(factory);
        var html = await PageAsync(client);

        Assert.AreEqual(BackupDestination.ConnectionString, settings.Destination);
        Assert.AreEqual(ConnectionString, settings.ConnectionString);
        Assert.AreEqual("hub-backups", settings.ContainerName);
        Assert.DoesNotContain(AccountKey, html);
        Assert.Contains("AccountKey=••••••••", html);
        Assert.Contains("id=\"Destination_ConnectionString\" name=\"Destination\" value=\"ConnectionString\" checked=\"checked\"", html);
    }

    [TestMethod]
    public async Task AnInvalidConnectionStringOrContainerNameIsNotSaved()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();

        await SaveAsync(client, Form(new Dictionary<string, string>
        {
            ["Destination"] = "ConnectionString",
            ["ConnectionString"] = "not a connection string",
            ["ContainerName"] = "Hub_Backups"
        }));

        var settings = await SettingsAsync(factory);
        var html = await PageAsync(client);

        Assert.IsNull(settings.ConnectionString);
        Assert.IsNull(settings.ContainerName);
        Assert.Contains("The connection string was not changed", html);
        Assert.Contains("The container name was not changed", html);
    }

    [TestMethod]
    public async Task TheIntervalAndTheNumberToKeepStayWithinTheirLimits()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();

        await SaveAsync(client, Form(new Dictionary<string, string> { ["IntervalInHours"] = "0", ["Keep"] = "99999" }));

        var settings = await SettingsAsync(factory);

        Assert.AreEqual(1, settings.IntervalInHours);
        Assert.AreEqual(1000, settings.Keep);
    }

    [TestMethod]
    public async Task AnInvalidPrefixIsNotSavedAndAnEmptyOneGoesBackToTheDefault()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new BackupSettings { Prefix = "site" });

        await SaveAsync(client, Form(new Dictionary<string, string> { ["Prefix"] = "../other" }));

        var invalid = await SettingsAsync(factory);
        var html = await PageAsync(client);

        await SaveAsync(client, Form(new Dictionary<string, string> { ["Prefix"] = "" }));

        var empty = await SettingsAsync(factory);

        Assert.AreEqual("site", invalid.Prefix);
        Assert.Contains("The file name prefix was not changed", html);
        Assert.AreEqual("hub-backup", empty.Prefix);
    }

    [TestMethod]
    public async Task BackUpNowUploadsAZipAndThePageListsIt()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new BackupSettings { Enabled = true, ContainerUrl = ContainerUrl });

        var response = await HubWebFactory.PostFormAsync(client, Page, "/admin/backup/run", []);
        var html = await PageAsync(client);
        var blob = factory.Backups.Storage.Blobs.Keys.Single();

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual(Page, response.Headers.Location?.OriginalString);
        Assert.Contains($"Backup done: {blob}.", html);
        Assert.Contains("class=\"backup-ok\"", html);
        Assert.Contains(blob, html);
        Assert.Contains("manual", html);
        Assert.DoesNotContain("No backup was made yet.", html);
    }

    [TestMethod]
    public async Task AFailedBackupShowsTheErrorOnThePage()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new BackupSettings { Enabled = true, ContainerUrl = ContainerUrl });

        factory.Backups.Storage.FailUploadWith = $"The specified container does not exist: {ContainerUrl}";

        await HubWebFactory.PostFormAsync(client, Page, "/admin/backup/run", []);

        var html = await PageAsync(client);

        Assert.Contains("The backup failed.", html);
        Assert.Contains("The specified container does not exist: ••••••••", html);
        Assert.Contains("class=\"backup-failed\"", html);
        Assert.DoesNotContain("sig=", html);
    }

    [TestMethod]
    public async Task TheButtonsAreOffUntilADestinationIsSaved()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();

        var before = await PageAsync(client);

        await factory.SaveAsync(new BackupSettings { ContainerUrl = ContainerUrl });

        var after = await PageAsync(client);
        var isSqlite = await factory.RunAsync(services => Task.FromResult(services.GetRequiredService<HubDbContext>().Database.IsSqlite()));

        Assert.AreEqual(3, CountDisabledButtons(before));
        Assert.AreEqual(isSqlite ? 0 : 1, CountDisabledButtons(after));
    }

    [TestMethod]
    public async Task TestConnectionReportsTheResult()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new BackupSettings { ContainerUrl = ContainerUrl });

        await HubWebFactory.PostFormAsync(client, Page, "/admin/backup/test", []);

        var passed = await PageAsync(client);

        factory.Backups.Storage.HideFromList = true;

        await HubWebFactory.PostFormAsync(client, Page, "/admin/backup/test", []);

        var failed = await PageAsync(client);

        Assert.Contains("The destination accepted a test file, listed it and deleted it.", passed);
        Assert.Contains("The test failed.", failed);
        Assert.IsEmpty(factory.Backups.Storage.Blobs);
    }

    [TestMethod]
    public async Task ThePageShowsTheLastAndTheNextBackup()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new BackupSettings { Enabled = true, ContainerUrl = ContainerUrl });

        var waiting = await PageAsync(client);

        await factory.RunAsync(services => services.GetRequiredService<IBackupHistoryService>().AddAsync(new BackupRun
        {
            StartedAtUtc = DateTime.UtcNow.AddHours(-1),
            Succeeded = true,
            BlobName = "hub-backup-20261119-000000.zip",
            SizeInBytes = 3 * 1024 * 1024,
            Trigger = BackupTrigger.Schedule
        }));

        var done = await PageAsync(client);

        Assert.Contains("None yet", waiting);
        Assert.Contains("Within a minute", waiting);
        Assert.DoesNotContain("None yet", done);
        Assert.Contains("hub-backup-20261119-000000.zip", done);
        Assert.Contains("3.0 MB", done);
        Assert.Contains("scheduled", done);
    }

    private static async Task<string> PageAsync(HttpClient client)
    {
        return WebUtility.HtmlDecode(await client.GetStringAsync(Page));
    }

    private static int CountDisabledButtons(string html)
    {
        return html.Split("disabled=\"disabled\"").Length - 1;
    }

    private static Dictionary<string, string> Form(Dictionary<string, string> changes)
    {
        var form = new Dictionary<string, string>
        {
            ["Enabled"] = "false",
            ["Destination"] = "ContainerUrl",
            ["IntervalInHours"] = "8",
            ["Keep"] = "30",
            ["Prefix"] = "hub-backup"
        };

        foreach (var (key, value) in changes)
            form[key] = value;

        return form;
    }

    private static Task<HttpResponseMessage> SaveAsync(HttpClient client, Dictionary<string, string> form)
    {
        return HubWebFactory.PostFormAsync(client, Page, Page, form);
    }

    private static Task<BackupSettings> SettingsAsync(HubWebFactory factory)
    {
        return factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<BackupSettings>());
    }
}
