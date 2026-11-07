using FluentValidation;

namespace Application.Users;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .MaximumLength(RegisterUserRequestValidator.EmailMaxLength)
            .WithMessage(
                $"El correo electrónico no puede superar {RegisterUserRequestValidator.EmailMaxLength} caracteres.")
            .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.");

        RuleFor(request => request.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MaximumLength(RegisterUserRequestValidator.PasswordMaxLength)
            .WithMessage(
                $"La contraseña no puede superar {RegisterUserRequestValidator.PasswordMaxLength} caracteres.");
    }
}
