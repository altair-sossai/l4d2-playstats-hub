using L4D2PlayStats.Hub.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace L4D2PlayStats.Hub.Migrations.SqlServer;

public class SqlServerDesignTimeFactory : IDesignTimeDbContextFactory<HubDbContext>
{
    private const string DesignTimeConnectionString = "Server=localhost;Database=hub;Trusted_Connection=True;TrustServerCertificate=True";

    public HubDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<HubDbContext>()
            .UseSqlServer(DesignTimeConnectionString, sqlServer => sqlServer.MigrationsAssembly(typeof(SqlServerDesignTimeFactory).Assembly.GetName().Name))
            .Options;

        return new HubDbContext(options);
    }
}
