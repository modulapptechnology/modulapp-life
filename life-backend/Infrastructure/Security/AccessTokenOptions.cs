namespace Infrastructure.Security;

public sealed record AccessTokenOptions(string SigningKey, string Issuer, string Audience, TimeSpan Lifetime)
{
    public const string SectionName = "Authentication:AccessToken";

    public const string SigningKeyKey = "SigningKey";

    public const string IssuerKey = "Issuer";

    public const string AudienceKey = "Audience";

    public const string LifetimeMinutesKey = "LifetimeMinutes";

    public const string DefaultIssuer = "life-api";

    public const string DefaultAudience = "life-app";

    public const int SigningKeyMinBytes = 32;

    public static readonly TimeSpan DefaultLifetime = TimeSpan.FromMinutes(15);
}
