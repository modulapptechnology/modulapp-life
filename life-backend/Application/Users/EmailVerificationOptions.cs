namespace Life.Application.Users;

public sealed record EmailVerificationOptions(TimeSpan TokenLifetime)
{
    public const string SectionName = "EmailVerification";

    public const string TokenLifetimeMinutesKey = "TokenLifetimeMinutes";

    public static readonly TimeSpan DefaultTokenLifetime = TimeSpan.FromMinutes(15);
}
