using System.Net;
using System.Net.Http.Headers;
using API.Tests.Support;
using Application.Users;
using Domain.Users;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests.Security;

[Collection(ApiCollection.Name)]
public class AccessTokenValidationTests(ApiFactory api)
{
    private const string _route = "/api/v1/JwtProbe";

    private AccessToken IssueToken(DateTime issuedAt)
    {
        var issuer = api.Services.GetRequiredService<IAccessTokenIssuer>();

        return issuer.Issue(new User { Id = Guid.NewGuid(), Email = "person@life.test" }, issuedAt);
    }

    private async Task<HttpStatusCode> GetWithTokenAsync(string? token)
    {
        var client = api.CreateClient();

        if (token is not null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await client.GetAsync(_route);

        return response.StatusCode;
    }

    [Fact]
    public async Task Get_WithValidAccessToken_Returns200()
    {
        var token = IssueToken(DateTime.UtcNow);

        var status = await GetWithTokenAsync(token.Value);

        Assert.Equal(HttpStatusCode.OK, status);
    }

    [Fact]
    public async Task Get_WithExpiredAccessToken_Returns401()
    {
        var token = IssueToken(DateTime.UtcNow.AddHours(-2));

        var status = await GetWithTokenAsync(token.Value);

        Assert.Equal(HttpStatusCode.Unauthorized, status);
    }

    [Fact]
    public async Task Get_WithTamperedAccessToken_Returns401()
    {
        var token = IssueToken(DateTime.UtcNow);
        var parts = token.Value.Split('.');
        var tampered = $"{parts[0]}.{parts[1]}.{new string(parts[2].Reverse().ToArray())}";

        var status = await GetWithTokenAsync(tampered);

        Assert.Equal(HttpStatusCode.Unauthorized, status);
    }

    [Fact]
    public async Task Get_WithoutAccessToken_Returns401()
    {
        var status = await GetWithTokenAsync(null);

        Assert.Equal(HttpStatusCode.Unauthorized, status);
    }
}
