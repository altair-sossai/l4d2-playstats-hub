using System.Globalization;
using L4D2PlayStats.Hub.Web.Models.Backups;
using Microsoft.Data.Sqlite;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Backups;

public static class BackupCounter
{
    public static async Task<BackupCounts> CountAsync(SqliteConnection connection, SqliteTransaction? transaction, BackupTables tables, CancellationToken cancellationToken)
    {
        return new BackupCounts
        {
            Matches = await CountAsync(connection, transaction, tables.Matches, cancellationToken),
            StatisticsFiles = await CountAsync(connection, transaction, tables.StatisticsFiles, cancellationToken),
            Players = await CountAsync(connection, transaction, tables.Players, cancellationToken),
            Settings = await CountAsync(connection, transaction, tables.Settings, cancellationToken)
        };
    }

    private static async Task<long> CountAsync(SqliteConnection connection, SqliteTransaction? transaction, string table, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();

        command.Transaction = transaction;
        command.CommandText = string.Format(CultureInfo.InvariantCulture, AppConsts.BackupCountSqlFormat, table.Replace("\"", "\"\"", StringComparison.Ordinal));

        return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }
}
