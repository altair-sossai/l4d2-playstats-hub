using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Configurations;

public class ChampionshipJoinRequestConfiguration : IEntityTypeConfiguration<ChampionshipJoinRequest>
{
    public void Configure(EntityTypeBuilder<ChampionshipJoinRequest> builder)
    {
        builder.Property(e => e.Message).HasMaxLength(AppConsts.ChampionshipJoinMessageMaxLength);
        builder.HasIndex(e => new { e.ChampionshipId, e.CommunityId }).IsUnique();
        builder.HasIndex(e => new { e.ChampionshipId, e.Status });
        builder.HasOne<Championship>().WithMany().HasForeignKey(e => e.ChampionshipId).OnDelete(DeleteBehavior.Cascade);
    }
}
