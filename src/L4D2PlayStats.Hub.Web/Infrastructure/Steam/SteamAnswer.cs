namespace L4D2PlayStats.Hub.Web.Infrastructure.Steam;

public record SteamAnswer<T>(bool Ok, T Value);
