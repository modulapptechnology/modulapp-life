using FluentValidation;

namespace Application.Users;

public sealed class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public const int TokenMaxLength = 200;

    public VerifyEmailRequestValidator()
    {
        RuleFor(request => request.Token)
            .NotEmpty().WithMessage("El enlace de verificación no es válido.")
            .MaximumLength(TokenMaxLength).WithMessage("El enlace de verificación no es válido.");
    }
}
