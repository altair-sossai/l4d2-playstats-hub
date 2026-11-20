namespace L4D2PlayStats.Hub.Web.Infrastructure.Steam;

public record SteamProfileLookup(IReadOnlyDictionary<long, SteamProfile> Profiles, IReadOnlySet<long> Answered);