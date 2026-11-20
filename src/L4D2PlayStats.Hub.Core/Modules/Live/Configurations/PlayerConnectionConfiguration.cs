using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Configurations;

public class PlayerConnectionConfiguration : IEntityTypeConfiguration<PlayerConnection>
{
    public void Configure(EntityTypeBuilder<PlayerConnection> builder)
    {
        builder.ToTable("PlayerConnections");
        builder.Property(e => e.IpAddress).HasMaxLength(AppConsts.IpAddressMaxLength);
        builder.Property(e => e.Name).HasMaxLength(AppConsts.LivePlayerNameMaxLength);
        builder.HasIndex(e => new { e.IpAddress, e.CommunityId }).IsUnique();
        builder.HasIndex(e => e.CommunityId);
        builder.HasIndex(e => e.LastSeenAtUtc);
    }
}