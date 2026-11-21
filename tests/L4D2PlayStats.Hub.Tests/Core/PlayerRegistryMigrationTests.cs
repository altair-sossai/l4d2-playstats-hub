using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class PlayerRegistryMigrationTests
{
    private const string ForcedNamesMigration = "20261121150721_ForcedNames";
    private const long Known = 76561198000000601;
    private const long Unknown = 76561198000000602;
    private const long Administrator = 76561198000000603;

    [TestMethod]
    public async Task TheMigrationMovesTheForcedNamesIntoThePlayerProfilesAndDropsTheOldTable()
    {
        using var database = new TestDatabase();

        if (!database.IsSqlite)
            Assert.Inconclusive("The data copy is checked with SQLite.");

        await using var dbContext = database.CreateContext();

        await dbContext.GetService<IMigrator>().MigrateAsync(ForcedNamesMigration);

        await dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO \"PlayerProfiles\" (\"CommunityId\", \"Name\", \"FirstSeenAtUtc\", \"LastSeenAtUtc\") VALUES ({Known}, 'InGame', '2026-01-01 10:00:00', '2026-11-01 10:00:00')");
        await dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO \"ForcedName\" (\"CommunityId\", \"Name\", \"UpdatedAtUtc\", \"UpdatedBy\") VALUES ({Known}, 'Forced Known', '2026-11-10 12:00:00', {Administrator})");
        await dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO \"ForcedName\" (\"CommunityId\", \"Name\", \"UpdatedAtUtc\", \"UpdatedBy\") VALUES ({Unknown}, 'Forced Unknown', '2026-11-11 08:30:00', NULL)");

        await dbContext.Database.MigrateAsync();

        var profiles = await dbContext.PlayerProfiles.AsNoTracking().OrderBy(profile => profile.CommunityId).ToListAsync();
        var tables = await dbContext.Database.SqlQueryRaw<string>("SELECT \"name\" AS \"Value\" FROM \"sqlite_master\" WHERE \"type\" = 'table' AND \"name\" = 'ForcedName'").ToListAsync();

        Assert.HasCount(2, profiles);
        Assert.AreEqual("Forced Known", profiles[0].ForcedName);
        Assert.AreEqual(new DateTime(2026, 11, 10, 12, 0, 0), profiles[0].ForcedNameUpdatedAtUtc);
        Assert.AreEqual(Administrator, profiles[0].ForcedNameUpdatedBy);
        Assert.AreEqual("InGame", profiles[0].Name);
        Assert.AreEqual(new DateTime(2026, 1, 1, 10, 0, 0), profiles[0].FirstSeenAtUtc);
        Assert.AreEqual(new DateTime(2026, 11, 1, 10, 0, 0), profiles[0].LastSeenAtUtc);
        Assert.AreEqual("Forced Unknown", profiles[1].ForcedName);
        Assert.AreEqual(new DateTime(2026, 11, 11, 8, 30, 0), profiles[1].ForcedNameUpdatedAtUtc);
        Assert.IsNull(profiles[1].ForcedNameUpdatedBy);
        Assert.IsNull(profiles[1].Name);
        Assert.AreEqual(new DateTime(2026, 11, 11, 8, 30, 0), profiles[1].FirstSeenAtUtc);
        Assert.AreEqual(new DateTime(2026, 11, 11, 8, 30, 0), profiles[1].LastSeenAtUtc);
        Assert.IsEmpty(tables);
    }

    [TestMethod]
    public async Task MigratingBackPutsTheForcedNamesInTheirOwnTableAgain()
    {
        using var database = new TestDatabase();

        if (!database.IsSqlite)
            Assert.Inconclusive("The data copy is checked with SQLite.");

        await using var dbContext = database.CreateContext();

        await dbContext.Database.MigrateAsync();
        await dbContext.Database.ExecuteSqlRawAsync($"INSERT INTO \"PlayerProfiles\" (\"CommunityId\", \"ForcedName\", \"ForcedNameUpdatedAtUtc\", \"FirstSeenAtUtc\", \"LastSeenAtUtc\") VALUES ({Known}, 'Forced Known', '2026-11-10 12:00:00', '2026-01-01 10:00:00', '2026-11-01 10:00:00')");

        await dbContext.GetService<IMigrator>().MigrateAsync(ForcedNamesMigration);

        var names = await dbContext.Database.SqlQueryRaw<string>("SELECT \"Name\" AS \"Value\" FROM \"ForcedName\"").ToListAsync();

        CollectionAssert.AreEqual(new[] { "Forced Known" }, names);
    }
}
