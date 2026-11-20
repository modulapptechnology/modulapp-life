using FluentValidation;

namespace Life.Application.Users;

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public const int TokenMaxLength = 128;

    public ResetPasswordRequestValidator()
    {
        RuleFor(request => request.Token)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El token de recuperación es obligatorio.")
            .MaximumLength(TokenMaxLength).WithMessage("El token de recuperación no es válido.");

        RuleFor(request => request.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(RegisterUserRequestValidator.PasswordMinLength)
            .WithMessage($"La contraseña debe tener al menos {RegisterUserRequestValidator.PasswordMinLength} caracteres.")
            .MaximumLength(RegisterUserRequestValidator.PasswordMaxLength)
            .WithMessage($"La contraseña no puede superar {RegisterUserRequestValidator.PasswordMaxLength} caracteres.")
            .Must(password => password.Any(char.IsUpper))
            .WithMessage("La contraseña debe incluir al menos una letra mayúscula.")
            .Must(password => password.Any(char.IsLower))
            .WithMessage("La contraseña debe incluir al menos una letra minúscula.")
            .Must(password => password.Any(char.IsDigit))
            .WithMessage("La contraseña debe incluir al menos un número.");
    }
}
