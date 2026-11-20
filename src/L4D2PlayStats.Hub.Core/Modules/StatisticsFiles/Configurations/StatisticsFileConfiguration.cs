using L4D2PlayStats.Hub.Core.Modules.Seasons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Configurations;

public class StatisticsFileConfiguration : IEntityTypeConfiguration<StatisticsFile>
{
    public void Configure(EntityTypeBuilder<StatisticsFile> builder)
    {
        builder.Property(e => e.FileName).HasMaxLength(AppConsts.StatisticsFileNameMaxLength);
        builder.Property(e => e.ConfigurationName).HasMaxLength(AppConsts.ConfigurationNameMaxLength);
        builder.Property(e => e.MapName).HasMaxLength(AppConsts.MapNameMaxLength);
        builder.HasIndex(e => new { e.GameServerId, e.FileName }).IsUnique();
        builder.HasIndex(e => new { e.GameServerId, e.SeasonId, e.FileDate });
        builder.HasOne(e => e.GameServer).WithMany().HasForeignKey(e => e.GameServerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Season>().WithMany().HasForeignKey(e => e.SeasonId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Match).WithMany(m => m.Files).HasForeignKey(e => e.MatchId).OnDelete(DeleteBehavior.SetNull);
    }
}
