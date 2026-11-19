namespace L4D2PlayStats.Hub.Core.Modules.Community.Models;

public record CommunityVoteParticipant(long CommunityId, string Name, DateTime? LastVotedAtUtc, DateTime? LastComparedAtUtc)
{
    public bool Voted => LastVotedAtUtc != null;
    public bool Compared => LastComparedAtUtc != null;
}
