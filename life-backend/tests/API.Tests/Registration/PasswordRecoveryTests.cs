using System.Net;
using System.Net.Http.Json;
using Life.API.Tests.Support;
using Life.Application.Common;
using Life.Application.Email;
using Life.Application.Users;
using Life.Domain.Users;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Life.API.Tests.Registration;

[Collection(ApiCollection.Name)]
public class PasswordRecoveryTests(ApiFactory api)
{
    private const string _forgotRoute = "/api/v1/Auth/forgot-password";

    private const string _resetRoute = "/api/v1/Auth/reset-password";

    private const string _loginRoute = "/api/v1/Auth/login";

    private const string _email = "person@life.test";

    private const string _oldPassword = "Abcdefgh12";

    private const string _newPassword = "Zyxwvuts98";

    private sealed record Environment(
        HttpClient Client,
        InMemoryUserRepository Users,
        InMemoryRefreshTokenRepository RefreshTokens,
        CapturingEmailSender Sender);

    private Environment CreateEnvironment(Action<IDictionary<string, string?>>? settings = null)
    {
        var users = new InMemoryUserRepository();
        var refreshTokens = new InMemoryRefreshTokenRepository(users);
        var sender = new CapturingEmailSender();
        var values = new Dictionary<string, string?>();
        settings?.Invoke(values);

        var factory = api.WithWebHostBuilder(builder =>
        {
            foreach (var (key, value) in values)
            {
                builder.UseSetting(key, value);
            }

            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IUserRepository>(users);
                services.AddSingleton<IRefreshTokenRepository>(refreshTokens);
                services.AddSingleton<IPasswordResetRepository>(new InMemoryPasswordResetRepository(users));
                services.AddSingleton<IEmailSender>(sender);
                services.AddSingleton<IUnitOfWork, PassThroughUnitOfWork>();
            });
        });

        var hasher = factory.Services.GetRequiredService<IPasswordHasher>();
        users.Seed(new User { Email = _email, PasswordHash = hasher.Hash(_oldPassword), Status = UserStatus.Active });
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });

        return new Environment(client, users, refreshTokens, sender);
    }

    private static string ReadToken(CapturingEmailSender sender)
    {
        var link = sender.Sent.Last().Token;

        return link[(link.IndexOf("token=", StringComparison.Ordinal) + "token=".Length)..];
    }

    [Fact]
    public async Task ForgotPassword_WithAKnownEmail_Returns202AndSendsTheLink()
    {
        var environment = CreateEnvironment();

        var response = await environment.Client.PostAsJsonAsync(_forgotRoute, new { email = _email });

        Assert.Equal(
            (HttpStatusCode.Accepted, 1),
            (response.StatusCode, environment.Sender.Sent.Count));
    }

    [Fact]
    public async Task ForgotPassword_WithAnUnknownEmail_ReturnsTheSameResponseAsAKnownOne()
    {
        var environment = CreateEnvironment();
        var known = await environment.Client.PostAsJsonAsync(_forgotRoute, new { email = _email });

        var unknown = await environment.Client.PostAsJsonAsync(_forgotRoute, new { email = "nobody@life.test" });

        Assert.Equal(
            (known.StatusCode, await known.Content.ReadAsStringAsync(), 1),
            (unknown.StatusCode, await unknown.Content.ReadAsStringAsync(), environment.Sender.Sent.Count));
    }

    [Fact]
    public async Task ForgotPassword_WithAnInvalidEmail_Returns400()
    {
        var environment = CreateEnvironment();

        var response = await environment.Client.PostAsJsonAsync(_forgotRoute, new { email = "not-an-email" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_BeyondTheConfiguredLimit_Returns429()
    {
        var environment = CreateEnvironment(settings =>
            settings["RateLimiting:PasswordRecovery:PermitLimit"] = "2");
        await environment.Client.PostAsJsonAsync(_forgotRoute, new { email = _email });
        await environment.Client.PostAsJsonAsync(_forgotRoute, new { email = _email });

        var response = await environment.Client.PostAsJsonAsync(_forgotRoute, new { email = _email });

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithTheTokenFromTheEmail_Returns204AndTheNewPasswordLogsIn()
    {
        var environment = CreateEnvironment();
        await environment.Client.PostAsJsonAsync(_forgotRoute, new { email = _email });
        var token = ReadToken(environment.Sender);

        var reset = await environment.Client.PostAsJsonAsync(_resetRoute, new { token, newPassword = _newPassword });

        var oldLogin = await environment.Client.PostAsJsonAsync(_loginRoute, new { email = _email, password = _oldPassword });
        var newLogin = await environment.Client.PostAsJsonAsync(_loginRoute, new { email = _email, password = _newPassword });
        Assert.Equal(
            (HttpStatusCode.NoContent, HttpStatusCode.Unauthorized, HttpStatusCode.OK),
            (reset.StatusCode, oldLogin.StatusCode, newLogin.StatusCode));
    }

    [Fact]
    public async Task ResetPassword_WhenTheTokenIsUsedTwice_RejectsTheSecondUse()
    {
        var environment = CreateEnvironment();
        await environment.Client.PostAsJsonAsync(_forgotRoute, new { email = _email });
        var token = ReadToken(environment.Sender);
        await environment.Client.PostAsJsonAsync(_resetRoute, new { token, newPassword = _newPassword });

        var second = await environment.Client.PostAsJsonAsync(_resetRoute, new { token, newPassword = "Qwertyui12" });

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithAnUnknownToken_Returns400()
    {
        var environment = CreateEnvironment();

        var response = await environment.Client.PostAsJsonAsync(
            _resetRoute,
            new { token = "unknown-token", newPassword = _newPassword });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithAWeakPassword_Returns400AndKeepsTheToken()
    {
        var environment = CreateEnvironment();
        await environment.Client.PostAsJsonAsync(_forgotRoute, new { email = _email });
        var token = ReadToken(environment.Sender);

        var weak = await environment.Client.PostAsJsonAsync(_resetRoute, new { token, newPassword = "short" });

        var retry = await environment.Client.PostAsJsonAsync(_resetRoute, new { token, newPassword = _newPassword });
        Assert.Equal((HttpStatusCode.BadRequest, HttpStatusCode.NoContent), (weak.StatusCode, retry.StatusCode));
    }

    [Fact]
    public async Task ResetPassword_WithTheTokenFromTheEmail_RevokesTheSessionsOpenedBefore()
    {
        var environment = CreateEnvironment();
        await environment.Client.PostAsJsonAsync(_loginRoute, new { email = _email, password = _oldPassword });
        await environment.Client.PostAsJsonAsync(_forgotRoute, new { email = _email });
        var token = ReadToken(environment.Sender);

        await environment.Client.PostAsJsonAsync(_resetRoute, new { token, newPassword = _newPassword });

        Assert.All(environment.RefreshTokens.Tokens, session => Assert.NotNull(session.RevokedAt));
    }
}
