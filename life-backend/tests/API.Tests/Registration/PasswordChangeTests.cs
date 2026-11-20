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
public class PasswordChangeTests(ApiFactory api)
{
    private const string _changeRoute = "/api/v1/Auth/change-password";

    private const string _loginRoute = "/api/v1/Auth/login";

    private const string _email = "person@life.test";

    private const string _oldPassword = "Abcdefgh12";

    private const string _newPassword = "Zyxwvuts98";

    private sealed record Environment(
        HttpClient Client,
        Guid UserId,
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
                services.AddSingleton<IEmailSender>(sender);
                services.AddSingleton<IUnitOfWork, PassThroughUnitOfWork>();
            });
        });

        var hasher = factory.Services.GetRequiredService<IPasswordHasher>();
        var user = new User { Email = _email, PasswordHash = hasher.Hash(_oldPassword), Status = UserStatus.Active };
        users.Seed(user);
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });

        return new Environment(client, user.Id, refreshTokens, sender);
    }

    private static Task<HttpResponseMessage> LoginAsync(HttpClient client, string password = _oldPassword)
    {
        return client.PostAsJsonAsync(_loginRoute, new { email = _email, password });
    }

    private static Task<HttpResponseMessage> ChangeAsync(
        Environment environment,
        string currentPassword,
        string newPassword,
        Guid? userId = null,
        Guid? sessionId = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _changeRoute)
        {
            Content = JsonContent.Create(new { currentPassword, newPassword }),
        };

        if (userId is not null)
        {
            request.Headers.Add(TestAuthentication.Header, userId.Value.ToString());
        }

        if (sessionId is not null)
        {
            request.Headers.Add(TestAuthentication.SessionHeader, sessionId.Value.ToString());
        }

        return environment.Client.SendAsync(request);
    }

    [Fact]
    public async Task ChangePassword_WithTheCurrentPassword_Returns204AndTheNewPasswordLogsIn()
    {
        var environment = CreateEnvironment();

        var change = await ChangeAsync(environment, _oldPassword, _newPassword, environment.UserId);

        var oldLogin = await LoginAsync(environment.Client, _oldPassword);
        var newLogin = await LoginAsync(environment.Client, _newPassword);
        Assert.Equal(
            (HttpStatusCode.NoContent, HttpStatusCode.Unauthorized, HttpStatusCode.OK),
            (change.StatusCode, oldLogin.StatusCode, newLogin.StatusCode));
    }

    [Fact]
    public async Task ChangePassword_WithTheCurrentPassword_SendsTheNoticeToTheUser()
    {
        var environment = CreateEnvironment();

        await ChangeAsync(environment, _oldPassword, _newPassword, environment.UserId);

        Assert.Equal([_email], environment.Sender.Sent.Select(sent => sent.Email));
    }

    [Fact]
    public async Task ChangePassword_WithTheCurrentPassword_RevokesTheOtherSessionsAndKeepsTheCurrentOne()
    {
        var environment = CreateEnvironment();
        await LoginAsync(environment.Client);
        await LoginAsync(environment.Client);
        var sessions = environment.RefreshTokens.Tokens.ToList();

        await ChangeAsync(environment, _oldPassword, _newPassword, environment.UserId, sessions[1].FamilyId);

        Assert.Equal(
            (true, false),
            (sessions[0].RevokedAt is not null, sessions[1].RevokedAt is not null));
    }

    [Fact]
    public async Task ChangePassword_WithAWrongCurrentPassword_Returns400AndKeepsThePassword()
    {
        var environment = CreateEnvironment();

        var change = await ChangeAsync(environment, "Wrongpass12", _newPassword, environment.UserId);

        var login = await LoginAsync(environment.Client, _oldPassword);
        Assert.Equal(
            (HttpStatusCode.BadRequest, HttpStatusCode.OK, 0),
            (change.StatusCode, login.StatusCode, environment.Sender.Sent.Count));
    }

    [Fact]
    public async Task ChangePassword_WithAWrongCurrentPassword_KeepsEverySessionOpen()
    {
        var environment = CreateEnvironment();
        await LoginAsync(environment.Client);

        await ChangeAsync(environment, "Wrongpass12", _newPassword, environment.UserId);

        Assert.All(environment.RefreshTokens.Tokens, session => Assert.Null(session.RevokedAt));
    }

    [Fact]
    public async Task ChangePassword_WithANewPasswordEqualToTheCurrent_Returns400()
    {
        var environment = CreateEnvironment();

        var change = await ChangeAsync(environment, _oldPassword, _oldPassword, environment.UserId);

        Assert.Equal(HttpStatusCode.BadRequest, change.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WithAWeakNewPassword_Returns400()
    {
        var environment = CreateEnvironment();

        var change = await ChangeAsync(environment, _oldPassword, "short", environment.UserId);

        Assert.Equal(HttpStatusCode.BadRequest, change.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WithoutAuthentication_Returns401()
    {
        var environment = CreateEnvironment();

        var change = await ChangeAsync(environment, _oldPassword, _newPassword);

        Assert.Equal(HttpStatusCode.Unauthorized, change.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_ForAnUnknownUser_Returns401()
    {
        var environment = CreateEnvironment();

        var change = await ChangeAsync(environment, _oldPassword, _newPassword, Guid.NewGuid());

        Assert.Equal(HttpStatusCode.Unauthorized, change.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_BeyondTheConfiguredLimit_Returns429()
    {
        var environment = CreateEnvironment(settings => settings["RateLimiting:PasswordChange:PermitLimit"] = "2");
        await ChangeAsync(environment, "Wrongpass12", _newPassword, environment.UserId);
        await ChangeAsync(environment, "Wrongpass12", _newPassword, environment.UserId);

        var change = await ChangeAsync(environment, "Wrongpass12", _newPassword, environment.UserId);

        Assert.Equal(HttpStatusCode.TooManyRequests, change.StatusCode);
    }
}
