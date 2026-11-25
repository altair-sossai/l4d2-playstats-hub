using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Configurations;

public class ChampionshipPenaltyConfiguration : IEntityTypeConfiguration<ChampionshipPenalty>
{
    public void Configure(EntityTypeBuilder<ChampionshipPenalty> builder)
    {
        builder.Property(e => e.Reason).HasMaxLength(AppConsts.ChampionshipReasonMaxLength);
        builder.HasOne<ChampionshipTeam>().WithMany().HasForeignKey(e => e.TeamId).OnDelete(DeleteBehavior.NoAction);
    }
}
