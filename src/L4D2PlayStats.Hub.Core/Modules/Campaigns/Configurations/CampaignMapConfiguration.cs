using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Campaigns.Configurations;

public class CampaignMapConfiguration : IEntityTypeConfiguration<CampaignMap>
{
    public void Configure(EntityTypeBuilder<CampaignMap> builder)
    {
        builder.Property(e => e.MapName).HasMaxLength(AppConsts.MapNameMaxLength);
        builder.HasIndex(e => e.MapName);
    }
}
