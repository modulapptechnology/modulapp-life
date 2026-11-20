using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Life.API.Authorization;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    public static Guid? GetSessionId(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var value = user.FindFirstValue(JwtRegisteredClaimNames.Sid) ?? user.FindFirstValue(ClaimTypes.Sid);

        return Guid.TryParse(value, out var sessionId) ? sessionId : null;
    }
}
