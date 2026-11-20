using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;
using L4D2PlayStats.Hub.Core.Modules.Backups.Models;
using L4D2PlayStats.Hub.Core.Modules.Backups.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Web.Infrastructure.Backups;
using L4D2PlayStats.Hub.Web.Models.Backups;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace L4D2PlayStats.Hub.Tests.Web.Backups;

[TestClass]
public sealed class BackupVerificationTests : IDisposable
{
    private const string Page = "/admin/backup";
    private const string Signature = "sv=2024-11-04&sp=racwdl&sig=c2VjcmV0LXNpZw%3D%3D";
    private const string ContainerUrl = "https://account.blob.core.windows.net/backups?" + Signature;
    private const string OlderBlob = "hub-backup-20261101-000000.zip";
    private const string NewerBlob = "hub-backup-29991231-235959.zip";
    private const string PassedMessage = "The zip opened, the database passed the integrity check and has {0} matches, {1} statistics files, {2} players and {3} settings, the same as the site had when the backup was made.";
    private const string UnreadableMessage = "The backup could not be read: {0}";

    private readonly string _work = Path.Combine(Path.GetTempPath(), "l4d2-playstats-hub-verify-tests", Guid.NewGuid().ToString("N"));

    public BackupVerificationTests()
    {
        Directory.CreateDirectory(_work);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();

        if (Directory.Exists(_work))
            Directory.Delete(_work, true);
    }

    [TestMethod]
    public async Task EachBackupIsDownloadedAgainAndVerified()
    {
        using var factory = await StartAsync();

        if (!await IsSqliteAsync(factory))
            return;

        await factory.CreateServerAsync("Verified Server");

        var run = await RunAsync(factory);
        var history = await HistoryAsync(factory);
        var counts = Manifest(factory.Backups.Storage.Blobs[run.BlobName!]).Counts!;

        Assert.IsTrue(run.Succeeded, run.Error);
        Assert.IsNotNull(run.Verification);
        Assert.AreEqual(BackupVerificationStatus.Passed, run.Verification.Status, Describe(run.Verification));
        Assert.AreEqual(PassedMessage, run.Verification.Message);
        CollectionAssert.AreEqual(new[] { counts.Matches, counts.StatisticsFiles, counts.Players, counts.Settings }.Select(count => count.ToString(CultureInfo.InvariantCulture)).ToList(), run.Verification.Arguments);
        Assert.IsGreaterThan(0L, counts.Settings);
        Assert.AreEqual(BackupVerificationStatus.Passed, history.Runs[0].Verification!.Status);
        Assert.HasCount(1, factory.Backups.Storage.DownloadedTo);
        Assert.AreEqual(run.BlobName, factory.Backups.Storage.Blobs.Keys.Single());
        AssertRemoved(factory.Backups.Storage.DownloadedTo[0]);
    }

