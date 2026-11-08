using FluentValidation;

namespace Application.Users;

public sealed class RefreshSessionRequestValidator : AbstractValidator<RefreshSessionRequest>
{
    public const int RefreshTokenMaxLength = 200;

    public RefreshSessionRequestValidator()
    {
        RuleFor(request => request.RefreshToken)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El token de renovación es obligatorio.")
            .MaximumLength(RefreshTokenMaxLength)
            .WithMessage($"El token de renovación no puede superar {RefreshTokenMaxLength} caracteres.");
    }
}
