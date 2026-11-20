namespace Life.Application.Users;

public sealed record PasswordRecoveryOptions(TimeSpan TokenLifetime, string ResetUrl)
{
    public const string SectionName = "PasswordRecovery";

    public const string TokenLifetimeMinutesKey = "TokenLifetimeMinutes";

    public const string ResetUrlKey = "ResetUrl";

    public const string DefaultResetUrl = "http://localhost:8081/reset-password";

    public const string TokenQueryName = "token";

    public static readonly TimeSpan DefaultTokenLifetime = TimeSpan.FromMinutes(15);
}
