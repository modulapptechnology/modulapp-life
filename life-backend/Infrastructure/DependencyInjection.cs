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
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
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
        var urlKey = $"{section}:{EmailVerificationOptions.VerificationUrlKey}";
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

        var url = configuration[urlKey];

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException(
                $"The setting '{urlKey}' must be an absolute URL, but was '{url}'.");
        }

        return new EmailVerificationOptions(lifetime, url);
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
