using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Services;

public class CommunityRandomService : ICommunityRandomService
{
    public int Next(int exclusiveMaximum)
    {
        return Random.Shared.Next(exclusiveMaximum);
    }
}