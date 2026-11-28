using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Bans.Configurations;

public class BanConfiguration : IEntityTypeConfiguration<Ban>
{
    public void Configure(EntityTypeBuilder<Ban> builder)
    {
        builder.ToTable("Bans");
        builder.Property(e => e.Name).HasMaxLength(AppConsts.LivePlayerNameMaxLength);
        builder.Property(e => e.Reason).HasMaxLength(AppConsts.BanReasonMaxLength);
        builder.Property(e => e.Command).HasMaxLength(AppConsts.BanCommandMaxLength);
        builder.Property(e => e.GameServerName).HasMaxLength(AppConsts.GameServerNameMaxLength);
        builder.Property(e => e.CreatedByName).HasMaxLength(AppConsts.LivePlayerNameMaxLength);
        builder.Property(e => e.UpdatedByName).HasMaxLength(AppConsts.LivePlayerNameMaxLength);
        builder.Property(e => e.LiftedByName).HasMaxLength(AppConsts.LivePlayerNameMaxLength);
        builder.Property(e => e.LiftReason).HasMaxLength(AppConsts.BanLiftReasonMaxLength);
        builder.HasIndex(e => new { e.CommunityId, e.LiftedAtUtc, e.EndsAtUtc });
        builder.HasIndex(e => e.CreatedAtUtc);
    }
}
