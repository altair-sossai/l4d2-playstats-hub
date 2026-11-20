using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Campaigns.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(AppConsts.CampaignNameMaxLength);
        builder.HasMany(e => e.Maps).WithOne().HasForeignKey(e => e.CampaignId).OnDelete(DeleteBehavior.Cascade);
    }
}
