using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Bans.Configurations;

public class BanAlertConfiguration : IEntityTypeConfiguration<BanAlert>
{
    public void Configure(EntityTypeBuilder<BanAlert> builder)
    {
        builder.ToTable("BanAlerts");
        builder.HasIndex(e => new { e.BanId, e.CommunityId }).IsUnique();
        builder.HasIndex(e => new { e.Status, e.CommunityId });
        builder.HasOne<Ban>().WithMany().HasForeignKey(e => e.BanId).OnDelete(DeleteBehavior.Cascade);
    }
}
