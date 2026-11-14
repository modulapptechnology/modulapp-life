using Life.Application.Common;
using Life.Application.Subscriptions;

namespace Life.Application.Tests.Subscriptions;

public sealed class InMemorySubscriptionStore(IEnumerable<LocalSubscription> subscriptions)
    : ISubscriptionReconciliationStore
{
    private readonly List<LocalSubscription> _subscriptions = [.. subscriptions];

    public List<ReconciliationEntry> Entries { get; } = [];

    public bool FailOnApply { get; set; }

    public IReadOnlyList<LocalSubscription> Subscriptions => _subscriptions;

    public Task<IReadOnlyList<LocalSubscription>> ListAsync(
        SubscriptionProvider provider,
        Guid? afterId,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<LocalSubscription> page = _subscriptions
            .Where(candidate => candidate.Provider == provider)
            .Where(candidate => afterId is null || candidate.Id.CompareTo(afterId.Value) > 0)
            .OrderBy(candidate => candidate.Id)
            .Take(batchSize)
            .ToList();

        return Task.FromResult(page);
    }

    public Task ApplyAsync(
        Guid subscriptionId,
        ProviderSubscriptionState state,
        CancellationToken cancellationToken = default)
    {
        if (FailOnApply)
        {
            throw new ConcurrencyConflictException(new InvalidOperationException());
        }

        var index = _subscriptions.FindIndex(candidate => candidate.Id == subscriptionId);
        _subscriptions[index] = _subscriptions[index] with
        {
            Status = state.Status,
            ExpiresAt = state.ExpiresAt,
        };

        return Task.CompletedTask;
    }

    public Task RecordAsync(ReconciliationEntry entry, CancellationToken cancellationToken = default)
    {
        Entries.Add(entry);

        return Task.CompletedTask;
    }
}
