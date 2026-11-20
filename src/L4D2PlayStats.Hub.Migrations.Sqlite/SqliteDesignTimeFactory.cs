using L4D2PlayStats.Hub.Core;
using L4D2PlayStats.Hub.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace L4D2PlayStats.Hub.Migrations.Sqlite;

public class SqliteDesignTimeFactory : IDesignTimeDbContextFactory<HubDbContext>
{
    public HubDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<HubDbContext>()
            .UseSqlite(AppConsts.DefaultConnectionString, sqlite => sqlite.MigrationsAssembly(typeof(SqliteDesignTimeFactory).Assembly.GetName().Name))
            .Options;

        return new HubDbContext(options);
    }
}
