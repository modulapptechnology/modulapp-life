using FluentValidation;

namespace Application.Workspaces;

public sealed class CreateWorkspaceRequestValidator : AbstractValidator<CreateWorkspaceRequest>
{
    public const int NameMaxLength = 200;

    public const int DescriptionMaxLength = 1000;

    public CreateWorkspaceRequestValidator()
    {
        RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El nombre del espacio de trabajo es obligatorio.")
            .MaximumLength(NameMaxLength)
            .WithMessage($"El nombre del espacio de trabajo no puede superar {NameMaxLength} caracteres.");

        RuleFor(request => request.Type)
            .IsInEnum().WithMessage("El tipo de espacio de trabajo no es válido.");

        RuleFor(request => request.Description)
            .MaximumLength(DescriptionMaxLength)
            .WithMessage($"La descripción no puede superar {DescriptionMaxLength} caracteres.");
    }
}
