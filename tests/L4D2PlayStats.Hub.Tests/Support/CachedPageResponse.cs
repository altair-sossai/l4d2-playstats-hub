using System.Net;

namespace L4D2PlayStats.Hub.Tests.Support;

public record CachedPageResponse(HttpStatusCode Status, string Html, bool Cached, bool SetsCookies);
