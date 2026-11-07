namespace Application.Users;

public sealed record LoginResponse(string AccessToken, string TokenType, DateTime ExpiresAt)
{
    public const string BearerTokenType = "Bearer";
}
