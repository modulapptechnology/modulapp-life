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

        RuleFor(request => request.NewPassword).MeetPasswordPolicy();
    }
}
