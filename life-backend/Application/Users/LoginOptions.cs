namespace Life.Application.Users;

public sealed record LoginOptions(int MaxFailedAttempts, TimeSpan LockoutDuration)
{
    public const string SectionName = "Authentication:Login";

    public const string MaxFailedAttemptsKey = "MaxFailedAttempts";

    public const string LockoutMinutesKey = "LockoutMinutes";

    public const int DefaultMaxFailedAttempts = 5;

    public static readonly TimeSpan DefaultLockoutDuration = TimeSpan.FromMinutes(15);
}
