using Life.Domain.Users;

namespace Life.Application.Users;

public sealed class SessionIssuer(
    IAccessTokenIssuer accessTokenIssuer,
    IVerificationTokenService tokenService,
    IRefreshTokenRepository refreshTokens,
    RefreshTokenOptions options)
{
    public LoginResponse Issue(User user, Guid familyId, string deviceId, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrEmpty(deviceId);

        var accessToken = accessTokenIssuer.Issue(user, now);
        var refreshToken = tokenService.Create();
        var refreshExpiresAt = now.Add(options.Lifetime);

        refreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            FamilyId = familyId,
            DeviceId = deviceId,
            TokenHash = refreshToken.Hash,
            ExpiresAt = refreshExpiresAt,
        });

        return new LoginResponse(
            accessToken.Value,
            LoginResponse.BearerTokenType,
            accessToken.ExpiresAt,
            refreshToken.Value,
            refreshExpiresAt);
    }
}
