namespace Life.Application.Users;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
