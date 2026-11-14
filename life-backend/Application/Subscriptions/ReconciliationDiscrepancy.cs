namespace Life.Application.Subscriptions;

public sealed record ReconciliationDiscrepancy(
    Guid SubscriptionId,
    SubscriptionProvider Provider,
    string ExternalId,
    string Reason);
