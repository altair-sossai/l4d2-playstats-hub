using L4D2PlayStats.Hub.Core.Modules.Seasons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Punishments.Configurations;

public class PunishmentConfiguration : IEntityTypeConfiguration<Punishment>
{
    public void Configure(EntityTypeBuilder<Punishment> builder)
    {
        builder.Property(e => e.Reason).HasMaxLength(AppConsts.PunishmentReasonMaxLength);
        builder.HasIndex(e => new { e.SeasonId, e.CommunityId });
        builder.HasOne<Season>().WithMany().HasForeignKey(e => e.SeasonId).OnDelete(DeleteBehavior.Cascade);
    }
}
