using Life.Application.Sync;
using Life.Domain.Sync;

namespace Life.Application.Tests.Support;

public sealed class FakeSyncOperationLog : ISyncOperationLog
{
    private readonly Dictionary<(Guid WorkspaceId, Guid UserId, Guid OperationId), SyncOperationLog> _entries = [];

    public int Count => _entries.Count;

    public Task<SyncOperationLog?> FindAsync(
        Guid workspaceId,
        Guid userId,
        Guid operationId,
        CancellationToken cancellationToken = default)
    {
        _entries.TryGetValue((workspaceId, userId, operationId), out var entry);

        return Task.FromResult(entry);
    }

    public Task AddAsync(SyncOperationLog entry, CancellationToken cancellationToken = default)
    {
        var key = (entry.WorkspaceId, entry.UserId, entry.OperationId);

        if (!_entries.TryAdd(key, entry))
        {
            throw new DuplicateSyncOperationException(new InvalidOperationException("Duplicate key."));
        }

        return Task.CompletedTask;
    }
}
