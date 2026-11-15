using L4D2PlayStats.Hub.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace L4D2PlayStats.Hub.Migrations.PostgreSql;

public class PostgreSqlDesignTimeFactory : IDesignTimeDbContextFactory<HubDbContext>
{
    private const string DesignTimeConnectionString = "Host=localhost;Database=hub;Username=hub;Password=hub";

    public HubDbContext CreateDbContext(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var options = new DbContextOptionsBuilder<HubDbContext>()
            .UseNpgsql(DesignTimeConnectionString, npgsql => npgsql.MigrationsAssembly(typeof(PostgreSqlDesignTimeFactory).Assembly.GetName().Name))
            .Options;

        return new HubDbContext(options);
    }
}
