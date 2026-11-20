using Life.Application.Users;

namespace Life.Application.Tests.Users;

public class ChangePasswordRequestValidatorTests
{
    private readonly ChangePasswordRequestValidator _validator = new();

    [Fact]
    public void Validate_WithBothPasswords_IsValid()
    {
        var request = new ChangePasswordRequest("Abcdefgh12", "Zyxwvuts98");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithAnEmptyCurrentPassword_ReportsIt()
    {
        var request = new ChangePasswordRequest(string.Empty, "Zyxwvuts98");

        var result = _validator.Validate(request);

        Assert.Equal(
            ["La contraseña actual es obligatoria."],
            result.Errors.Select(error => error.ErrorMessage));
    }

    [Fact]
    public void Validate_WithACurrentPasswordBeyondTheMaximumLength_ReportsItAsIncorrect()
    {
        var request = new ChangePasswordRequest(new string('a', 129), "Zyxwvuts98");

        var result = _validator.Validate(request);

        Assert.Equal(
            ["La contraseña actual no es correcta."],
            result.Errors.Select(error => error.ErrorMessage));
    }

    [Theory]
    [InlineData("", "La contraseña es obligatoria.")]
    [InlineData("Ab1", "La contraseña debe tener al menos 10 caracteres.")]
    [InlineData("abcdefgh12", "La contraseña debe incluir al menos una letra mayúscula.")]
    [InlineData("ABCDEFGH12", "La contraseña debe incluir al menos una letra minúscula.")]
    [InlineData("Abcdefghij", "La contraseña debe incluir al menos un número.")]
    public void Validate_WithANewPasswordThatBreaksThePolicy_ReportsTheFirstFailure(string newPassword, string message)
    {
        var request = new ChangePasswordRequest("Abcdefgh12", newPassword);

        var result = _validator.Validate(request);

        Assert.Equal([message], result.Errors.Select(error => error.ErrorMessage));
    }
}
