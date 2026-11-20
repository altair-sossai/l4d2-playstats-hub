using L4D2PlayStats.Hub.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace L4D2PlayStats.Hub.Migrations.MySql;

public class MySqlDesignTimeFactory : IDesignTimeDbContextFactory<HubDbContext>
{
    private const string DesignTimeConnectionString = "Server=localhost;Database=hub;User=hub;Password=hub";

    public HubDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<HubDbContext>()
            .UseMySQL(DesignTimeConnectionString, mySql => mySql.MigrationsAssembly(typeof(MySqlDesignTimeFactory).Assembly.GetName().Name))
            .Options;

        return new HubDbContext(options);
    }
}
