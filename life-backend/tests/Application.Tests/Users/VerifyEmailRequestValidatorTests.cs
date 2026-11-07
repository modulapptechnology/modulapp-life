using Application.Users;

namespace Application.Tests.Users;

public class VerifyEmailRequestValidatorTests
{
    private readonly VerifyEmailRequestValidator _validator = new();

    [Fact]
    public void Validate_WithToken_IsValid()
    {
        var request = new VerifyEmailRequest("abc123");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyToken_IsInvalid()
    {
        var request = new VerifyEmailRequest(string.Empty);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithTokenLongerThanTheLimit_IsInvalid()
    {
        var request = new VerifyEmailRequest(new string('a', VerifyEmailRequestValidator.TokenMaxLength + 1));

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }
}
