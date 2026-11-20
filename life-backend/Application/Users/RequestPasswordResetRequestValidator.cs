using FluentValidation;

namespace Life.Application.Users;

public sealed class RequestPasswordResetRequestValidator : AbstractValidator<RequestPasswordResetRequest>
{
    public RequestPasswordResetRequestValidator()
    {
        RuleFor(request => request.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .MaximumLength(RegisterUserRequestValidator.EmailMaxLength)
            .WithMessage("El correo electrónico no tiene un formato válido.")
            .EmailAddress().WithMessage("El correo electrónico no tiene un formato válido.");
    }
}
