using L4D2PlayStats.Hub.Core.Modules.Matches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Configurations;

public class ChampionshipGameConfiguration : IEntityTypeConfiguration<ChampionshipGame>
{
    public void Configure(EntityTypeBuilder<ChampionshipGame> builder)
    {
        builder.Property(e => e.Notes).HasMaxLength(AppConsts.ChampionshipNotesMaxLength);
        builder.HasIndex(e => new { e.SeriesId, e.Number }).IsUnique();
        builder.HasIndex(e => e.MatchId);
        builder.HasOne<Match>().WithMany().HasForeignKey(e => e.MatchId).OnDelete(DeleteBehavior.SetNull);
        builder.Ignore(e => e.IsPlayed);
        builder.Ignore(e => e.TeamATotal);
        builder.Ignore(e => e.TeamBTotal);
        builder.Ignore(e => e.IsTie);
    }
}
