using Life.Infrastructure.Security;
using Life.Persistence.Configuration;

namespace Life.Infrastructure.Tests.Security;

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
    public void Verify_WithTheOriginalPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("Abcdefgh12");

        var matches = _hasher.Verify("Abcdefgh12", hash);

        Assert.True(matches);
    }

    [Fact]
    public void Verify_WithADifferentPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("Abcdefgh12");

        var matches = _hasher.Verify("Abcdefgh13", hash);

        Assert.False(matches);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("plain-text")]
    [InlineData("PBKDF2-SHA256$600000$not-base64$not-base64")]
    [InlineData("PBKDF2-SHA256$600000$AA==$")]
    [InlineData("PBKDF2-SHA256$0$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
    [InlineData("OTHER$600000$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
    public void Verify_WithMissingOrMalformedHash_ReturnsFalse(string? hash)
    {
        var matches = _hasher.Verify("Abcdefgh12", hash);

        Assert.False(matches);
    }

    [Fact]
    public void Verify_WithNullPassword_ThrowsArgumentNullException()
    {
        Action action = () => _ = _hasher.Verify(null!, null);

        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void Hash_WithNullPassword_ThrowsArgumentNullException()
    {
        var action = () => _hasher.Hash(null!);

        Assert.Throws<ArgumentNullException>(action);
    }
}
