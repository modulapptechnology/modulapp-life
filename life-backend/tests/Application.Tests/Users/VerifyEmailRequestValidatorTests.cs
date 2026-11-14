using Life.Application.Users;

namespace Life.Application.Tests.Users;

public class VerifyEmailRequestValidatorTests
{
    private readonly VerifyEmailRequestValidator _validator = new();

    [Fact]
    public void Validate_WithEmailAndSixDigitCode_IsValid()
    {
        var request = new VerifyEmailRequest("person@life.test", "012345");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WithInvalidEmail_ReportsTheEmailField(string email)
    {
        var request = new VerifyEmailRequest(email, "123456");

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(VerifyEmailRequest.Email)],
            result.Errors.Select(error => error.PropertyName).Distinct());
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("12345a")]
    [InlineData("١٢٣٤٥٦")]
    public void Validate_WithACodeThatIsNotSixDigits_ReportsTheCodeField(string code)
    {
        var request = new VerifyEmailRequest("person@life.test", code);

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(VerifyEmailRequest.Code)],
            result.Errors.Select(error => error.PropertyName).Distinct());
    }
}
