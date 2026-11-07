using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.Tests.Support;
using Application.Common;
using Application.Users;
using Domain.Users;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests.Registration;

[Collection(ApiCollection.Name)]
public class RegistrationTests(ApiFactory api)
{
    private const string _registerRoute = "/api/v1/Auth/register";

    private const string _verifyRoute = "/api/v1/Auth/verify-email";

    private const string _validPassword = "Abcdefgh12";

    private static Task<HttpResponseMessage> RegisterAsync(HttpClient client)
    {
        var body = new { email = "person@life.test", password = _validPassword };

        return client.PostAsJsonAsync(_registerRoute, body);
    }

    private static string VerifyUrl(string token) => $"{_verifyRoute}?token={Uri.EscapeDataString(token)}";

    private static async Task<IReadOnlyDictionary<string, string[]>> ReadErrorsAsync(
        HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = document.RootElement.GetProperty("errors");

        return errors.EnumerateObject().ToDictionary(
            property => property.Name,
            property => property.Value.EnumerateArray()
                .Select(item => item.GetString() ?? string.Empty)
                .ToArray());
    }

    private (HttpClient Client, InMemoryUserRepository Users, CapturingEmailSender Sender) CreateClient(
        Action<IDictionary<string, string?>>? settings = null)
    {
        var users = new InMemoryUserRepository();
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
                services.AddSingleton<IVerificationEmailSender>(sender);
                services.AddSingleton<IUnitOfWork, PassThroughUnitOfWork>();
            });
        });

        return (factory.CreateClient(), users, sender);
    }

    [Fact]
    public async Task Register_WithValidData_Returns201WithoutExposingThePassword()
    {
        var (client, users, _) = CreateClient();

        var response = await client.PostAsJsonAsync(
            _registerRoute,
            new { email = "person@life.test", password = _validPassword });

        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(
            (HttpStatusCode.Created, true, false, 1),
            (
                response.StatusCode,
                content.Contains("person@life.test", StringComparison.Ordinal),
                content.Contains(_validPassword, StringComparison.Ordinal),
                users.Users.Count));
    }

    [Fact]
    public async Task Register_WithValidData_StoresOnlyTheHashOfThePassword()
    {
        var (client, users, _) = CreateClient();

        await RegisterAsync(client);

        var hash = users.Users.Single().PasswordHash;
        Assert.Equal(
            (true, false),
            (hash?.StartsWith("PBKDF2-SHA256$", StringComparison.Ordinal), hash?.Contains(_validPassword)));
    }

    [Fact]
    public async Task Register_WithValidData_SendsTheVerificationEmailToTheNewUser()
    {
        var (client, _, sender) = CreateClient();

        await RegisterAsync(client);

        Assert.Equal("person@life.test", sender.Sent.Single().Email);
    }

    [Fact]
    public async Task Register_WithRegisteredEmailInDifferentCase_Returns409WithoutExtraData()
    {
        var (client, users, _) = CreateClient();
        await RegisterAsync(client);

        var response = await client.PostAsJsonAsync(
            _registerRoute,
            new { email = "PERSON@LIFE.TEST", password = _validPassword });

        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal(
            (HttpStatusCode.Conflict, false, 1),
            (
                response.StatusCode,
                content.Contains("person@life.test", StringComparison.OrdinalIgnoreCase),
                users.Users.Count));
    }

    [Fact]
    public async Task Register_WithPasswordThatBreaksThePolicy_Returns400WithTheDetail()
    {
        var (client, users, _) = CreateClient();

        var response = await client.PostAsJsonAsync(
            _registerRoute,
            new { email = "person@life.test", password = "short" });

        var errors = await ReadErrorsAsync(response);
        Assert.Equal(
            (HttpStatusCode.BadRequest, true, 0),
            (response.StatusCode, errors.ContainsKey("Password"), users.Users.Count));
    }

    [Fact]
    public async Task Register_WithInvalidEmail_Returns400WithTheEmailField()
    {
        var (client, _, _) = CreateClient();

        var response = await client.PostAsJsonAsync(
            _registerRoute,
            new { email = "not-an-email", password = _validPassword });

        var errors = await ReadErrorsAsync(response);
        Assert.Equal((HttpStatusCode.BadRequest, true), (response.StatusCode, errors.ContainsKey("Email")));
    }

    [Fact]
    public async Task Register_WhenThePermitLimitIsExceeded_Returns429()
    {
        var (client, _, _) = CreateClient(settings => settings["RateLimiting:Registration:PermitLimit"] = "2");
        var body = new { email = "not-an-email", password = _validPassword };

        var statuses = new List<HttpStatusCode>();

        for (var attempt = 0; attempt < 3; attempt++)
        {
            statuses.Add((await client.PostAsJsonAsync(_registerRoute, body)).StatusCode);
        }

        Assert.Equal(
            [HttpStatusCode.BadRequest, HttpStatusCode.BadRequest, HttpStatusCode.TooManyRequests],
            statuses);
    }

    [Fact]
    public async Task VerifyEmail_WithTheEmailedToken_Returns204AndActivatesTheUser()
    {
        var (client, users, sender) = CreateClient();
        await RegisterAsync(client);
        var token = sender.Sent.Single().Token;

        var response = await client.GetAsync($"{_verifyRoute}?token={Uri.EscapeDataString(token)}");

        var user = users.Users.Single();
        Assert.Equal(
            (HttpStatusCode.NoContent, UserStatus.Active, true),
            (response.StatusCode, user.Status, user.EmailVerifiedAt.HasValue));
    }

    [Fact]
    public async Task VerifyEmail_WhenTheTokenIsUsedTwice_Returns400TheSecondTime()
    {
        var (client, _, sender) = CreateClient();
        await RegisterAsync(client);
        var url = $"{_verifyRoute}?token={Uri.EscapeDataString(sender.Sent.Single().Token)}";
        await client.GetAsync(url);

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task VerifyEmail_WithExpiredToken_Returns400AndKeepsTheUserPending()
    {
        var (client, users, sender) = CreateClient();
        await RegisterAsync(client);
        users.Tokens.Single().ExpiresAt = DateTime.UtcNow.AddMinutes(-1);

        var response = await client.GetAsync(VerifyUrl(sender.Sent.Single().Token));

        Assert.Equal(
            (HttpStatusCode.BadRequest, UserStatus.Pending),
            (response.StatusCode, users.Users.Single().Status));
    }

    [Fact]
    public async Task VerifyEmail_WithUnknownToken_Returns400()
    {
        var (client, _, _) = CreateClient();

        var response = await client.GetAsync($"{_verifyRoute}?token=unknown");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task VerifyEmail_WithoutToken_Returns400()
    {
        var (client, _, _) = CreateClient();

        var response = await client.GetAsync(_verifyRoute);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
