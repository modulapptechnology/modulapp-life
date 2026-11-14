namespace Life.Application.Users;

public sealed record RefreshTokenOptions(TimeSpan Lifetime)
{
    public const string SectionName = "Authentication:RefreshToken";

    public const string LifetimeDaysKey = "LifetimeDays";

    public const int DefaultLifetimeDays = 60;

    public static readonly TimeSpan DefaultLifetime = TimeSpan.FromDays(DefaultLifetimeDays);
}
