using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Notifications.Configurations;

public class NotificationReadConfiguration : IEntityTypeConfiguration<NotificationRead>
{
    public void Configure(EntityTypeBuilder<NotificationRead> builder)
    {
        builder.HasKey(e => new { e.NotificationId, e.CommunityId });
        builder.HasIndex(e => e.CommunityId);
        builder.HasOne<Notification>().WithMany().HasForeignKey(e => e.NotificationId).OnDelete(DeleteBehavior.Cascade);
    }
}
