using L4D2PlayStats.Hub.Core.Modules.Administrators;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasKey(e => e.Key);
            entity.Property(e => e.Key).HasMaxLength(100);
        });

        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.HasKey(e => e.CommunityId);
            entity.Property(e => e.CommunityId).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<GameServer>(entity =>
        {
            entity.Property(e => e.PublicId).HasMaxLength(60);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.AlternativeAddress).HasMaxLength(200);
            entity.Property(e => e.KeyHash).HasMaxLength(100);
            entity.HasIndex(e => e.PublicId).IsUnique();
        });

        modelBuilder.Entity<Season>(entity => { entity.HasIndex(e => e.StartsAt).IsUnique(); });

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.HasMany(e => e.Maps).WithOne().HasForeignKey(e => e.CampaignId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CampaignMap>(entity =>
        {
            entity.Property(e => e.MapName).HasMaxLength(100);
            entity.HasIndex(e => e.MapName);
        });

        modelBuilder.Entity<GameModeRule>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ConfigurationPattern).HasMaxLength(100);
        });

        modelBuilder.Entity<GameConfiguration>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.HasIndex(e => new { e.Name, e.TeamSize }).IsUnique();
        });

        modelBuilder.Entity<StatisticsFile>(entity =>
        {
            entity.Property(e => e.FileName).HasMaxLength(200);
            entity.Property(e => e.ConfigurationName).HasMaxLength(100);
            entity.Property(e => e.MapName).HasMaxLength(100);
            entity.HasIndex(e => new { e.GameServerId, e.FileName }).IsUnique();
            entity.HasIndex(e => new { e.GameServerId, e.SeasonId, e.FileDate });
            entity.HasOne(e => e.GameServer).WithMany().HasForeignKey(e => e.GameServerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Season>().WithMany().HasForeignKey(e => e.SeasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Match).WithMany(m => m.Files).HasForeignKey(e => e.MatchId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Match>(entity =>
        {
            entity.Property(e => e.Campaign).HasMaxLength(100);
            entity.Property(e => e.ConfigurationName).HasMaxLength(100);
            entity.HasIndex(e => new { e.SeasonId, e.LastFileDate });
            entity.HasIndex(e => e.LastFileDate);
            entity.HasOne(e => e.GameServer).WithMany().HasForeignKey(e => e.GameServerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Season).WithMany().HasForeignKey(e => e.SeasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.Players).WithOne(p => p.Match).HasForeignKey(p => p.MatchId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MatchPlayer>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Ignore(e => e.Teams);
            entity.HasIndex(e => e.CommunityId);
        });

        modelBuilder.Entity<PlayerProfile>(entity =>
        {
            entity.HasKey(e => e.CommunityId);
            entity.Property(e => e.CommunityId).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Punishment>(entity =>
        {
            entity.Property(e => e.Reason).HasMaxLength(300);
            entity.HasIndex(e => new { e.SeasonId, e.CommunityId });
            entity.HasOne<Season>().WithMany().HasForeignKey(e => e.SeasonId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HubDbContext).Assembly);
    }
}
