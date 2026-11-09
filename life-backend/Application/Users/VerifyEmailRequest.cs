namespace Application.Users;

public sealed record VerifyEmailRequest(string Email, string Code);
