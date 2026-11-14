namespace Life.Application.Subscriptions;

public interface ISubscriptionReconciliationStore
{
    Task<IReadOnlyList<LocalSubscription>> ListAsync(
        SubscriptionProvider provider,
        Guid? afterId,
        int batchSize,
        CancellationToken cancellationToken = default);

    Task ApplyAsync(
        Guid subscriptionId,
        ProviderSubscriptionState state,
        CancellationToken cancellationToken = default);

    Task RecordAsync(ReconciliationEntry entry, CancellationToken cancellationToken = default);
}
