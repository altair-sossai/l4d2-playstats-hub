using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Servers.Configurations;

public class GameServerConfiguration : IEntityTypeConfiguration<GameServer>
{
    public void Configure(EntityTypeBuilder<GameServer> builder)
    {
        builder.Property(e => e.PublicId).HasMaxLength(AppConsts.GameServerPublicIdColumnMaxLength);
        builder.Property(e => e.Name).HasMaxLength(AppConsts.GameServerNameMaxLength);
        builder.Property(e => e.Address).HasMaxLength(AppConsts.GameServerAddressMaxLength);
        builder.Property(e => e.AlternativeAddress).HasMaxLength(AppConsts.GameServerAddressMaxLength);
        builder.Property(e => e.KeyHash).HasMaxLength(AppConsts.GameServerKeyHashMaxLength);
        builder.HasIndex(e => e.PublicId).IsUnique();
    }
}