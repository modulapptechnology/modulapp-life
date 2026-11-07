using System.Security.Cryptography;
using Application.Users;

namespace Infrastructure.Security;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    public const string Prefix = "PBKDF2-SHA256";

    public const int Iterations = 600_000;

    private const int _saltSize = 16;

    private const int _hashSize = 32;

    private static readonly Lazy<string> _unknownUserHash = new(() => new Pbkdf2PasswordHasher().Hash("unknown"));

    public string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(_saltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, _hashSize);

        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string? passwordHash)
    {
        ArgumentNullException.ThrowIfNull(password);

        var parsed = Parse(passwordHash);

        if (parsed is null)
        {
            Verify(password, _unknownUserHash.Value);

            return false;
        }

        var (iterations, salt, expected) = parsed.Value;
        var actual = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expected.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static (int Iterations, byte[] Salt, byte[] Hash)? Parse(string? passwordHash)
    {
        var parts = passwordHash?.Split('$');

        if (parts is not [Prefix, var iterationsText, var saltText, var hashText]
            || !int.TryParse(iterationsText, out var iterations)
            || iterations <= 0
            || iterations > Iterations)
        {
            return null;
        }

        try
        {
            var salt = Convert.FromBase64String(saltText);
            var hash = Convert.FromBase64String(hashText);

            return salt.Length == _saltSize && hash.Length == _hashSize ? (iterations, salt, hash) : null;
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
