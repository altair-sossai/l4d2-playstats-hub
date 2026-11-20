using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Configurations;

public class RemoteCommandLogConfiguration : IEntityTypeConfiguration<RemoteCommandLog>
{
    public void Configure(EntityTypeBuilder<RemoteCommandLog> builder)
    {
        builder.ToTable("RemoteCommandLogs");
        builder.Property(e => e.GameServerName).HasMaxLength(AppConsts.LivePlayerNameMaxLength);
        builder.Property(e => e.AdministratorName).HasMaxLength(AppConsts.LivePlayerNameMaxLength);
        builder.Property(e => e.Command).HasMaxLength(AppConsts.ServerCommandMaxLength + AppConsts.ServerCommandPrefix.Length);
        builder.HasIndex(e => e.CreatedAtUtc);
    }
}