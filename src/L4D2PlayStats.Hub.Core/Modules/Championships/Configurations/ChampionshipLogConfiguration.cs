using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Configurations;

public class ChampionshipLogConfiguration : IEntityTypeConfiguration<ChampionshipLog>
{
    public void Configure(EntityTypeBuilder<ChampionshipLog> builder)
    {
        builder.Property(e => e.Text).HasMaxLength(AppConsts.ChampionshipLogTextMaxLength).IsRequired();
        builder.Property(e => e.Values).HasMaxLength(AppConsts.ChampionshipLogValuesMaxLength);
        builder.HasIndex(e => new { e.ChampionshipId, e.CreatedAtUtc });
        builder.HasOne<Championship>().WithMany().HasForeignKey(e => e.ChampionshipId).OnDelete(DeleteBehavior.Cascade);
    }
}
