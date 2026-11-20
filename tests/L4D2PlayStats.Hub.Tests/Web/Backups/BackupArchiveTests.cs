using System.IO.Compression;
using System.Text;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Web.Infrastructure.Backups;
using L4D2PlayStats.Hub.Web.Models.Backups;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Backups;

[TestClass]
public sealed class BackupArchiveTests : IDisposable
{
    private readonly string _work = Path.Combine(Path.GetTempPath(), "l4d2-playstats-hub-backup-tests", Guid.NewGuid().ToString("N"));

    public BackupArchiveTests()
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
    public async Task TheZipHasTheDatabaseTheKeysTheLogoAndTheManifestButNotTheAvatars()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        Write(factory.DataDirectory, "keys/key-0f7a.xml", "<key/>");
        Write(factory.DataDirectory, "branding/logo.png", "png");
        Write(factory.DataDirectory, "avatars/76561198000000100.jpg", "jpg");

        var zipPath = Path.Combine(_work, "backup.zip");
        var (manifest, isSqlite) = await factory.RunAsync(async services =>
        {
            var created = await services.GetRequiredService<IBackupArchiveService>().CreateAsync(zipPath);

            return (created, services.GetRequiredService<HubDbContext>().Database.IsSqlite());
        });

        using var zip = ZipFile.OpenRead(zipPath);

        var entries = zip.Entries.Select(entry => entry.FullName).ToList();

