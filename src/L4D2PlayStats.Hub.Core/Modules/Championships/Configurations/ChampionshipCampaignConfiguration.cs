using L4D2PlayStats.Hub.Core.Modules.Campaigns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Configurations;

public class ChampionshipCampaignConfiguration : IEntityTypeConfiguration<ChampionshipCampaign>
{
    public void Configure(EntityTypeBuilder<ChampionshipCampaign> builder)
    {
        builder.HasIndex(e => new { e.ChampionshipId, e.CampaignId }).IsUnique();
        builder.HasOne<Campaign>().WithMany().HasForeignKey(e => e.CampaignId).OnDelete(DeleteBehavior.Cascade);
    }
}
