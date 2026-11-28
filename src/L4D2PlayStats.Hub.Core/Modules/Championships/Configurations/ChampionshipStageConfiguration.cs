using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Configurations;

public class ChampionshipStageConfiguration : IEntityTypeConfiguration<ChampionshipStage>
{
    public void Configure(EntityTypeBuilder<ChampionshipStage> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(AppConsts.ChampionshipStageNameMaxLength).IsRequired();
        builder.Property(e => e.Tiebreakers).HasMaxLength(AppConsts.ChampionshipTiebreakersMaxLength).IsRequired();
        builder.HasIndex(e => new { e.ChampionshipId, e.Order });
        builder.HasMany(e => e.Groups).WithOne().HasForeignKey(e => e.StageId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.Series).WithOne(e => e.Stage).HasForeignKey(e => e.StageId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.Penalties).WithOne().HasForeignKey(e => e.StageId).OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(e => e.IsKnockout);
    }
}
