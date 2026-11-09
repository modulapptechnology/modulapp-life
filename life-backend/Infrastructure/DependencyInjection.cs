using System.Text;
using Application.Users;
using Infrastructure.Email;
using Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public const string EmailProviderKey = "Email:Provider";

    public const string SmtpProvider = "Smtp";

    public const string LogProvider = "Log";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton(BuildEmailVerificationOptions(configuration));
        services.AddSingleton(BuildSmtpOptions(configuration));
        services.AddSingleton(BuildLoginOptions(configuration));
        services.AddSingleton(BuildAccessTokenOptions(configuration));
        services.AddSingleton(BuildRefreshTokenOptions(configuration));
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        services.AddSingleton<IVerificationTokenService, VerificationTokenService>();
        AddVerificationEmailSender(services, configuration);

        return services;
    }

    private static void AddVerificationEmailSender(IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration[EmailProviderKey] ?? SmtpProvider;

        if (provider.Equals(SmtpProvider, StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IVerificationEmailSender, SmtpVerificationEmailSender>();
        }
        else if (provider.Equals(LogProvider, StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IVerificationEmailSender, LoggingVerificationEmailSender>();
        }
        else
        {
            throw new InvalidOperationException(
                $"The setting '{EmailProviderKey}' must be '{SmtpProvider}' or '{LogProvider}', "
                    + $"but was '{provider}'.");
        }
    }

    private static EmailVerificationOptions BuildEmailVerificationOptions(IConfiguration configuration)
    {
        const string section = EmailVerificationOptions.SectionName;
        var lifetimeKey = $"{section}:{EmailVerificationOptions.TokenLifetimeMinutesKey}";
        var lifetime = EmailVerificationOptions.DefaultTokenLifetime;
        var lifetimeValue = configuration[lifetimeKey];

        if (lifetimeValue is not null)
        {
            if (!int.TryParse(lifetimeValue, out var minutes) || minutes <= 0)
            {
                throw new InvalidOperationException(
                    $"The setting '{lifetimeKey}' must be a positive number of minutes, "
                        + $"but was '{lifetimeValue}'.");
            }

            lifetime = TimeSpan.FromMinutes(minutes);
        }

        return new EmailVerificationOptions(lifetime);
    }

    private static LoginOptions BuildLoginOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection(LoginOptions.SectionName);
        var attempts = ReadPositiveNumber(
            section,
            LoginOptions.MaxFailedAttemptsKey,
            LoginOptions.DefaultMaxFailedAttempts);
        var minutes = ReadPositiveNumber(
            section,
            LoginOptions.LockoutMinutesKey,
            (int)LoginOptions.DefaultLockoutDuration.TotalMinutes);

        return new LoginOptions(attempts, TimeSpan.FromMinutes(minutes));
    }

    private static AccessTokenOptions BuildAccessTokenOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection(AccessTokenOptions.SectionName);
        var signingKey = section[AccessTokenOptions.SigningKeyKey] ?? string.Empty;

        if (Encoding.UTF8.GetByteCount(signingKey) < AccessTokenOptions.SigningKeyMinBytes)
        {
            throw new InvalidOperationException(
                $"The setting '{AccessTokenOptions.SectionName}:{AccessTokenOptions.SigningKeyKey}' must be "
                    + $"at least {AccessTokenOptions.SigningKeyMinBytes} bytes long. "
                    + "Set it with user-secrets or an environment variable.");
        }

        var minutes = ReadPositiveNumber(
            section,
            AccessTokenOptions.LifetimeMinutesKey,
            (int)AccessTokenOptions.DefaultLifetime.TotalMinutes);

        return new AccessTokenOptions(
            signingKey,
            section[AccessTokenOptions.IssuerKey] ?? AccessTokenOptions.DefaultIssuer,
            section[AccessTokenOptions.AudienceKey] ?? AccessTokenOptions.DefaultAudience,
            TimeSpan.FromMinutes(minutes));
    }

    private static RefreshTokenOptions BuildRefreshTokenOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection(RefreshTokenOptions.SectionName);
        var days = ReadPositiveNumber(
            section,
            RefreshTokenOptions.LifetimeDaysKey,
            RefreshTokenOptions.DefaultLifetimeDays);

        return new RefreshTokenOptions(TimeSpan.FromDays(days));
    }

    private static int ReadPositiveNumber(IConfigurationSection section, string key, int defaultValue)
    {
        var value = section[key];

        if (value is null)
        {
            return defaultValue;
        }

        if (!int.TryParse(value, out var number) || number <= 0)
        {
            throw new InvalidOperationException(
                $"The setting '{section.Path}:{key}' must be a positive number, but was '{value}'.");
        }

        return number;
    }

    private static SmtpOptions BuildSmtpOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection(SmtpOptions.SectionName);
        var portValue = section["Port"];
        var port = SmtpOptions.DefaultPort;

        if (portValue is not null && (!int.TryParse(portValue, out port) || port <= 0))
        {
            throw new InvalidOperationException(
                $"The setting '{SmtpOptions.SectionName}:Port' must be a positive number, "
                    + $"but was '{portValue}'.");
        }

        var enableSsl = !bool.TryParse(section["EnableSsl"], out var parsedSsl) || parsedSsl;

        return new SmtpOptions(
            section["Host"] ?? string.Empty,
            port,
            enableSsl,
            section["UserName"] ?? string.Empty,
            section["Password"] ?? string.Empty,
            section["FromAddress"] ?? string.Empty);
    }
}
