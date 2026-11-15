using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class MigrationTests
{
    [TestMethod]
    public async Task TheMigrationsCreateTheWholeDatabase()
    {
        using var database = new TestDatabase();
        await using var dbContext = database.CreateContext();

        await dbContext.Database.MigrateAsync();

        var pending = await dbContext.Database.GetPendingMigrationsAsync();

        Assert.IsEmpty(pending);
        Assert.AreEqual(0, await dbContext.GameServers.CountAsync());
        Assert.AreEqual(0, await dbContext.Matches.CountAsync());
        Assert.AreEqual(0, await dbContext.StatisticsFiles.CountAsync());
    }

    [TestMethod]
    public void TheMigrationsAreUpToDateWithTheModel()
    {
        using var database = new TestDatabase();
        using var dbContext = database.CreateContext();

        Assert.IsFalse(dbContext.Database.HasPendingModelChanges(), $"The model changed. Add a migration for {database.Provider}.");
    }

    [TestMethod]
    public async Task RunningTheMigrationsTwiceChangesNothing()
    {
        using var database = new TestDatabase();
        await using var dbContext = database.CreateContext();

        await dbContext.Database.MigrateAsync();
        await dbContext.Database.MigrateAsync();

        Assert.IsNotEmpty(await dbContext.Database.GetAppliedMigrationsAsync());
    }
}
