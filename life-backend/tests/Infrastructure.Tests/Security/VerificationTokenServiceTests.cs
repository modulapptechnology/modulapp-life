using Life.Application.Users;
using Life.Infrastructure.Security;
using Life.Persistence.Configuration;

namespace Life.Infrastructure.Tests.Security;

public class VerificationTokenServiceTests
{
    private readonly VerificationTokenService _service = new();

    [Fact]
    public void Create_WhenCalled_ReturnsALongUrlSafeTokenWithoutPadding()
    {
        var token = _service.Create();

        Assert.Equal(
            (43, 0),
            (token.Value.Length, token.Value.Count(character => character is '+' or '/' or '=')));
    }

    [Fact]
    public void Create_WhenCalledTwice_ReturnsDifferentTokens()
    {
        var first = _service.Create();
        var second = _service.Create();

        Assert.NotEqual(first.Value, second.Value);
    }

    [Fact]
    public void Create_WhenCalled_ReturnsTheHashOfTheTokenAndNotTheTokenItself()
    {
        var token = _service.Create();

        Assert.Equal((_service.Hash(token.Value), true), (token.Hash, token.Hash != token.Value));
    }

    [Fact]
    public void CreateCode_WhenCalled_ReturnsACodeOfOnlyDigitsWithTheConfiguredLength()
    {
        var token = _service.CreateCode();

        Assert.Equal(
            (VerificationToken.CodeLength, true),
            (token.Value.Length, token.Value.All(char.IsAsciiDigit)));
    }

    [Fact]
    public void CreateCode_WhenCalledManyTimes_KeepsTheLeadingZeros()
    {
        var codes = Enumerable.Range(0, 2000).Select(_ => _service.CreateCode().Value);

        Assert.All(codes, code => Assert.Equal(VerificationToken.CodeLength, code.Length));
    }

    [Fact]
    public void CreateCode_WhenCalledManyTimes_ReturnsDifferentCodes()
    {
        var codes = Enumerable.Range(0, 20).Select(_ => _service.CreateCode().Value).Distinct();

        Assert.True(codes.Count() > 1);
    }

    [Fact]
    public void CreateCode_WhenCalled_ReturnsTheHashOfTheCodeAndNotTheCodeItself()
    {
        var token = _service.CreateCode();

        Assert.Equal((_service.Hash(token.Value), true), (token.Hash, token.Hash != token.Value));
    }

    [Fact]
    public void Hash_WhenCalled_ReturnsAHashThatFitsInTheTokenHashColumn()
    {
        var hash = _service.Hash("123456");

        Assert.Equal(EmailVerificationTokenConfiguration.TokenHashMaxLength, hash.Length);
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
