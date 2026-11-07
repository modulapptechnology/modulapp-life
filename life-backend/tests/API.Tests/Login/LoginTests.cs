using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.Tests.Support;
using Application.Common;
using Application.Users;
using Domain.Users;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests.Login;

[Collection(ApiCollection.Name)]
public class LoginTests(ApiFactory api)
{
    private const string _loginRoute = "/api/v1/Auth/login";

    private const string _email = "person@life.test";

    private const string _password = "Abcdefgh12";

    private static Task<HttpResponseMessage> LoginAsync(HttpClient client, string email, string password)
    {
        return client.PostAsJsonAsync(_loginRoute, new { email, password });
    }

    private (HttpClient Client, InMemoryUserRepository Users) CreateClient(
        UserStatus status = UserStatus.Active,
        Action<IDictionary<string, string?>>? settings = null)
    {
        var users = new InMemoryUserRepository();
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
                services.AddSingleton<IUnitOfWork, PassThroughUnitOfWork>();
            });
        });

        var hasher = factory.Services.GetRequiredService<IPasswordHasher>();
        users.Seed(new User { Email = _email, PasswordHash = hasher.Hash(_password), Status = status });

        return (factory.CreateClient(), users);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithTheSession()
    {
        var (client, _) = CreateClient();

        var response = await LoginAsync(client, _email, _password);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(
            (HttpStatusCode.OK, "Bearer", 3),
            (
                response.StatusCode,
                body.RootElement.GetProperty("tokenType").GetString(),
                body.RootElement.GetProperty("accessToken").GetString()!.Split('.').Length));
    }

    [Fact]
    public async Task Login_WithEmailInDifferentCase_Returns200()
    {
        var (client, _) = CreateClient();

        var response = await LoginAsync(client, "PERSON@LIFE.TEST", _password);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var (client, _) = CreateClient();

        var response = await LoginAsync(client, _email, "Wrongpass12");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmailOrWrongPassword_ReturnsTheSameBody()
    {
        var (client, _) = CreateClient();

        var unknown = await LoginAsync(client, "ghost@life.test", _password);
        var wrong = await LoginAsync(client, _email, "Wrongpass12");

        var bodies = (await unknown.Content.ReadAsStringAsync(), await wrong.Content.ReadAsStringAsync());
        Assert.Equal(
            (HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized, true),
            (unknown.StatusCode, wrong.StatusCode, ProblemTitle(bodies.Item1) == ProblemTitle(bodies.Item2)));
    }

    [Fact]
    public async Task Login_WithValidCredentials_DoesNotExposeThePasswordOrItsHash()
    {
        var (client, users) = CreateClient();

        var response = await LoginAsync(client, _email, _password);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(
            (false, false),
            (
                content.Contains(_password, StringComparison.Ordinal),
                content.Contains(users.Users.Single().PasswordHash!, StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Login_AfterTheFailedAttemptLimit_RejectsEvenTheRightPassword()
    {
        var (client, users) = CreateClient(settings: settings =>
            settings["Authentication:Login:MaxFailedAttempts"] = "2");
        await LoginAsync(client, _email, "Wrongpass12");
        await LoginAsync(client, _email, "Wrongpass12");

        var response = await LoginAsync(client, _email, _password);

        Assert.Equal(
            (HttpStatusCode.Unauthorized, true),
            (response.StatusCode, users.Users.Single().LockedUntil.HasValue));
    }

    [Fact]
    public async Task Login_WithPendingUser_Returns401()
    {
        var (client, _) = CreateClient(UserStatus.Pending);

        var response = await LoginAsync(client, _email, _password);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithSuccessAndFailure_AuditsBothAttempts()
    {
        var (client, users) = CreateClient();
        await LoginAsync(client, _email, "Wrongpass12");
        await LoginAsync(client, _email, _password);

        var outcomes = users.LoginAttempts.Select(attempt => attempt.Outcome).ToList();

        Assert.Equal([LoginOutcome.InvalidCredentials, LoginOutcome.Succeeded], outcomes);
    }

    [Fact]
    public async Task Login_WithInvalidEmail_Returns400WithTheEmailField()
    {
        var (client, users) = CreateClient();

        var response = await LoginAsync(client, "not-an-email", _password);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(
            (HttpStatusCode.BadRequest, true, 0),
            (
                response.StatusCode,
                body.RootElement.GetProperty("errors").TryGetProperty("Email", out _),
                users.LoginAttempts.Count));
    }

    [Fact]
    public async Task Login_WhenThePermitLimitIsExceeded_Returns429()
    {
        var (client, _) = CreateClient(settings: settings => settings["RateLimiting:Login:PermitLimit"] = "2");

        var statuses = new List<HttpStatusCode>();

        for (var attempt = 0; attempt < 3; attempt++)
        {
            statuses.Add((await LoginAsync(client, "not-an-email", _password)).StatusCode);
        }

        Assert.Equal(
            [HttpStatusCode.BadRequest, HttpStatusCode.BadRequest, HttpStatusCode.TooManyRequests],
            statuses);
    }

    private static string? ProblemTitle(string json)
    {
        using var document = JsonDocument.Parse(json);

        return document.RootElement.GetProperty("title").GetString();
    }
}
