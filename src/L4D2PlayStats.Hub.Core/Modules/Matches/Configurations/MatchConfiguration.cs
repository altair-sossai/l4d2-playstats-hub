using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Matches.Configurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.Property(e => e.Campaign).HasMaxLength(AppConsts.CampaignNameMaxLength);
        builder.Property(e => e.ConfigurationName).HasMaxLength(AppConsts.ConfigurationNameMaxLength);
        builder.HasIndex(e => new { e.SeasonId, e.LastFileDate });
        builder.HasIndex(e => e.LastFileDate);
        builder.HasOne(e => e.GameServer).WithMany().HasForeignKey(e => e.GameServerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Season).WithMany().HasForeignKey(e => e.SeasonId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.Players).WithOne(p => p.Match).HasForeignKey(p => p.MatchId).OnDelete(DeleteBehavior.Cascade);
    }
}