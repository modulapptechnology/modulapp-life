using Application.Users;

namespace Application.Tests.Users;

public class RegisterUserRequestValidatorTests
{
    private const string _validPassword = "Abcdefgh12";

    private readonly RegisterUserRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidEmailAndPassword_IsValid()
    {
        var request = new RegisterUserRequest("person@life.test", _validPassword);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    public void Validate_WithInvalidEmail_ReportsTheEmailField(string email)
    {
        var request = new RegisterUserRequest(email, _validPassword);

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(RegisterUserRequest.Email)],
            result.Errors.Select(error => error.PropertyName).Distinct());
    }

    [Fact]
    public void Validate_WithEmailLongerThanTheLimit_ReportsTheEmailField()
    {
        var email = new string('a', RegisterUserRequestValidator.EmailMaxLength) + "@life.test";
        var request = new RegisterUserRequest(email, _validPassword);

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterUserRequest.Email));
    }

    [Fact]
    public void Validate_WithPasswordOfTheMinimumLength_IsValid()
    {
        var password = "Aa1" + new string('b', RegisterUserRequestValidator.PasswordMinLength - 3);
        var request = new RegisterUserRequest("person@life.test", password);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithPasswordShorterThanTheMinimum_ReportsThePasswordField()
    {
        var password = "Aa1" + new string('b', RegisterUserRequestValidator.PasswordMinLength - 4);
        var request = new RegisterUserRequest("person@life.test", password);

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(RegisterUserRequest.Password)],
            result.Errors.Select(error => error.PropertyName).Distinct());
    }

    [Fact]
    public void Validate_WithPasswordLongerThanTheMaximum_ReportsThePasswordField()
    {
        var password = "Aa1" + new string('b', RegisterUserRequestValidator.PasswordMaxLength);
        var request = new RegisterUserRequest("person@life.test", password);

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(RegisterUserRequest.Password)],
            result.Errors.Select(error => error.PropertyName).Distinct());
    }

    [Theory]
    [InlineData("abcdefgh12", "mayúscula")]
    [InlineData("ABCDEFGH12", "minúscula")]
    [InlineData("Abcdefghij", "número")]
    public void Validate_WithPasswordMissingACharacterClass_ExplainsWhatIsMissing(
        string password,
        string expectedDetail)
    {
        var request = new RegisterUserRequest("person@life.test", password);

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains(expectedDetail));
    }

    [Fact]
    public void Validate_WithEmptyPassword_ReportsASingleMessage()
    {
        var request = new RegisterUserRequest("person@life.test", string.Empty);

        var result = _validator.Validate(request);

        Assert.Single(result.Errors);
    }
}
