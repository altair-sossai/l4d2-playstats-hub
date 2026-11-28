using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Configurations;

public class ChampionshipDraftPickConfiguration : IEntityTypeConfiguration<ChampionshipDraftPick>
{
    public void Configure(EntityTypeBuilder<ChampionshipDraftPick> builder)
    {
        builder.HasIndex(e => new { e.ChampionshipId, e.Number }).IsUnique();
        builder.HasOne<ChampionshipTeam>().WithMany().HasForeignKey(e => e.TeamId).OnDelete(DeleteBehavior.NoAction);
    }
}
