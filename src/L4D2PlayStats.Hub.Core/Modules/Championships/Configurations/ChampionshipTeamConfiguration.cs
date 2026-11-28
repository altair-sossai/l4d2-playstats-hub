using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Configurations;

public class ChampionshipTeamConfiguration : IEntityTypeConfiguration<ChampionshipTeam>
{
    public void Configure(EntityTypeBuilder<ChampionshipTeam> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(AppConsts.ChampionshipTeamNameMaxLength).IsRequired();
        builder.Property(e => e.Tag).HasMaxLength(AppConsts.ChampionshipTeamTagMaxLength);
        builder.Property(e => e.Flag).HasMaxLength(AppConsts.ChampionshipFileNameMaxLength);
        builder.Property(e => e.PendingName).HasMaxLength(AppConsts.ChampionshipTeamNameMaxLength);
        builder.Property(e => e.PendingTag).HasMaxLength(AppConsts.ChampionshipTeamTagMaxLength);
        builder.Property(e => e.PendingFlag).HasMaxLength(AppConsts.ChampionshipFileNameMaxLength);
        builder.Property(e => e.RejectionReason).HasMaxLength(AppConsts.ChampionshipReasonMaxLength);
        builder.HasIndex(e => e.ChampionshipId);
        builder.HasMany(e => e.Members).WithOne().HasForeignKey(e => e.TeamId).OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(e => e.HasPendingIdentity);
    }
}
