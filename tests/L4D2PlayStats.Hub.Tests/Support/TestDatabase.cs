using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Migrations.Sqlite;
using L4D2PlayStats.Hub.Web.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Tests.Support;

public sealed class TestDatabase : IDisposable
{
    public const string ProviderVariable = "HUB_TEST_DATABASE_PROVIDER";
    public const string ConnectionVariable = "HUB_TEST_DATABASE_CONNECTION";
    public const string DatabasePlaceholder = "{database}";

    private const string SqliteProvider = "Sqlite";

    private readonly SqliteConnection? _connection;
    private readonly string? _connectionString;

    public TestDatabase()
    {
        var provider = Environment.GetEnvironmentVariable(ProviderVariable);

        Provider = string.IsNullOrWhiteSpace(provider) ? SqliteProvider : provider.Trim();

        if (IsSqlite)
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            return;
        }

        var template = Environment.GetEnvironmentVariable(ConnectionVariable);

        if (string.IsNullOrWhiteSpace(template) || !template.Contains(DatabasePlaceholder))
            throw new InvalidOperationException($"{ConnectionVariable} must hold a connection string with {DatabasePlaceholder} in place of the database name.");

        _connectionString = template.Replace(DatabasePlaceholder, $"hub_test_{Guid.NewGuid():N}");
    }

    public string Provider { get; }

    public bool IsSqlite => Provider.Equals(SqliteProvider, StringComparison.OrdinalIgnoreCase);

    public void Dispose()
    {
        if (_connection != null)
        {
            _connection.Dispose();
            return;
        }

        using var dbContext = CreateContext();

        dbContext.Database.EnsureDeleted();
    }

    public void Configure(DbContextOptionsBuilder options)
    {
        if (_connection != null)
        {
            options.UseSqlite(_connection, sqlite => sqlite.MigrationsAssembly(typeof(SqliteDesignTimeFactory).Assembly.GetName().Name));
            return;
        }

        HubDatabase.Configure(options, Provider, _connectionString!);
    }

    public HubDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HubDbContext>();

        Configure(options);

        return new HubDbContext(options.Options);
    }
}
