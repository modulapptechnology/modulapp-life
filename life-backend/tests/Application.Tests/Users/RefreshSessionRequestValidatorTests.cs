using Life.Application.Users;

namespace Life.Application.Tests.Users;

public class RefreshSessionRequestValidatorTests
{
    private readonly RefreshSessionRequestValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithoutToken_IsInvalid(string token)
    {
        var request = new RefreshSessionRequest(token);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithTooLongToken_IsInvalid()
    {
        var request = new RefreshSessionRequest(
            new string('a', RefreshSessionRequestValidator.RefreshTokenMaxLength + 1));

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithToken_IsValid()
    {
        var request = new RefreshSessionRequest("a-token");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }
}
