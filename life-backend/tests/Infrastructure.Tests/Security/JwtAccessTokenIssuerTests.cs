using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Life.Domain.Users;
using Life.Infrastructure.Security;

namespace Life.Infrastructure.Tests.Security;

public class JwtAccessTokenIssuerTests
{
    private const string _signingKey = "test-only-signing-key-not-a-secret-0123456789";

    private static readonly DateTime _now = new(2026, 11, 7, 12, 0, 0, DateTimeKind.Utc);

    private static readonly AccessTokenOptions _options =
        new(_signingKey, "life-api", "life-app", TimeSpan.FromMinutes(15));

    private readonly User _user = new() { Id = Guid.NewGuid(), Email = "person@life.test" };

    private static byte[] Decode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + ((4 - (padded.Length % 4)) % 4), '=');

        return Convert.FromBase64String(padded);
    }

    private static JsonElement ReadPayload(string token)
    {
        using var document = JsonDocument.Parse(Decode(token.Split('.')[1]));

        return document.RootElement.Clone();
    }

    [Fact]
    public void Issue_WithUser_ReturnsATokenWithTheUserIssuerAudienceAndExpiration()
    {
        var issuer = new JwtAccessTokenIssuer(_options);

        var token = issuer.Issue(_user, _now);

        var payload = ReadPayload(token.Value);
        Assert.Equal(
            (_user.Id.ToString(), "person@life.test", "life-api", "life-app"),
            (
                payload.GetProperty("sub").GetString(),
                payload.GetProperty("email").GetString(),
                payload.GetProperty("iss").GetString(),
                payload.GetProperty("aud").GetString()));
    }

    [Fact]
    public void Issue_WithUser_ExpiresAfterTheConfiguredLifetime()
    {
        var issuer = new JwtAccessTokenIssuer(_options);

        var token = issuer.Issue(_user, _now);

        var payload = ReadPayload(token.Value);
        Assert.Equal(
            (_now.AddMinutes(15), new DateTimeOffset(_now.AddMinutes(15)).ToUnixTimeSeconds()),
            (token.ExpiresAt, payload.GetProperty("exp").GetInt64()));
    }

    [Fact]
    public void Issue_WithUser_SignsTheTokenWithHmacSha256AndTheSigningKey()
    {
        var issuer = new JwtAccessTokenIssuer(_options);

        var token = issuer.Issue(_user, _now);

        var parts = token.Value.Split('.');
        var expected = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(_signingKey),
            Encoding.ASCII.GetBytes($"{parts[0]}.{parts[1]}"));
        Assert.Equal(expected, Decode(parts[2]));
    }

    [Fact]
    public void Issue_WithUser_DeclaresTheAlgorithmInTheHeader()
    {
        var issuer = new JwtAccessTokenIssuer(_options);

        var token = issuer.Issue(_user, _now);

        using var header = JsonDocument.Parse(Decode(token.Value.Split('.')[0]));
        Assert.Equal(
            ("HS256", "JWT"),
            (header.RootElement.GetProperty("alg").GetString(), header.RootElement.GetProperty("typ").GetString()));
    }

    [Fact]
    public void Issue_CalledTwice_ReturnsDifferentTokenIdentifiers()
    {
        var issuer = new JwtAccessTokenIssuer(_options);

        var first = issuer.Issue(_user, _now);
        var second = issuer.Issue(_user, _now);

        Assert.NotEqual(
            ReadPayload(first.Value).GetProperty("jti").GetString(),
            ReadPayload(second.Value).GetProperty("jti").GetString());
    }

    [Fact]
    public void Issue_WithNullUser_ThrowsArgumentNullException()
    {
        var issuer = new JwtAccessTokenIssuer(_options);

        var action = () => issuer.Issue(null!, _now);

        Assert.Throws<ArgumentNullException>(action);
    }
}
