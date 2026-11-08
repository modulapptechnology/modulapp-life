namespace Application.Users;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt)
{
    public const string BearerTokenType = "Bearer";
}
