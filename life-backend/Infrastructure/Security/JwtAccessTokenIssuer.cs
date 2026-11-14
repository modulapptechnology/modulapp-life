using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Life.Application.Users;
using Life.Domain.Users;

namespace Life.Infrastructure.Security;

public sealed class JwtAccessTokenIssuer(AccessTokenOptions options) : IAccessTokenIssuer
{
    private static readonly byte[] _header = Encoding.UTF8.GetBytes("""{"alg":"HS256","typ":"JWT"}""");

    public AccessToken Issue(User user, DateTime issuedAt)
    {
        ArgumentNullException.ThrowIfNull(user);

        var expiresAt = issuedAt.Add(options.Lifetime);
        var payload = JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, object>
        {
            ["sub"] = user.Id.ToString(),
            ["email"] = user.Email,
            ["jti"] = Guid.NewGuid().ToString(),
            ["iss"] = options.Issuer,
            ["aud"] = options.Audience,
            ["iat"] = new DateTimeOffset(issuedAt).ToUnixTimeSeconds(),
            ["exp"] = new DateTimeOffset(expiresAt).ToUnixTimeSeconds(),
        });
        var unsigned = $"{Encode(_header)}.{Encode(payload)}";
        var signature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(options.SigningKey), Encoding.ASCII.GetBytes(unsigned));

        return new AccessToken($"{unsigned}.{Encode(signature)}", expiresAt);
    }

    private static string Encode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