        Assert.Contains("keys/key-0f7a.xml", entries);
        Assert.Contains("branding/logo.png", entries);
        Assert.Contains("manifest.json", entries);
        Assert.IsFalse(entries.Any(entry => entry.StartsWith("avatars", StringComparison.Ordinal)));
        Assert.AreEqual(isSqlite, entries.Contains("hub.db"));
        Assert.AreEqual(isSqlite, manifest.IncludesDatabase);
        Assert.AreEqual("<key/>", Read(zip, "keys/key-0f7a.xml"));
    }

    [TestMethod]
    public async Task TheDatabaseInTheZipOpensAndHasTheDataOfTheSite()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.CreateServerAsync("Backup Server");

        var zipPath = Path.Combine(_work, "backup.zip");
        var isSqlite = await factory.RunAsync(async services =>
        {
            await services.GetRequiredService<IBackupArchiveService>().CreateAsync(zipPath);

            return services.GetRequiredService<HubDbContext>().Database.IsSqlite();
        });

        if (!isSqlite)
            return;

        var database = Path.Combine(_work, "restored.db");

        using (var zip = ZipFile.OpenRead(zipPath))
        {
            zip.GetEntry("hub.db")!.ExtractToFile(database);
        }

        await using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = database, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ConnectionString);

        await connection.OpenAsync();

        Assert.AreEqual(1L, Scalar(connection, "select count(*) from Administrators"));
        Assert.AreEqual(1L, Scalar(connection, "select count(*) from GameServers where Name = 'Backup Server'"));
        Assert.AreEqual(1L, Scalar(connection, "select count(*) from Settings where Key = 'SetupSettings'"));
        Assert.AreEqual("ok", Scalar(connection, "pragma integrity_check"));
    }

    [TestMethod]
    public async Task TheZipHasNeitherTheSecretsKeyNorAnyPlainSecret()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new SiteSettings { SteamApiKey = "STEAMKEY0123456789ABCDEF" });

        Write(factory.DataDirectory, "keys/key-0f7a.xml", "<key/>");

        var zipPath = Path.Combine(_work, "backup.zip");

        await factory.RunAsync(services => services.GetRequiredService<IBackupArchiveService>().CreateAsync(zipPath));

        var keyText = File.ReadAllText(factory.SecretsKeyFile);

        using var zip = ZipFile.OpenRead(zipPath);

        Assert.IsFalse(zip.Entries.Any(entry => entry.Name == "secrets.key"));

        foreach (var entry in zip.Entries)
        {
            var content = Bytes(entry);

            Assert.IsFalse(Contains(content, Encoding.ASCII.GetBytes(keyText)), entry.FullName);
            Assert.IsFalse(Contains(content, Convert.FromBase64String(keyText)), entry.FullName);
            Assert.IsFalse(Contains(content, Encoding.UTF8.GetBytes("STEAMKEY0123456789ABCDEF")), entry.FullName);
        }
    }

    [TestMethod]
    public void AKeyFileKeptInsideTheDataFolderIsLeftOut()
    {
        var data = Path.Combine(_work, "data");

        Write(data, "keys/key-1.xml", "<key/>");
        Write(data, "keys/secrets.key", "secret");

        var key = new SecretsKey(new byte[32], Path.Combine(data, "keys", "secrets.key"), Path.Combine(data, "keys", "secrets.key"), false, null);
        var zipPath = Path.Combine(_work, "backup.zip");
        var manifest = BackupArchiveWriter.Write(zipPath, null, data, new BackupManifest(), key);

        using var zip = ZipFile.OpenRead(zipPath);

        CollectionAssert.AreEqual(new[] { "keys/key-1.xml", "manifest.json" }, zip.Entries.Select(entry => entry.FullName).Order().ToList());
        CollectionAssert.AreEqual(new[] { "keys/key-1.xml" }, manifest.Files);
    }

    [TestMethod]
    public async Task TheManifestListsTheVersionTheTimeTheProviderAndTheFiles()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        Write(factory.DataDirectory, "branding/logo.png", "png");

        var zipPath = Path.Combine(_work, "backup.zip");

        await factory.RunAsync(services => services.GetRequiredService<IBackupArchiveService>().CreateAsync(zipPath));

        using var zip = ZipFile.OpenRead(zipPath);

        var manifest = JsonSerializer.Deserialize<BackupManifest>(Read(zip, "manifest.json"))!;
        var files = zip.Entries.Select(entry => entry.FullName).Where(name => name != "manifest.json").Order().ToList();

        Assert.IsFalse(string.IsNullOrWhiteSpace(manifest.HubVersion));
        Assert.IsFalse(string.IsNullOrWhiteSpace(manifest.Provider));
        Assert.IsLessThan(TimeSpan.FromMinutes(5), (DateTime.UtcNow - manifest.CreatedAtUtc).Duration());
        CollectionAssert.AreEqual(files, manifest.Files.Order().ToList());
    }

    [TestMethod]
    public void WithoutSqliteTheZipHasTheKeysAndTheLogoButNoDatabase()
    {
        var data = Path.Combine(_work, "data");

        Write(data, "keys/key-1.xml", "<key/>");
        Write(data, "branding/logo.webp", "webp");
        Write(data, "avatars/1.jpg", "jpg");
        Write(data, "hub.db", "live database");

        var zipPath = Path.Combine(_work, "backup.zip");
        var manifest = BackupArchiveWriter.Write(zipPath, null, data, new BackupManifest { HubVersion = "1.0.0", Provider = "Npgsql.EntityFrameworkCore.PostgreSQL" });

        using var zip = ZipFile.OpenRead(zipPath);

        var entries = zip.Entries.Select(entry => entry.FullName).Order().ToList();

        CollectionAssert.AreEqual(new[] { "branding/logo.webp", "keys/key-1.xml", "manifest.json" }, entries);
        Assert.IsFalse(manifest.IncludesDatabase);
        CollectionAssert.AreEqual(new[] { "keys/key-1.xml", "branding/logo.webp" }, manifest.Files);
    }

    [TestMethod]
    public void TheLiveDatabaseFileIsNeverCopied()
    {
        var data = Path.Combine(_work, "data");
        var snapshot = Path.Combine(_work, "snapshot.db");

        Write(data, "hub.db", "live database");
        File.WriteAllText(snapshot, "snapshot");

        var zipPath = Path.Combine(_work, "backup.zip");

        BackupArchiveWriter.Write(zipPath, snapshot, data, new BackupManifest());

        using var zip = ZipFile.OpenRead(zipPath);

        Assert.AreEqual("snapshot", Read(zip, "hub.db"));
    }

    [TestMethod]
    public void FoldersThatDoNotExistAreLeftOut()
    {
        var data = Path.Combine(_work, "empty");

        Directory.CreateDirectory(data);

        var zipPath = Path.Combine(_work, "backup.zip");
        var manifest = BackupArchiveWriter.Write(zipPath, null, data, new BackupManifest());

        using var zip = ZipFile.OpenRead(zipPath);

        CollectionAssert.AreEqual(new[] { "manifest.json" }, zip.Entries.Select(entry => entry.FullName).ToList());
        Assert.IsEmpty(manifest.Files);
    }

    private static void Write(string root, string relative, string content)
    {
        var path = Path.Combine(root, relative);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    private static string Read(ZipArchive zip, string name)
    {
        using var reader = new StreamReader(zip.GetEntry(name)!.Open());

        return reader.ReadToEnd();
    }

    private static byte[] Bytes(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var memory = new MemoryStream();

        stream.CopyTo(memory);

        return memory.ToArray();
    }

    private static bool Contains(byte[] content, byte[] value)
    {
        return content.AsSpan().IndexOf(value) >= 0;
    }

    private static object? Scalar(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();

        command.CommandText = sql;

        return command.ExecuteScalar();
    }
}
