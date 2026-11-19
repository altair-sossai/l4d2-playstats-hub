using System.Data;
using System.Reflection;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Web.Infrastructure;
using L4D2PlayStats.Hub.Web.Infrastructure.Backups;
using L4D2PlayStats.Hub.Web.Models.Backups;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Web.Services;

public class BackupArchiveService(HubDbContext dbContext, HubPaths paths, IClock clock) : IBackupArchiveService
{
    public bool IncludesDatabase => dbContext.Database.IsSqlite();

    public async Task<BackupManifest> CreateAsync(string zipPath, CancellationToken cancellationToken = default)
    {
        var work = Path.Combine(Path.GetTempPath(), AppConsts.BackupTempFolder, Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(work);

        try
        {
            string? snapshot = null;

            if (IncludesDatabase)
            {
                snapshot = Path.Combine(work, AppConsts.BackupDatabaseEntry);
                await SnapshotAsync(snapshot, cancellationToken);
            }

            var manifest = new BackupManifest
            {
                HubVersion = Version(),
                CreatedAtUtc = clock.UtcNow,
                Provider = dbContext.Database.ProviderName ?? string.Empty
            };

            return BackupArchiveWriter.Write(zipPath, snapshot, paths.DataDirectory, manifest);
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    private async Task SnapshotAsync(string target, CancellationToken cancellationToken)
    {
        var source = (SqliteConnection)dbContext.Database.GetDbConnection();
        var opened = source.State != ConnectionState.Open;

        if (opened)
            await source.OpenAsync(cancellationToken);

        try
        {
            var builder = new SqliteConnectionStringBuilder { DataSource = target, Pooling = false };

            await using var destination = new SqliteConnection(builder.ConnectionString);

            await destination.OpenAsync(cancellationToken);
            source.BackupDatabase(destination);
        }
        finally
        {
            if (opened)
                await source.CloseAsync();
        }
    }

    private static string Version()
    {
        var version = typeof(BackupArchiveService).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        return string.IsNullOrWhiteSpace(version) ? AppConsts.BackupUnknownVersion : version;
    }
}
