using FluentValidation;

namespace Life.Application.Users;

public sealed class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public const int EmailMaxLength = 320;

    public const int PasswordMinLength = 10;

    public const int PasswordMaxLength = 128;

    public RegisterUserRequestValidator()
    {
        RuleFor(request => request.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .MaximumLength(EmailMaxLength)
            .WithMessage($"El correo electrónico no puede superar {EmailMaxLength} caracteres.")
            .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.");

        RuleFor(request => request.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(PasswordMinLength)
            .WithMessage($"La contraseña debe tener al menos {PasswordMinLength} caracteres.")
            .MaximumLength(PasswordMaxLength)
            .WithMessage($"La contraseña no puede superar {PasswordMaxLength} caracteres.")
            .Must(password => password.Any(char.IsUpper))
            .WithMessage("La contraseña debe incluir al menos una letra mayúscula.")
            .Must(password => password.Any(char.IsLower))
            .WithMessage("La contraseña debe incluir al menos una letra minúscula.")
            .Must(password => password.Any(char.IsDigit))
            .WithMessage("La contraseña debe incluir al menos un número.");
    }
}
