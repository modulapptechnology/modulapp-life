namespace Application.Subscriptions;

public interface IReconciliationLock
{
    Task<IAsyncDisposable?> TryAcquireAsync(CancellationToken cancellationToken = default);
}
