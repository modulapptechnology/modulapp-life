using FluentValidation;

namespace Life.Application.Sync;

public sealed class SyncRequestValidator : AbstractValidator<SyncRequest>
{
    public const int MaxOperations = 100;

    public const int MaxPageSize = 200;

    public const int DefaultPageSize = 100;

    public const int EntityTypeMaxLength = 64;

    public SyncRequestValidator()
    {
        RuleFor(request => request.Cursor)
            .Must(cursor => SyncCursor.TryParse(cursor, out _))
            .WithMessage("El punto de sincronización no es válido.");

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, MaxPageSize)
            .When(request => request.PageSize is not null)
            .WithMessage($"El tamaño de página debe estar entre 1 y {MaxPageSize}.");

        RuleFor(request => request.Operations)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("La lista de operaciones es obligatoria.")
            .Must(operations => operations.Count <= MaxOperations)
            .WithMessage($"El lote no puede superar {MaxOperations} operaciones.");

        RuleForEach(request => request.Operations)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("La operación es obligatoria.")
            .SetValidator(new SyncOperationRequestValidator());
    }
}
