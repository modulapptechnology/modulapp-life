namespace Application.Users;

public sealed record EmailVerificationOptions(TimeSpan TokenLifetime, string VerificationUrl)
{
    public const string SectionName = "EmailVerification";

    public const string TokenLifetimeMinutesKey = "TokenLifetimeMinutes";

    public const string VerificationUrlKey = "VerificationUrl";

    public static readonly TimeSpan DefaultTokenLifetime = TimeSpan.FromHours(24);
}
