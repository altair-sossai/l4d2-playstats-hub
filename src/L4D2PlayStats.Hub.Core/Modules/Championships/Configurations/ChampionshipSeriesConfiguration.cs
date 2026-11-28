using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Configurations;

public class ChampionshipSeriesConfiguration : IEntityTypeConfiguration<ChampionshipSeries>
{
    public void Configure(EntityTypeBuilder<ChampionshipSeries> builder)
    {
        builder.Property(e => e.Notes).HasMaxLength(AppConsts.ChampionshipNotesMaxLength);
        builder.HasIndex(e => new { e.StageId, e.Round, e.Position });
        builder.HasIndex(e => e.TeamAId);
        builder.HasIndex(e => e.TeamBId);
        builder.HasMany(e => e.Games).WithOne().HasForeignKey(e => e.SeriesId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.Proposals).WithOne().HasForeignKey(e => e.SeriesId).OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(e => e.HasTeams);
        builder.Ignore(e => e.LoserTeamId);
    }
}
