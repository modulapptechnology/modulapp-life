using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Life.Application.Users;

namespace Life.Flows.Tests.Support;

public sealed record FlowUser(string Email, Guid Id, HttpClient Client)
{
    public const string Password = "Abcdefgh12";

    private const string _registerRoute = "/api/v1/Auth/register";

    private const string _verifyRoute = "/api/v1/Auth/verify-email";

    private const string _loginRoute = "/api/v1/Auth/login";

    public static string NewEmail(string label)
    {
        return $"{label}-{Guid.NewGuid():N}@life.test";
    }

    public static async Task RegisterAsync(FlowsFactory api, string email)
    {
        await api.CreateClient()
            .PostAsJsonAsync(_registerRoute, new { email, password = Password })
            .ExpectAsync("register", HttpStatusCode.Created);
    }

    public static async Task<FlowUser> SignUpAsync(FlowsFactory api, string label)
    {
        var email = NewEmail(label);
        var client = api.CreateClient();

        var registered = await client
            .PostAsJsonAsync(_registerRoute, new { email, password = Password })
            .ExpectAsync("register", HttpStatusCode.Created);
        var account = await registered.Content.ReadFromJsonAsync<RegisterUserResponse>();

        var code = api.Emails.LastCodeFor(email)
            ?? throw new InvalidOperationException($"No verification code was sent to '{email}'.");
        await client
            .PostAsJsonAsync(_verifyRoute, new { email, code })
            .ExpectAsync("verify email", HttpStatusCode.NoContent);

        var login = await client
            .PostAsJsonAsync(_loginRoute, new { email, password = Password })
            .ExpectAsync("login", HttpStatusCode.OK);
        var session = await login.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            session!.TokenType,
            session.AccessToken);

        return new FlowUser(email, account!.Id, client);
    }
}
