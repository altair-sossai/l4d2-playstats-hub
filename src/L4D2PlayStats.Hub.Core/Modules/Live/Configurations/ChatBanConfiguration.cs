using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Configurations;

public class ChatBanConfiguration : IEntityTypeConfiguration<ChatBan>
{
    public void Configure(EntityTypeBuilder<ChatBan> builder)
    {
        builder.ToTable("ChatBans");
        builder.HasKey(e => e.CommunityId);
        builder.Property(e => e.CommunityId).ValueGeneratedNever();
        builder.Property(e => e.Name).HasMaxLength(AppConsts.LivePlayerNameMaxLength);
        builder.Property(e => e.Reason).HasMaxLength(AppConsts.ChatBanReasonMaxLength);
        builder.Property(e => e.CreatedByName).HasMaxLength(AppConsts.LivePlayerNameMaxLength);
    }
}
