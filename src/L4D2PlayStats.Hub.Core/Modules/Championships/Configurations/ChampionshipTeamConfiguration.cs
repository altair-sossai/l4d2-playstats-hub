using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Configurations;

public class ChampionshipTeamConfiguration : IEntityTypeConfiguration<ChampionshipTeam>
{
    public void Configure(EntityTypeBuilder<ChampionshipTeam> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(AppConsts.ChampionshipTeamNameMaxLength).IsRequired();
        builder.Property(e => e.Tag).HasMaxLength(AppConsts.ChampionshipTeamTagMaxLength);
        builder.HasIndex(e => e.ChampionshipId);
        builder.HasMany(e => e.Members).WithOne().HasForeignKey(e => e.TeamId).OnDelete(DeleteBehavior.Cascade);
    }
}
