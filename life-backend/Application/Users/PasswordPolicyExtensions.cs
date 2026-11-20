using FluentValidation;

namespace Life.Application.Users;

public static class PasswordPolicyExtensions
{
    public static IRuleBuilderOptions<T, string> MeetPasswordPolicy<T>(this IRuleBuilderInitial<T, string> rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        return rule
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
