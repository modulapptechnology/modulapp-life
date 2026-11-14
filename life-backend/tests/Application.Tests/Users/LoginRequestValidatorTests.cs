using Life.Application.Users;

namespace Life.Application.Tests.Users;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Validate_WithEmailAndPassword_IsValid()
    {
        var request = new LoginRequest("person@life.test", "any-password");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WithInvalidEmail_ReportsTheEmailField(string email)
    {
        var request = new LoginRequest(email, "any-password");

        var result = _validator.Validate(request);

        Assert.Equal(
            new[] { nameof(LoginRequest.Email) },
            result.Errors.Select(error => error.PropertyName).Distinct());
    }

    [Fact]
    public void Validate_WithEmptyPassword_ReportsThePasswordField()
    {
        var request = new LoginRequest("person@life.test", string.Empty);

        var result = _validator.Validate(request);

        Assert.Equal(
            new[] { nameof(LoginRequest.Password) },
            result.Errors.Select(error => error.PropertyName).Distinct());
    }

    [Fact]
    public void Validate_WithPasswordLongerThanTheLimit_ReportsThePasswordField()
    {
        var request = new LoginRequest(
            "person@life.test",
            new string('a', RegisterUserRequestValidator.PasswordMaxLength + 1));

        var result = _validator.Validate(request);

        Assert.Equal(
            new[] { nameof(LoginRequest.Password) },
            result.Errors.Select(error => error.PropertyName).Distinct());
    }

    [Fact]
    public void Validate_WithAPasswordThatDoesNotMeetTheRegistrationPolicy_IsStillValid()
    {
        var request = new LoginRequest("person@life.test", "short");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }
}
