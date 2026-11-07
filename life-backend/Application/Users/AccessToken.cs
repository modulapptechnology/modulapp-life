namespace Application.Users;

public sealed record AccessToken(string Value, DateTime ExpiresAt);
