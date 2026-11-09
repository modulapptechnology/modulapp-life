using FluentValidation;

namespace Application.Users;

public sealed class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(request => request.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .MaximumLength(RegisterUserRequestValidator.EmailMaxLength)
            .WithMessage("El correo electrónico no tiene un formato válido.")
            .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.");

        RuleFor(request => request.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El código de verificación es obligatorio.")
            .Must(code => code.Length == VerificationToken.CodeLength && code.All(char.IsAsciiDigit))
            .WithMessage($"El código debe tener {VerificationToken.CodeLength} dígitos.");
    }
}
