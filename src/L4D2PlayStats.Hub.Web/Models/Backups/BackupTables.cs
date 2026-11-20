using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Players;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace L4D2PlayStats.Hub.Web.Models.Backups;

public record BackupTables(string Matches, string StatisticsFiles, string Players, string Settings)
{
    public static BackupTables From(IModel model)
    {
        return new BackupTables(Table<Match>(model), Table<StatisticsFile>(model), Table<PlayerProfile>(model), Table<Setting>(model));
    }

    private static string Table<T>(IModel model)
    {
        return model.FindEntityType(typeof(T))?.GetTableName() ?? typeof(T).Name;
    }
}