    [TestMethod]
    public async Task TheManifestKeepsTheRowCountsOfTheSiteAtBackupTime()
    {
        using var factory = await StartAsync();

        if (!await IsSqliteAsync(factory))
            return;

        var run = await RunAsync(factory);
        var manifest = Manifest(factory.Backups.Storage.Blobs[run.BlobName!]);
        var expected = await factory.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            return (await dbContext.Matches.LongCountAsync(), await dbContext.StatisticsFiles.LongCountAsync(), await dbContext.PlayerProfiles.LongCountAsync());
        });

        Assert.IsNotNull(manifest.Counts);
        Assert.AreEqual(expected, (manifest.Counts.Matches, manifest.Counts.StatisticsFiles, manifest.Counts.Players));
        Assert.IsGreaterThan(0L, manifest.Counts.Settings);
    }

    [TestMethod]
    public async Task ATruncatedDownloadFailsTheVerificationAndKeepsTheOlderBackups()
    {
        using var factory = await StartAsync(settings => settings.Keep = 1);

        if (!await IsSqliteAsync(factory))
            return;

        var storage = factory.Backups.Storage;

        storage.Blobs[OlderBlob] = [1];
        storage.ChangeDownload = content => content[..(content.Length / 2)];

        var run = await RunAsync(factory);

        Assert.IsTrue(run.Succeeded);
        Assert.AreEqual(BackupVerificationStatus.Failed, run.Verification!.Status);
        Assert.AreEqual("The downloaded file has {0} bytes, but the backup uploaded {1}.", run.Verification.Message);
        Assert.Contains(OlderBlob, storage.Blobs.Keys.ToList());
        Assert.IsEmpty(storage.Deleted);
        AssertRemoved(storage.DownloadedTo.Single());
    }

    [TestMethod]
    public async Task ACorruptedZipIsReportedAsFailed()
    {
        using var factory = await StartAsync();

        if (!await IsSqliteAsync(factory))
            return;

        factory.Backups.Storage.Blobs[NewerBlob] = "PK\u0003\u0004 this is not a zip"u8.ToArray();

        var result = await VerifyAsync(factory);

        Assert.AreEqual(NewerBlob, result.BlobName);
        Assert.AreEqual(BackupVerificationStatus.Failed, result.Verification.Status);
        Assert.AreEqual(UnreadableMessage, result.Verification.Message);
        AssertRemoved(factory.Backups.Storage.DownloadedTo.Single());
    }

    [TestMethod]
    public async Task ATruncatedZipIsReportedAsFailed()
    {
        using var factory = await StartAsync();

        if (!await IsSqliteAsync(factory))
            return;

        var run = await RunAsync(factory);
        var content = factory.Backups.Storage.Blobs[run.BlobName!];

        factory.Backups.Storage.Blobs[NewerBlob] = content[..(content.Length - 200)];

        var result = await VerifyAsync(factory);

        Assert.AreEqual(BackupVerificationStatus.Failed, result.Verification.Status);
        Assert.AreEqual(UnreadableMessage, result.Verification.Message);
        AssertRemoved(factory.Backups.Storage.DownloadedTo.Last());
    }

    [TestMethod]
    public async Task ADatabaseThatFailsTheIntegrityCheckIsReportedAsFailed()
    {
        using var factory = await StartAsync();

        if (!await IsSqliteAsync(factory))
            return;

        factory.Backups.Storage.Blobs[NewerBlob] = Zip(CorruptedDatabase(), new BackupManifest { IncludesDatabase = true, Files = ["hub.db"] });

        var result = await VerifyAsync(factory);

        Assert.AreEqual(BackupVerificationStatus.Failed, result.Verification.Status);
        Assert.AreEqual("The database failed the integrity check: {0}", result.Verification.Message, Describe(result.Verification));
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Verification.Arguments.Single()));
        AssertRemoved(factory.Backups.Storage.DownloadedTo.Single());
    }

    [TestMethod]
    public async Task RowCountsThatDoNotMatchTheManifestFailTheVerification()
    {
        using var factory = await StartAsync();

        if (!await IsSqliteAsync(factory))
            return;

        var run = await RunAsync(factory);
        var manifest = Manifest(factory.Backups.Storage.Blobs[run.BlobName!]);

        var counts = manifest.Counts!;

        counts.Matches = 7;
        factory.Backups.Storage.Blobs[NewerBlob] = ReplaceManifest(factory.Backups.Storage.Blobs[run.BlobName!], manifest);

        var result = await VerifyAsync(factory);

        Assert.AreEqual(BackupVerificationStatus.Failed, result.Verification.Status);
        Assert.AreEqual("The table {0} has {1} rows in the backup, but the site had {2} when the backup was made.", result.Verification.Message);
        CollectionAssert.AreEqual(new[] { "Matches", "0", "7" }, result.Verification.Arguments);
    }

    [TestMethod]
    public async Task AZipWithoutTheManifestOrTheDatabaseFails()
    {
        using var factory = await StartAsync();

        if (!await IsSqliteAsync(factory))
            return;

        var storage = factory.Backups.Storage;

        storage.Blobs[OlderBlob] = Zip(null, new BackupManifest { IncludesDatabase = false });
        storage.Blobs[NewerBlob] = Zip(CorruptedDatabase(), null);

        var noManifest = await VerifyAsync(factory);

        storage.Blobs.TryRemove(NewerBlob, out _);

        var noDatabase = await VerifyAsync(factory);

        Assert.AreEqual("The zip has no readable manifest.json.", noManifest.Verification.Message);
        Assert.AreEqual("The zip has no hub.db.", noDatabase.Verification.Message);
        Assert.AreEqual(BackupVerificationStatus.Failed, noDatabase.Verification.Status);
    }

    [TestMethod]
    public async Task VerifyNowChecksTheNewestBackupAndRecordsItInTheHistory()
    {
        using var factory = await StartAsync();
        using var client = factory.Client(HubWebFactory.Administrator);

        if (!await IsSqliteAsync(factory))
            return;

        var run = await RunAsync(factory);

        await factory.RunAsync(services => services.GetRequiredService<IBackupHistoryService>().RecordVerificationAsync(run.BlobName!, BackupVerification.Of(BackupVerificationStatus.Failed, "old")));

        var response = await HubWebFactory.PostFormAsync(client, Page, "/admin/backup/verify", []);
        var html = WebUtility.HtmlDecode(await client.GetStringAsync(Page));
        var history = await HistoryAsync(factory);

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual(Page, response.Headers.Location?.OriginalString);
        Assert.Contains($"The backup was verified: {run.BlobName}.", html);
        Assert.Contains("backup-verify-result", html);
        Assert.Contains("class=\"label label-success backup-verified\"", html);
        Assert.Contains("<th>Verified</th>", html);
        Assert.AreEqual(BackupVerificationStatus.Passed, history.Runs.Single().Verification!.Status);
        Assert.HasCount(2, factory.Backups.Storage.DownloadedTo);
    }

    [TestMethod]
    public async Task AFailedVerificationShowsOnThePageWithoutTheSecret()
    {
        using var factory = await StartAsync();
        using var client = factory.Client(HubWebFactory.Administrator);

        if (!await IsSqliteAsync(factory))
            return;

        await RunAsync(factory);

        factory.Backups.Storage.FailDownloadWith = $"Server failed to authenticate the request for {ContainerUrl}.\nStatus: 403";

        await HubWebFactory.PostFormAsync(client, Page, "/admin/backup/verify", []);

        var html = WebUtility.HtmlDecode(await client.GetStringAsync(Page));

        Assert.Contains("The verification failed.", html);
        Assert.Contains("The backup could not be read: Server failed to authenticate the request for ••••••••.", html);
        Assert.Contains("class=\"label label-danger backup-verify-failed\"", html);
        Assert.DoesNotContain("sig=", html);
        Assert.DoesNotContain("c2VjcmV0LXNpZw", html);
    }

    [TestMethod]
    public async Task TheLogNeverHasTheSecret()
    {
        using var factory = await StartAsync();
        var logs = new LogCollector();

        using var logged = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services => services.AddSingleton<ILoggerProvider>(logs)));

        if (!await IsSqliteAsync(factory))
            return;

        factory.Backups.Storage.Blobs[NewerBlob] = [1, 2, 3];
        factory.Backups.Storage.FailDownloadWith = $"The request for {ContainerUrl} failed";

        using (var scope = logged.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<IBackupService>().VerifyLatestAsync();
        }

        var errors = logs.Entries.Where(entry => entry.Category.Contains("Backup", StringComparison.Ordinal) && entry.Level == LogLevel.Error).ToList();

        Assert.IsNotEmpty(errors);
        Assert.IsFalse(logs.Entries.Any(entry => entry.Message.Contains("sig=", StringComparison.Ordinal) || entry.Message.Contains("c2VjcmV0LXNpZw", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task VerifyNowWithoutBackupsSaysSo()
    {
        using var factory = await StartAsync();

        if (!await IsSqliteAsync(factory))
            return;

        var result = await VerifyAsync(factory);

        Assert.IsNull(result.BlobName);
        Assert.AreEqual(BackupVerificationStatus.Failed, result.Verification.Status);
        Assert.AreEqual("No backup file was found in the destination.", result.Verification.Message);
        Assert.IsEmpty(factory.Backups.Storage.DownloadedTo);
    }

    [TestMethod]
    public async Task VerifyNowNeedsADestination()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        var result = await VerifyAsync(factory);

        Assert.AreEqual(BackupVerificationStatus.Failed, result.Verification.Status);
        Assert.AreEqual("The backup destination is not set.", result.Verification.Message);
        Assert.IsEmpty(factory.Backups.Created);
    }

    [TestMethod]
    public async Task OnlyAdministratorsCanVerify()
    {
        using var factory = await StartAsync();
        using var player = factory.Client(HubWebFactory.Player);
        using var anonymous = factory.Client();

        factory.Backups.Storage.Blobs[NewerBlob] = [1];

        var asPlayer = await player.PostAsync("/admin/backup/verify", new FormUrlEncodedContent([]));
        var asAnonymous = await anonymous.PostAsync("/admin/backup/verify", new FormUrlEncodedContent([]));

        Assert.AreNotEqual(HttpStatusCode.Redirect, asPlayer.StatusCode);
        Assert.IsFalse(asAnonymous.IsSuccessStatusCode);
        Assert.AreNotEqual(Page, asAnonymous.Headers.Location?.OriginalString);
        Assert.IsEmpty(factory.Backups.Storage.DownloadedTo);
        Assert.IsEmpty(factory.Backups.Created);
    }

    [TestMethod]
    public async Task TheVerifyButtonIsOnOnlyWithADestinationAndSqlite()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();

        var before = WebUtility.HtmlDecode(await client.GetStringAsync(Page));

        await factory.SaveAsync(new BackupSettings { ContainerUrl = ContainerUrl });

        var after = WebUtility.HtmlDecode(await client.GetStringAsync(Page));
        var isSqlite = await IsSqliteAsync(factory);

        Assert.Contains("action=\"/admin/backup/verify\"", before);
        Assert.Contains("Verify now", before);
        Assert.IsTrue(VerifyButtonDisabled(before));
        Assert.AreEqual(!isSqlite, VerifyButtonDisabled(after));
        Assert.AreEqual(!isSqlite, after.Contains("The verification is not available: it needs SQLite."));
    }

    [TestMethod]
    public async Task ASnapshotOfADatabaseInWalModeVerifies()
    {
        var source = Path.Combine(_work, "live.db");
        var snapshot = Path.Combine(_work, "snapshot.db");

        await using (var live = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = source, Pooling = false }.ConnectionString))
        {
            await live.OpenAsync();

            Execute(live, "pragma journal_mode=wal; create table Matches(Id integer); create table StatisticsFiles(Id integer); create table PlayerProfiles(Id integer); create table Settings(Id integer); insert into Matches values (1), (2); insert into Settings values (1);");

            await using var target = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = snapshot, Pooling = false }.ConnectionString);

            await target.OpenAsync();
            live.BackupDatabase(target);
        }

        var zipPath = Path.Combine(_work, "backup.zip");
        var counts = new BackupCounts { Matches = 2, Settings = 1 };

        await File.WriteAllBytesAsync(zipPath, Zip(await File.ReadAllBytesAsync(snapshot), new BackupManifest { IncludesDatabase = true, Files = ["hub.db"], Counts = counts }));

        var folder = Path.Combine(_work, "verify");

        Directory.CreateDirectory(folder);

        var verification = await BackupArchiveVerifier.VerifyAsync(zipPath, folder, new BackupTables("Matches", "StatisticsFiles", "PlayerProfiles", "Settings"), null, CancellationToken.None);

        Assert.AreEqual(BackupVerificationStatus.Passed, verification.Status, Describe(verification));
        CollectionAssert.AreEqual(new[] { "2", "0", "0", "1" }, verification.Arguments);
    }

    private static async Task<HubWebFactory> StartAsync(Action<BackupSettings>? change = null)
    {
        var factory = new HubWebFactory();
        var settings = new BackupSettings { Enabled = true, ContainerUrl = ContainerUrl };

        change?.Invoke(settings);

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(settings);

        return factory;
    }

    private static Task<bool> IsSqliteAsync(HubWebFactory factory)
    {
        return factory.RunAsync(services => Task.FromResult(services.GetRequiredService<HubDbContext>().Database.IsSqlite()));
    }

    private static async Task<BackupRun> RunAsync(HubWebFactory factory)
    {
        var run = await factory.RunAsync(services => services.GetRequiredService<IBackupService>().RunAsync(BackupTrigger.Manual));

        Assert.IsNotNull(run);

        return run;
    }

    private static async Task<BackupVerifyResult> VerifyAsync(HubWebFactory factory)
    {
        var result = await factory.RunAsync(services => services.GetRequiredService<IBackupService>().VerifyLatestAsync());

        Assert.IsNotNull(result);

        return result;
    }

    private static Task<BackupHistory> HistoryAsync(HubWebFactory factory)
    {
        return factory.RunAsync(services => services.GetRequiredService<IBackupHistoryService>().GetAsync());
    }

    private static void AssertRemoved(string downloadedZip)
    {
        Assert.IsFalse(File.Exists(downloadedZip));
        Assert.IsFalse(Directory.Exists(Path.GetDirectoryName(downloadedZip)));
    }

    private static string Describe(BackupVerification verification)
    {
        return string.Format(CultureInfo.InvariantCulture, verification.Message, verification.Arguments.Cast<object>().ToArray());
    }

    private static bool VerifyButtonDisabled(string html)
    {
        var form = html[html.IndexOf("action=\"/admin/backup/verify\"", StringComparison.Ordinal)..];
        var button = form[..form.IndexOf("</button>", StringComparison.Ordinal)];

        return button.Contains("disabled=\"disabled\"", StringComparison.Ordinal);
    }

    private static BackupManifest Manifest(byte[] zipContent)
    {
        using var zip = new ZipArchive(new MemoryStream(zipContent));
        using var stream = zip.GetEntry("manifest.json")!.Open();

        return JsonSerializer.Deserialize<BackupManifest>(stream)!;
    }

    private static byte[] ReplaceManifest(byte[] zipContent, BackupManifest manifest)
    {
        using var memory = new MemoryStream();

        memory.Write(zipContent);

        using (var zip = new ZipArchive(memory, ZipArchiveMode.Update, true))
        {
            zip.GetEntry("manifest.json")!.Delete();

            using var stream = zip.CreateEntry("manifest.json").Open();

            JsonSerializer.Serialize(stream, manifest);
        }

        return memory.ToArray();
    }

    private static byte[] Zip(byte[]? database, BackupManifest? manifest)
    {
        using var memory = new MemoryStream();

        using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, true))
        {
            if (database != null)
            {
                using var stream = zip.CreateEntry("hub.db").Open();

                stream.Write(database);
            }

            if (manifest != null)
            {
                using var stream = zip.CreateEntry("manifest.json").Open();

                JsonSerializer.Serialize(stream, manifest);
            }
        }

        return memory.ToArray();
    }

    private byte[] CorruptedDatabase()
    {
        var path = Path.Combine(_work, Guid.NewGuid().ToString("N") + ".db");
        const string value = "integrity-check-marker-value";

        using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = path, Pooling = false }.ConnectionString))
        {
            connection.Open();
            Execute(connection, $"pragma page_size=4096; create table Items(Name text); create index ItemsByName on Items(Name); insert into Items values ('{value}');");
        }

        var bytes = File.ReadAllBytes(path);
        var marker = Encoding.ASCII.GetBytes(value);
        var index = bytes.AsSpan(2 * 4096, 4096).IndexOf(marker);

        Assert.IsGreaterThanOrEqualTo(0, index);

        bytes[2 * 4096 + index] = (byte)'X';

        return bytes;
    }

    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();

        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
