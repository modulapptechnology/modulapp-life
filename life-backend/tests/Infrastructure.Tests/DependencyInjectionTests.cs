using Life.Application.Users;
using Life.Application.Workspaces;
using Life.Infrastructure.Email;
using Life.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Life.Infrastructure.Tests;

public class DependencyInjectionTests
{
    private const string _signingKey = "test-only-signing-key-not-a-secret-0123456789";

    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] values)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Authentication:AccessToken:SigningKey"] = _signingKey,
        };

        foreach (var (key, value) in values)
        {
            settings[key] = value;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }

    private static ServiceProvider BuildProvider(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);

        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddInfrastructure_WithValidCollection_ReturnsSameCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddInfrastructure(BuildConfiguration());

        Assert.Same(services, result);
    }

    [Fact]
    public void AddInfrastructure_WithoutSettings_RegistersTheHasherTheTokenServiceAndTheSmtpSender()
    {
        using var provider = BuildProvider(BuildConfiguration());

        var resolved = (
            provider.GetRequiredService<IPasswordHasher>(),
            provider.GetRequiredService<IVerificationTokenService>(),
            provider.GetRequiredService<IVerificationEmailSender>());

        Assert.Equal(
            (
                typeof(Pbkdf2PasswordHasher),
                typeof(VerificationTokenService),
                typeof(SmtpVerificationEmailSender)),
            (resolved.Item1.GetType(), resolved.Item2.GetType(), resolved.Item3.GetType()));
    }

    [Fact]
    public void AddInfrastructure_WithLogProvider_RegistersTheLoggingSender()
    {
        using var provider = BuildProvider(BuildConfiguration(("Email:Provider", "log")));

        var sender = provider.GetRequiredService<IVerificationEmailSender>();

        Assert.IsType<LoggingVerificationEmailSender>(sender);
    }

    [Fact]
    public void AddInfrastructure_WithUnknownProvider_ThrowsInvalidOperationException()
    {
        var configuration = BuildConfiguration(("Email:Provider", "pigeon"));

        var action = () => new ServiceCollection().AddInfrastructure(configuration);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void AddInfrastructure_WithoutLifetime_UsesFifteenMinutes()
    {
        using var provider = BuildProvider(BuildConfiguration());

        var options = provider.GetRequiredService<EmailVerificationOptions>();

        Assert.Equal(TimeSpan.FromMinutes(15), options.TokenLifetime);
    }

    [Fact]
    public void AddInfrastructure_WithConfiguredLifetime_UsesThoseMinutes()
    {
        var configuration = BuildConfiguration(("EmailVerification:TokenLifetimeMinutes", "30"));
        using var provider = BuildProvider(configuration);

        var options = provider.GetRequiredService<EmailVerificationOptions>();

        Assert.Equal(TimeSpan.FromMinutes(30), options.TokenLifetime);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("soon")]
    public void AddInfrastructure_WithInvalidLifetime_ThrowsInvalidOperationException(string minutes)
    {
        var configuration = BuildConfiguration(("EmailVerification:TokenLifetimeMinutes", minutes));

        var action = () => new ServiceCollection().AddInfrastructure(configuration);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void AddInfrastructure_WithoutLoginSettings_UsesFiveAttemptsAndFifteenMinutes()
    {
        using var provider = BuildProvider(BuildConfiguration());

        var options = provider.GetRequiredService<LoginOptions>();

        Assert.Equal((5, TimeSpan.FromMinutes(15)), (options.MaxFailedAttempts, options.LockoutDuration));
    }

    [Fact]
    public void AddInfrastructure_WithConfiguredLoginSettings_UsesThoseValues()
    {
        var configuration = BuildConfiguration(
            ("Authentication:Login:MaxFailedAttempts", "3"),
            ("Authentication:Login:LockoutMinutes", "30"));
        using var provider = BuildProvider(configuration);

        var options = provider.GetRequiredService<LoginOptions>();

        Assert.Equal((3, TimeSpan.FromMinutes(30)), (options.MaxFailedAttempts, options.LockoutDuration));
    }

    [Theory]
    [InlineData("Authentication:Login:MaxFailedAttempts", "0")]
    [InlineData("Authentication:Login:LockoutMinutes", "-1")]
    [InlineData("Authentication:AccessToken:LifetimeMinutes", "soon")]
    [InlineData("Authentication:RefreshToken:LifetimeDays", "0")]
    public void AddInfrastructure_WithInvalidAuthenticationNumber_ThrowsInvalidOperationException(
        string key,
        string value)
    {
        var configuration = BuildConfiguration((key, value));

        var action = () => new ServiceCollection().AddInfrastructure(configuration);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void AddInfrastructure_WithoutRefreshTokenSettings_UsesSixtyDays()
    {
        using var provider = BuildProvider(BuildConfiguration());

        var options = provider.GetRequiredService<RefreshTokenOptions>();

        Assert.Equal(TimeSpan.FromDays(60), options.Lifetime);
    }

    [Fact]
    public void AddInfrastructure_WithRefreshTokenLifetime_UsesTheConfiguredDays()
    {
        using var provider = BuildProvider(BuildConfiguration(("Authentication:RefreshToken:LifetimeDays", "7")));

        var options = provider.GetRequiredService<RefreshTokenOptions>();

        Assert.Equal(TimeSpan.FromDays(7), options.Lifetime);
    }

    [Fact]
    public void AddInfrastructure_WithoutWorkspaceLimitSettings_UsesFiveOwnedWorkspaces()
    {
        using var provider = BuildProvider(BuildConfiguration());

        var options = provider.GetRequiredService<WorkspaceLimitOptions>();

        Assert.Equal(5, options.MaxOwnedWorkspaces);
    }

    [Fact]
    public void AddInfrastructure_WithConfiguredWorkspaceLimit_UsesThatValue()
    {
        using var provider = BuildProvider(BuildConfiguration(("Workspaces:Limits:MaxOwnedWorkspaces", "2")));

        var options = provider.GetRequiredService<WorkspaceLimitOptions>();

        Assert.Equal(2, options.MaxOwnedWorkspaces);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("many")]
    public void AddInfrastructure_WithInvalidWorkspaceLimit_ThrowsInvalidOperationException(string value)
    {
        var configuration = BuildConfiguration(("Workspaces:Limits:MaxOwnedWorkspaces", value));

        var action = () => new ServiceCollection().AddInfrastructure(configuration);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Theory]
    [InlineData("")]
    [InlineData("too-short")]
    public void AddInfrastructure_WithoutAStrongSigningKey_ThrowsInvalidOperationException(string key)
    {
        var configuration = BuildConfiguration(("Authentication:AccessToken:SigningKey", key));

        var action = () => new ServiceCollection().AddInfrastructure(configuration);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void AddInfrastructure_WithoutTokenSettings_UsesTheDefaultIssuerAudienceAndLifetime()
    {
        using var provider = BuildProvider(BuildConfiguration());

        var options = provider.GetRequiredService<AccessTokenOptions>();

        Assert.Equal(
            (AccessTokenOptions.DefaultIssuer, AccessTokenOptions.DefaultAudience, TimeSpan.FromMinutes(15)),
            (options.Issuer, options.Audience, options.Lifetime));
    }

    [Fact]
    public void AddInfrastructure_WithNullConfiguration_ThrowsArgumentNullException()
    {
        var action = () => new ServiceCollection().AddInfrastructure(null!);

        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public async Task LoggingSender_WhenSending_CompletesWithoutThrowing()
    {
        var logger = NullLogger<LoggingVerificationEmailSender>.Instance;
        var sender = new LoggingVerificationEmailSender(logger);

        var task = sender.SendAsync("person@life.test", "token");

        await task;
        Assert.True(task.IsCompletedSuccessfully);
    }
}
