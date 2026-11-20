using L4D2PlayStats.Hub.Core.Modules.Administrators;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat;
using L4D2PlayStats.Hub.Core.Modules.Campaigns;
using L4D2PlayStats.Hub.Core.Modules.GameModes;
using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Players;
using L4D2PlayStats.Hub.Core.Modules.Punishments;
using L4D2PlayStats.Hub.Core.Modules.Seasons;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Data;

public class HubDbContext(DbContextOptions<HubDbContext> options) : DbContext(options)
{
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<Administrator> Administrators => Set<Administrator>();
    public DbSet<GameServer> GameServers => Set<GameServer>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignMap> CampaignMaps => Set<CampaignMap>();
    public DbSet<GameModeRule> GameModeRules => Set<GameModeRule>();
    public DbSet<GameConfiguration> GameConfigurations => Set<GameConfiguration>();
    public DbSet<StatisticsFile> StatisticsFiles => Set<StatisticsFile>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchPlayer> MatchPlayers => Set<MatchPlayer>();
    public DbSet<PlayerProfile> PlayerProfiles => Set<PlayerProfile>();
    public DbSet<Punishment> Punishments => Set<Punishment>();
    public DbSet<SkinAuthor> SkinAuthors => Set<SkinAuthor>();
    public DbSet<AntiCheatAccessSetting> AntiCheatAccessSettings => Set<AntiCheatAccessSetting>();
    public DbSet<AntiCheatWhitelistPlayer> AntiCheatWhitelist => Set<AntiCheatWhitelistPlayer>();
    public DbSet<AntiCheatRequiredPlayer> AntiCheatRequired => Set<AntiCheatRequiredPlayer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HubDbContext).Assembly);
    }
}