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
public class RefreshTests(ApiFactory api)
{
    private const string _loginRoute = "/api/v1/Auth/login";

    private const string _refreshRoute = "/api/v1/Auth/refresh";

    private const string _email = "person@life.test";

    private const string _password = "Abcdefgh12";

    private (HttpClient Client, InMemoryRefreshTokenRepository RefreshTokens) CreateClient(
        Action<IDictionary<string, string?>>? settings = null)
    {
        var users = new InMemoryUserRepository();
        var refreshTokens = new InMemoryRefreshTokenRepository(users);
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
                services.AddSingleton<IUnitOfWork, PassThroughUnitOfWork>();
            });
        });

        var hasher = factory.Services.GetRequiredService<IPasswordHasher>();
        users.Seed(new User { Email = _email, PasswordHash = hasher.Hash(_password), Status = UserStatus.Active });

        return (factory.CreateClient(), refreshTokens);
    }

    private static async Task<string> LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(_loginRoute, new { email = _email, password = _password });

        return await ReadRefreshTokenAsync(response);
    }

    private static Task<HttpResponseMessage> RefreshAsync(HttpClient client, string refreshToken)
    {
        return client.PostAsJsonAsync(_refreshRoute, new { refreshToken });
    }

    private static async Task<string> ReadRefreshTokenAsync(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return body.RootElement.GetProperty("refreshToken").GetString()!;
    }

    [Fact]
    public async Task Refresh_WithValidToken_Returns200WithANewPair()
    {
        var (client, _) = CreateClient();
        var refreshToken = await LoginAsync(client);

        var response = await RefreshAsync(client, refreshToken);

        var newRefreshToken = await ReadRefreshTokenAsync(response);
        Assert.Equal((HttpStatusCode.OK, true), (response.StatusCode, newRefreshToken != refreshToken));
    }

    [Fact]
    public async Task Refresh_WithAlreadyRotatedToken_Returns401()
    {
        var (client, _) = CreateClient();
        var refreshToken = await LoginAsync(client);
        await RefreshAsync(client, refreshToken);

        var response = await RefreshAsync(client, refreshToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithAlreadyRotatedToken_RevokesTheSessionChain()
    {
        var (client, _) = CreateClient();
        var firstToken = await LoginAsync(client);
        var secondToken = await ReadRefreshTokenAsync(await RefreshAsync(client, firstToken));
        await RefreshAsync(client, firstToken);

        var response = await RefreshAsync(client, secondToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithUnknownToken_Returns401()
    {
        var (client, _) = CreateClient();

        var response = await RefreshAsync(client, "unknown-token");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithoutToken_Returns400()
    {
        var (client, _) = CreateClient();

        var response = await RefreshAsync(client, string.Empty);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_AfterLogin_StoresOnlyTheHashOfTheToken()
    {
        var (client, refreshTokens) = CreateClient();
        var refreshToken = await LoginAsync(client);

        var stored = Assert.Single(refreshTokens.Tokens);

        Assert.NotEqual(refreshToken, stored.TokenHash);
    }

    [Fact]
    public async Task Refresh_WithConfiguredLifetime_ExpiresTheTokenAfterThoseDays()
    {
        var (client, refreshTokens) = CreateClient(settings =>
            settings["Authentication:RefreshToken:LifetimeDays"] = "5");
        await LoginAsync(client);

        var stored = Assert.Single(refreshTokens.Tokens);

        Assert.InRange((stored.ExpiresAt - DateTime.UtcNow).TotalDays, 4.9, 5.1);
    }
}
