using Application.Common;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public sealed class UnitOfWork(LifeDbContext context) : IUnitOfWork
{
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var strategy = context.Database.CreateExecutionStrategy();
        var attempt = 0;

        try
        {
            return await strategy.ExecuteAsync(async token =>
            {
                if (attempt++ > 0)
                {
                    context.ChangeTracker.Clear();
                }

                await using var transaction = await context.Database.BeginTransactionAsync(token);
                var result = await operation(token);
                await context.SaveChangesAsync(token);
                await transaction.CommitAsync(token);

                return result;
            }, cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException(exception);
        }
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await ExecuteInTransactionAsync(
            async token =>
            {
                await operation(token);

                return true;
            },
            cancellationToken);
    }
}
