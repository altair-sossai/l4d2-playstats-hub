using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Notifications.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(e => e.Text).HasMaxLength(AppConsts.NotificationTextMaxLength).IsRequired();
        builder.Property(e => e.Values).HasMaxLength(AppConsts.NotificationValuesMaxLength);
        builder.Property(e => e.Url).HasMaxLength(AppConsts.NotificationUrlMaxLength);
        builder.HasIndex(e => new { e.Audience, e.CommunityId, e.CreatedAtUtc });
        builder.HasIndex(e => e.CreatedAtUtc);
    }
}
