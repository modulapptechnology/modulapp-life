using Life.Application.Common;
using Life.Domain.Sync;

namespace Life.Application.Sync;

public sealed class SynchronizeChanges(
    IEnumerable<ISyncEntityHandler> handlers,
    ISyncOperationLog operationLog,
    IUnitOfWork unitOfWork)
{
    public const string UnsupportedEntityTypeError = "unsupported_entity_type";

    public const string VersionConflictError = "version_conflict";

    private readonly IReadOnlyList<ISyncEntityHandler> _handlers = handlers.ToList();

    public async Task<SyncResponse> ExecuteAsync(
        Guid userId,
        Guid workspaceId,
        SyncRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var context = new SyncContext(workspaceId, userId);
        var results = new List<SyncOperationResult>(request.Operations.Count);

        foreach (var operation in request.Operations)
        {
            results.Add(await ProcessAsync(context, operation, cancellationToken));
        }

        SyncCursor.TryParse(request.Cursor, out var after);
        var pageSize = request.PageSize ?? SyncRequestValidator.DefaultPageSize;
        var changes = await ListChangesAsync(context, after, pageSize, cancellationToken);
        var hasMore = changes.Count > pageSize;
        var page = changes.Take(pageSize).ToList();
        var nextCursor = page.Count > 0 ? page[^1].RowVersion : after;

        return new SyncResponse(results, page, SyncCursor.Format(nextCursor), hasMore);
    }

    private async Task<SyncOperationResult> ProcessAsync(
        SyncContext context,
        SyncOperationRequest operation,
        CancellationToken cancellationToken)
    {
        try
        {
            return await unitOfWork.ExecuteInTransactionAsync(
                token => ApplyOnceAsync(context, operation, token),
                cancellationToken);
        }
        catch (DuplicateSyncOperationException)
        {
            var original = await operationLog.FindAsync(
                context.WorkspaceId,
                context.UserId,
                operation.OperationId,
                cancellationToken);

            return original is null
                ? new SyncOperationResult(operation.OperationId, SyncOperationStatus.Rejected, VersionConflictError)
                : ToResult(original);
        }
        catch (ConcurrencyConflictException)
        {
            return new SyncOperationResult(operation.OperationId, SyncOperationStatus.Rejected, VersionConflictError);
        }
    }

    private async Task<SyncOperationResult> ApplyOnceAsync(
        SyncContext context,
        SyncOperationRequest operation,
        CancellationToken cancellationToken)
    {
        var existing = await operationLog.FindAsync(
            context.WorkspaceId,
            context.UserId,
            operation.OperationId,
            cancellationToken);

        if (existing is not null)
        {
            return ToResult(existing);
        }

        var handler = _handlers.FirstOrDefault(candidate =>
            string.Equals(candidate.EntityType, operation.EntityType, StringComparison.Ordinal));
        var applied = handler is null
            ? SyncApplyResult.Rejected(UnsupportedEntityTypeError)
            : await handler.ApplyAsync(context, operation, cancellationToken);

        await operationLog.AddAsync(
            new SyncOperationLog
            {
                OperationId = operation.OperationId,
                WorkspaceId = context.WorkspaceId,
                UserId = context.UserId,
                EntityType = operation.EntityType,
                EntityId = operation.EntityId,
                Kind = operation.Kind,
                Status = applied.Status,
                ErrorCode = applied.ErrorCode,
            },
            cancellationToken);

        return new SyncOperationResult(operation.OperationId, applied.Status, applied.ErrorCode);
    }

    private async Task<List<SyncChange>> ListChangesAsync(
        SyncContext context,
        byte[] after,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var changes = new List<SyncChange>();

        foreach (var handler in _handlers)
        {
            changes.AddRange(await handler.ListChangesAsync(context, after, pageSize + 1, cancellationToken));
        }

        return changes.OrderBy(change => change.RowVersion, SyncCursor.Comparer).ToList();
    }

    private static SyncOperationResult ToResult(SyncOperationLog entry)
    {
        return new SyncOperationResult(entry.OperationId, entry.Status, entry.ErrorCode);
    }
}
