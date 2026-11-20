using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace L4D2PlayStats.Hub.Tests.Support;

public class TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var user = Request.Headers[HubWebFactory.UserHeader].ToString();

        if (string.IsNullOrEmpty(user))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = Request.Headers.ContainsKey(HubWebFactory.NamelessHeader)
            ? new[] { new Claim(ClaimTypes.NameIdentifier, user) }
            : [new Claim(ClaimTypes.NameIdentifier, user), new Claim(ClaimTypes.Name, $"Player {user}")];
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}