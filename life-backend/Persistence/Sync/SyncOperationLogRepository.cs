using Life.Application.Sync;
using Life.Domain.Sync;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Life.Persistence.Sync;

public sealed class SyncOperationLogRepository(LifeDbContext context) : ISyncOperationLog
{
    private const int _duplicateKeyIndexError = 2601;

    private const int _duplicateKeyConstraintError = 2627;

    public Task<SyncOperationLog?> FindAsync(
        Guid workspaceId,
        Guid userId,
        Guid operationId,
        CancellationToken cancellationToken = default)
    {
        return context.SyncOperationLogs
            .AsNoTracking()
            .SingleOrDefaultAsync(
                entry => entry.WorkspaceId == workspaceId
                    && entry.UserId == userId
                    && entry.OperationId == operationId,
                cancellationToken);
    }

    public async Task AddAsync(SyncOperationLog entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        context.SyncOperationLogs.Add(entry);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateKey(exception))
        {
            context.ChangeTracker.Clear();

            throw new DuplicateSyncOperationException(exception);
        }
    }

    private static bool IsDuplicateKey(DbUpdateException exception)
    {
        return exception.InnerException is SqlException
        {
            Number: _duplicateKeyIndexError or _duplicateKeyConstraintError,
        };
    }
}
