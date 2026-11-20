using Life.Application.Users;

namespace Life.Application.Tests.Users;

public class ResetPasswordRequestValidatorTests
{
    private readonly ResetPasswordRequestValidator _validator = new();

    [Fact]
    public void Validate_WithATokenAndAValidPassword_IsValid()
    {
        var request = new ResetPasswordRequest("plain-token", "Abcdefgh12");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithoutAToken_ReportsTheTokenField(string token)
    {
        var request = new ResetPasswordRequest(token, "Abcdefgh12");

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(ResetPasswordRequest.Token)],
            result.Errors.Select(error => error.PropertyName).Distinct());
    }

    [Fact]
    public void Validate_WithATokenLongerThanTheLimit_ReportsTheTokenField()
    {
        var request = new ResetPasswordRequest(
            new string('a', ResetPasswordRequestValidator.TokenMaxLength + 1),
            "Abcdefgh12");

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(ResetPasswordRequest.Token)],
            result.Errors.Select(error => error.PropertyName).Distinct());
    }

    [Theory]
    [InlineData("")]
    [InlineData("Abcdefg1")]
    [InlineData("abcdefgh12")]
    [InlineData("ABCDEFGH12")]
    [InlineData("Abcdefghij")]
    public void Validate_WithAPasswordThatBreaksThePolicy_ReportsTheNewPasswordField(string password)
    {
        var request = new ResetPasswordRequest("plain-token", password);

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(ResetPasswordRequest.NewPassword)],
            result.Errors.Select(error => error.PropertyName).Distinct());
    }

    [Fact]
    public void Validate_WithAPasswordLongerThanTheLimit_ReportsTheNewPasswordField()
    {
        var password = "Aa1" + new string('x', RegisterUserRequestValidator.PasswordMaxLength);
        var request = new ResetPasswordRequest("plain-token", password);

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(ResetPasswordRequest.NewPassword)],
            result.Errors.Select(error => error.PropertyName).Distinct());
    }
}
