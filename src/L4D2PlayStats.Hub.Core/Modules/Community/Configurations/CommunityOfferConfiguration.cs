using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Configurations;

public class CommunityOfferConfiguration : IEntityTypeConfiguration<CommunityOffer>
{
    public void Configure(EntityTypeBuilder<CommunityOffer> builder)
    {
        builder.HasKey(e => e.VoterId);
        builder.Property(e => e.VoterId).ValueGeneratedNever();
    }
}
