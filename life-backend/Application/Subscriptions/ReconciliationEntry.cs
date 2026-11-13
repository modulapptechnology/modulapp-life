namespace Application.Subscriptions;

public sealed record ReconciliationEntry(
    Guid SubscriptionId,
    SubscriptionProvider Provider,
    SubscriptionStatus PreviousStatus,
    DateTimeOffset? PreviousExpiresAt,
    SubscriptionStatus CurrentStatus,
    DateTimeOffset? CurrentExpiresAt,
    DateTimeOffset ReconciledAt);
