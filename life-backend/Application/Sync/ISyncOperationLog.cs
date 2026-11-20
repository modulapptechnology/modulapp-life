using Life.Domain.Sync;

namespace Life.Application.Sync;

public interface ISyncOperationLog
{
    Task<SyncOperationLog?> FindAsync(
        Guid workspaceId,
        Guid userId,
        Guid operationId,
        CancellationToken cancellationToken = default);

    Task AddAsync(SyncOperationLog entry, CancellationToken cancellationToken = default);
}
