namespace L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;

public interface ICommunityRandomService
{
    int Next(int exclusiveMaximum);
}