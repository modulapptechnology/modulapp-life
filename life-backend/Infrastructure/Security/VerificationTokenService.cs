using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Application.Users;

namespace Infrastructure.Security;

public sealed class VerificationTokenService : IVerificationTokenService
{
    private const int _tokenSize = 32;

    private static readonly int _codeUpperBound = (int)Math.Pow(10, VerificationToken.CodeLength);

    public VerificationToken Create()
    {
        var value = Convert.ToBase64String(RandomNumberGenerator.GetBytes(_tokenSize))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        return new VerificationToken(value, Hash(value));
    }

    public VerificationToken CreateCode()
    {
        var code = RandomNumberGenerator.GetInt32(_codeUpperBound)
            .ToString($"D{VerificationToken.CodeLength}", CultureInfo.InvariantCulture);

        return new VerificationToken(code, Hash(code));
    }

    public string Hash(string token)
    {
        ArgumentNullException.ThrowIfNull(token);

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
