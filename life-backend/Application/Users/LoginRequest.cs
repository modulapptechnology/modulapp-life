namespace Application.Users;

public sealed record LoginRequest(string Email, string Password, string? DeviceId = null);
