using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Configurations;

public class ChampionshipGroupTeamConfiguration : IEntityTypeConfiguration<ChampionshipGroupTeam>
{
    public void Configure(EntityTypeBuilder<ChampionshipGroupTeam> builder)
    {
        builder.HasIndex(e => new { e.GroupId, e.TeamId }).IsUnique();
        builder.HasOne<ChampionshipTeam>().WithMany().HasForeignKey(e => e.TeamId).OnDelete(DeleteBehavior.NoAction);
    }
}
