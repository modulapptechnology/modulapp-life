using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace API.Tests.Support;

public static class TestAuthentication
{
    public const string Scheme = "Test";
    public const string Header = "X-Test-User";
}

public sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(TestAuthentication.Header, out var value) || !Guid.TryParse(value, out var userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], TestAuthentication.Scheme);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), TestAuthentication.Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
