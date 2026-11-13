using FluentValidation;

namespace Application.Workspaces;

public sealed class UpdateWorkspaceRequestValidator : AbstractValidator<UpdateWorkspaceRequest>
{
    public UpdateWorkspaceRequestValidator()
    {
        RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El nombre del espacio de trabajo es obligatorio.")
            .MaximumLength(CreateWorkspaceRequestValidator.NameMaxLength)
            .WithMessage(
                $"El nombre del espacio de trabajo no puede superar {CreateWorkspaceRequestValidator.NameMaxLength} caracteres.");

        RuleFor(request => request.Description)
            .MaximumLength(CreateWorkspaceRequestValidator.DescriptionMaxLength)
            .WithMessage(
                $"La descripción no puede superar {CreateWorkspaceRequestValidator.DescriptionMaxLength} caracteres.");

        RuleFor(request => request.RowVersion)
            .NotEmpty().WithMessage("La versión del espacio de trabajo es obligatoria.");
    }
}
