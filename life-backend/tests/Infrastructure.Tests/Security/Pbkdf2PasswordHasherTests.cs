using Infrastructure.Security;
using Persistence.Configuration;

namespace Infrastructure.Tests.Security;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Hash_WithPassword_ReturnsTheAlgorithmIterationsSaltAndHash()
    {
        var password = "Abcdefgh12";

        var hash = _hasher.Hash(password);

        var parts = hash.Split('$');
        Assert.Equal(
            (4, Pbkdf2PasswordHasher.Prefix, Pbkdf2PasswordHasher.Iterations.ToString()),
            (parts.Length, parts[0], parts[1]));
    }

    [Fact]
    public void Hash_WithPassword_DoesNotContainThePlainPassword()
    {
        var password = "Abcdefgh12";

        var hash = _hasher.Hash(password);

        Assert.DoesNotContain(password, hash, StringComparison.Ordinal);
    }

    [Fact]
    public void Hash_WithTheSamePasswordTwice_ReturnsDifferentHashesBecauseOfTheSalt()
    {
        var password = "Abcdefgh12";

        var first = _hasher.Hash(password);
        var second = _hasher.Hash(password);

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Hash_WithTheLongestAllowedPassword_FitsInThePasswordHashColumn()
    {
        var password = new string('a', 128);

        var hash = _hasher.Hash(password);

        Assert.True(hash.Length <= UserConfiguration.PasswordHashMaxLength);
    }

    [Fact]
    public void Hash_WithNullPassword_ThrowsArgumentNullException()
    {
        var action = () => _hasher.Hash(null!);

        Assert.Throws<ArgumentNullException>(action);
    }
}
