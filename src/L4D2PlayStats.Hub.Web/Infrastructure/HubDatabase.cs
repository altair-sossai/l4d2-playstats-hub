using L4D2PlayStats.Hub.Migrations.MySql;
using L4D2PlayStats.Hub.Migrations.PostgreSql;
using L4D2PlayStats.Hub.Migrations.Sqlite;
using L4D2PlayStats.Hub.Migrations.SqlServer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Infrastructure;

public static class HubDatabase
{
    public static void Configure(DbContextOptionsBuilder options, string provider, string connectionString)
    {
        if (Is(provider, CoreConsts.SqliteProvider))
        {
            options.UseSqlite(connectionString, sqlite => sqlite.MigrationsAssembly(typeof(SqliteDesignTimeFactory).Assembly.GetName().Name));
            return;
        }

        if (Is(provider, CoreConsts.SqlServerProvider))
        {
            options.UseSqlServer(connectionString, sqlServer => sqlServer.MigrationsAssembly(typeof(SqlServerDesignTimeFactory).Assembly.GetName().Name));
            return;
        }

        if (Is(provider, CoreConsts.PostgreSqlProvider))
        {
            AppContext.SetSwitch(AppConsts.NpgsqlLegacyTimestampSwitch, true);
            options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(PostgreSqlDesignTimeFactory).Assembly.GetName().Name));
            return;
        }

        if (Is(provider, CoreConsts.MySqlProvider))
        {
            options.UseMySQL(connectionString, mySql => mySql.MigrationsAssembly(typeof(MySqlDesignTimeFactory).Assembly.GetName().Name));
            return;
        }

        throw new NotSupportedException($"Database provider '{provider}' is not supported. Use {CoreConsts.SqliteProvider}, {CoreConsts.SqlServerProvider}, {CoreConsts.PostgreSqlProvider} or {CoreConsts.MySqlProvider}.");
    }

    public static string PrepareConnectionString(string provider, string connectionString, string contentRoot)
    {
        if (!Is(provider, CoreConsts.SqliteProvider))
            return connectionString;

        var builder = new SqliteConnectionStringBuilder(connectionString);

        if (string.IsNullOrEmpty(builder.DataSource) || builder.DataSource == AppConsts.SqliteMemoryDataSource)
            return connectionString;

        if (!Path.IsPathRooted(builder.DataSource))
            builder.DataSource = Path.Combine(contentRoot, builder.DataSource);

        var directory = Path.GetDirectoryName(builder.DataSource);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        return builder.ConnectionString;
    }

    private static bool Is(string provider, string expected)
    {
        return provider.Trim().Equals(expected, StringComparison.OrdinalIgnoreCase);
    }
}
