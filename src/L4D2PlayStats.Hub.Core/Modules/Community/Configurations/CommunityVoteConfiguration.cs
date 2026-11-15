using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Configurations;

public class CommunityVoteConfiguration : IEntityTypeConfiguration<CommunityVote>
{
    public void Configure(EntityTypeBuilder<CommunityVote> builder)
    {
        builder.HasIndex(e => new { e.VoterId, e.SiteDay });
    }
}
