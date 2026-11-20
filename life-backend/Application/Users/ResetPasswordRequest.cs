namespace Life.Application.Users;

public sealed record ResetPasswordRequest(string Token, string NewPassword);
