using System.Text.Json;
using FluentValidation;
using Life.Domain.Sync;

namespace Life.Application.Sync;

public sealed class SyncOperationRequestValidator : AbstractValidator<SyncOperationRequest>
{
    public SyncOperationRequestValidator()
    {
        RuleFor(operation => operation.OperationId)
            .NotEqual(Guid.Empty).WithMessage("El identificador de la operación es obligatorio.");

        RuleFor(operation => operation.EntityId)
            .NotEqual(Guid.Empty).WithMessage("El identificador del registro es obligatorio.");

        RuleFor(operation => operation.EntityType)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El tipo de registro es obligatorio.")
            .MaximumLength(SyncRequestValidator.EntityTypeMaxLength)
            .WithMessage(
                $"El tipo de registro no puede superar {SyncRequestValidator.EntityTypeMaxLength} caracteres.");

        RuleFor(operation => operation.Kind)
            .IsInEnum().WithMessage("El tipo de operación no es válido.");

        RuleFor(operation => operation.Payload)
            .Must(payload => payload is { ValueKind: JsonValueKind.Object })
            .When(operation => operation.Kind != SyncOperationKind.Delete)
            .WithMessage("El contenido de la operación es obligatorio.");
    }
}
