using Application.Common;
using Microsoft.Extensions.Logging;

namespace Application.Subscriptions;

public sealed class ReconcileSubscriptions(
    IEnumerable<ISubscriptionProviderClient> providers,
    ISubscriptionReconciliationStore store,
    IReconciliationLock reconciliationLock,
    IReconciliationAlerts alerts,
    IUnitOfWork unitOfWork,
    SubscriptionReconciliationOptions options,
    TimeProvider timeProvider,
    ILogger<ReconcileSubscriptions> logger)
{
    public async Task<ReconciliationSummary> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await using var handle = await reconciliationLock.TryAcquireAsync(cancellationToken);

        if (handle is null)
        {
            logger.LogInformation("Subscription reconciliation skipped: another run is in progress.");

            return ReconciliationSummary.Empty;
        }

        var tally = new Tally();

        foreach (var provider in providers)
        {
            await ReconcileProviderAsync(provider, tally, cancellationToken);
        }

        return new ReconciliationSummary(tally.Checked, tally.Corrected, tally.Unresolved, tally.Deferred);
    }

    private async Task ReconcileProviderAsync(
        ISubscriptionProviderClient provider,
        Tally tally,
        CancellationToken cancellationToken)
    {
        Guid? afterId = null;

        while (true)
        {
            var batch = await store.ListAsync(
                provider.Provider,
                afterId,
                options.BatchSize,
                cancellationToken);

            foreach (var local in batch)
            {
                await ReconcileOneAsync(provider, local, tally, cancellationToken);
            }

            if (batch.Count < options.BatchSize)
            {
                return;
            }

            afterId = batch[^1].Id;
        }
    }

    private async Task ReconcileOneAsync(
        ISubscriptionProviderClient provider,
        LocalSubscription local,
        Tally tally,
        CancellationToken cancellationToken)
    {
        tally.Checked++;

        ProviderSubscriptionState? remote;

        try
        {
            remote = await provider.GetStateAsync(local.ExternalId, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(
                exception,
                "Could not query {Provider} for subscription {SubscriptionId}; it will be retried.",
                local.Provider,
                local.Id);
            tally.Deferred++;

            return;
        }

        if (remote is null)
        {
            tally.Unresolved++;
            await alerts.RaiseAsync(
                new ReconciliationDiscrepancy(
                    local.Id,
                    local.Provider,
                    local.ExternalId,
                    "The provider does not recognize the subscription."),
                cancellationToken);

            return;
        }

        if (remote.Status == local.Status && remote.ExpiresAt == local.ExpiresAt)
        {
            return;
        }

        await CorrectAsync(local, remote, tally, cancellationToken);
    }

    private async Task CorrectAsync(
        LocalSubscription local,
        ProviderSubscriptionState remote,
        Tally tally,
        CancellationToken cancellationToken)
    {
        var entry = new ReconciliationEntry(
            local.Id,
            local.Provider,
            local.Status,
            local.ExpiresAt,
            remote.Status,
            remote.ExpiresAt,
            timeProvider.GetUtcNow());

        try
        {
            await unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    await store.ApplyAsync(local.Id, remote, token);
                    await store.RecordAsync(entry, token);
                },
                cancellationToken);
            tally.Corrected++;
        }
        catch (ConcurrencyConflictException)
        {
            tally.Deferred++;
        }
    }

    private sealed class Tally
    {
        public int Checked { get; set; }

        public int Corrected { get; set; }

        public int Unresolved { get; set; }

        public int Deferred { get; set; }
    }
}
