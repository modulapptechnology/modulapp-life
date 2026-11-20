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

        RuleFor(request => request.Password).MeetPasswordPolicy();
    }
}
