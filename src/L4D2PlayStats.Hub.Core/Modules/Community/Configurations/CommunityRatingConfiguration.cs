using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Configurations;

public class CommunityRatingConfiguration : IEntityTypeConfiguration<CommunityRating>
{
    public void Configure(EntityTypeBuilder<CommunityRating> builder)
    {
        builder.HasKey(e => e.CommunityId);
        builder.Property(e => e.CommunityId).ValueGeneratedNever();
        builder.HasIndex(e => e.Rating);
    }
}