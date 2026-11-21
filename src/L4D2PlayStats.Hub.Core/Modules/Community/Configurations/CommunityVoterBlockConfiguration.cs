using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Configurations;

public class CommunityVoterBlockConfiguration : IEntityTypeConfiguration<CommunityVoterBlock>
{
    public void Configure(EntityTypeBuilder<CommunityVoterBlock> builder)
    {
        builder.HasKey(e => e.CommunityId);
        builder.Property(e => e.CommunityId).ValueGeneratedNever();
    }
}
