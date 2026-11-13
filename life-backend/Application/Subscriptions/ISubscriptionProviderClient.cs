namespace Application.Subscriptions;

public interface ISubscriptionProviderClient
{
    SubscriptionProvider Provider { get; }

    Task<ProviderSubscriptionState?> GetStateAsync(
        string externalId,
        CancellationToken cancellationToken = default);
}
