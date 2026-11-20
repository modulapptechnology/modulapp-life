using FluentValidation;

namespace Life.Application.Users;

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(request => request.CurrentPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("La contraseña actual es obligatoria.")
            .MaximumLength(RegisterUserRequestValidator.PasswordMaxLength)
            .WithMessage("La contraseña actual no es correcta.");

        RuleFor(request => request.NewPassword).MeetPasswordPolicy();
    }
}
