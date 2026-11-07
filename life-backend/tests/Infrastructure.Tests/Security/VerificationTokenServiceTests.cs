using Infrastructure.Security;
using Persistence.Configuration;

namespace Infrastructure.Tests.Security;

public class VerificationTokenServiceTests
{
    private readonly VerificationTokenService _service = new();

    [Fact]
    public void Create_WhenCalled_ReturnsAUrlSafeTokenWithoutPadding()
    {
        var token = _service.Create();

        Assert.DoesNotContain(token.Value, character => character is '+' or '/' or '=');
    }

    [Fact]
    public void Create_WhenCalled_ReturnsTheHashOfTheTokenAndNotTheTokenItself()
    {
        var token = _service.Create();

        Assert.Equal((_service.Hash(token.Value), true), (token.Hash, token.Hash != token.Value));
    }

    [Fact]
    public void Create_WhenCalledTwice_ReturnsDifferentTokens()
    {
        var first = _service.Create();
        var second = _service.Create();

        Assert.NotEqual(first.Value, second.Value);
    }

    [Fact]
    public void Create_WhenCalled_ReturnsAHashThatFitsInTheTokenHashColumn()
    {
        var token = _service.Create();

        Assert.Equal(EmailVerificationTokenConfiguration.TokenHashMaxLength, token.Hash.Length);
    }

    [Fact]
    public void Hash_WithTheSameToken_ReturnsTheSameHash()
    {
        var token = "abc";

        var first = _service.Hash(token);
        var second = _service.Hash(token);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Hash_WithKnownToken_ReturnsItsSha256InUppercaseHexadecimal()
    {
        var token = "abc";

        var hash = _service.Hash(token);

        Assert.Equal("BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD", hash);
    }
}
