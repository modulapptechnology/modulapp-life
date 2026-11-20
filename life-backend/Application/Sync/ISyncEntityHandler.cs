namespace Life.Application.Sync;

public interface ISyncEntityHandler
{
    string EntityType { get; }

    Task<SyncApplyResult> ApplyAsync(
        SyncContext context,
        SyncOperationRequest operation,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SyncChange>> ListChangesAsync(
        SyncContext context,
        byte[] after,
        int limit,
        CancellationToken cancellationToken = default);
}
