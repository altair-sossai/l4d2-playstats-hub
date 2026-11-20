using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;
using L4D2PlayStats.Hub.Core.Modules.Backups.Models;
using L4D2PlayStats.Hub.Web.Models.Backups;
using Microsoft.Data.Sqlite;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Backups;

public static class BackupArchiveVerifier
{
    public static async Task<BackupVerification> VerifyAsync(string zipPath, string workFolder, BackupTables tables, long? expectedSize, CancellationToken cancellationToken)
    {
        var size = new FileInfo(zipPath).Length;

        if (expectedSize is > 0 && size != expectedSize.Value)
            return Failed(AppConsts.BackupVerifySizeMessage, Number(size), Number(expectedSize.Value));

        var databasePath = Path.Combine(workFolder, AppConsts.BackupDatabaseEntry);
        BackupManifest? manifest;

        await using (var zip = await ZipFile.OpenReadAsync(zipPath, cancellationToken))
        {
            var manifestEntry = zip.GetEntry(AppConsts.BackupManifestEntry);

            if (manifestEntry == null)
                return Failed(AppConsts.BackupVerifyNoManifestMessage);

            manifest = await ReadManifestAsync(manifestEntry, cancellationToken);

            if (manifest == null)
                return Failed(AppConsts.BackupVerifyNoManifestMessage);

            var missing = manifest.Files.FirstOrDefault(file => zip.GetEntry(file) == null);

            if (missing != null)
                return Failed(AppConsts.BackupVerifyMissingFileMessage, missing);

            var database = zip.GetEntry(AppConsts.BackupDatabaseEntry);

            if (!manifest.IncludesDatabase || database == null)
                return Failed(AppConsts.BackupVerifyNoDatabaseMessage);

            foreach (var entry in zip.Entries.Where(entry => entry != database && entry != manifestEntry))
                await ReadThroughAsync(entry, cancellationToken);

            await database.ExtractToFileAsync(databasePath, cancellationToken);
        }

        return await CheckDatabaseAsync(databasePath, tables, manifest.Counts, cancellationToken);
    }

    private static async Task<BackupManifest?> ReadManifestAsync(ZipArchiveEntry entry, CancellationToken cancellationToken)
    {
        await using var stream = await entry.OpenAsync(cancellationToken);

        return await JsonSerializer.DeserializeAsync<BackupManifest>(stream, cancellationToken: cancellationToken);
    }

    private static async Task ReadThroughAsync(ZipArchiveEntry entry, CancellationToken cancellationToken)
    {
        await using var stream = await entry.OpenAsync(cancellationToken);

        await stream.CopyToAsync(Stream.Null, cancellationToken);
    }

    private static async Task<BackupVerification> CheckDatabaseAsync(string databasePath, BackupTables tables, BackupCounts? expected, CancellationToken cancellationToken)
    {
        var builder = new SqliteConnectionStringBuilder { DataSource = databasePath, Mode = SqliteOpenMode.ReadOnly, Pooling = false };

        await using var connection = new SqliteConnection(builder.ConnectionString);

        await connection.OpenAsync(cancellationToken);

        var integrity = await IntegrityAsync(connection, cancellationToken);

        if (!string.Equals(integrity, AppConsts.BackupIntegrityOk, StringComparison.OrdinalIgnoreCase))
            return Failed(AppConsts.BackupVerifyIntegrityMessage, BackupErrors.Limit(integrity ?? string.Empty));

        if (expected == null)
            return Passed(AppConsts.BackupVerifyPassedWithoutCountsMessage);

        var actual = await BackupCounter.CountAsync(connection, null, tables, cancellationToken);
        var comparisons = new[]
        {
            (Table: tables.Matches, Actual: actual.Matches, Expected: expected.Matches),
            (Table: tables.StatisticsFiles, Actual: actual.StatisticsFiles, Expected: expected.StatisticsFiles),
            (Table: tables.Players, Actual: actual.Players, Expected: expected.Players),
            (Table: tables.Settings, Actual: actual.Settings, Expected: expected.Settings)
        };

        var mismatch = comparisons.FirstOrDefault(comparison => comparison.Actual != comparison.Expected);

        if (mismatch.Table != null)
            return Failed(AppConsts.BackupVerifyCountMessage, mismatch.Table, Number(mismatch.Actual), Number(mismatch.Expected));

        return Passed(AppConsts.BackupVerifyPassedMessage, Number(actual.Matches), Number(actual.StatisticsFiles), Number(actual.Players), Number(actual.Settings));
    }

    private static async Task<string?> IntegrityAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();

        command.CommandText = AppConsts.BackupIntegrityCheckSql;

        return Convert.ToString(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    private static string Number(long value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    private static BackupVerification Passed(string message, params string[] arguments)
    {
        return BackupVerification.Of(BackupVerificationStatus.Passed, message, arguments);
    }

    private static BackupVerification Failed(string message, params string[] arguments)
    {
        return BackupVerification.Of(BackupVerificationStatus.Failed, message, arguments);
    }
}