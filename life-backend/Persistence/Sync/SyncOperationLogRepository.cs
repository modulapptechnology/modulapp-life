using Life.Application.Sync;
using Life.Domain.Sync;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Life.Persistence.Sync;

public sealed class SyncOperationLogRepository(LifeDbContext context) : ISyncOperationLog
{
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
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
        };
    }
}
