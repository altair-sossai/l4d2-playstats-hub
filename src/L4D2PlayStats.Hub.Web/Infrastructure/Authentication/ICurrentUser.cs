namespace L4D2PlayStats.Hub.Web.Infrastructure.Authentication;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    long? CommunityId { get; }
    string? Name { get; }
    Task<bool> IsAdministratorAsync();
    bool ItIsMe(long? communityId);
    bool ItIsMe(string? communityId);
}
