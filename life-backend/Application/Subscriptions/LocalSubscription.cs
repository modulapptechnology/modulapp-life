namespace Application.Subscriptions;

public sealed record LocalSubscription(
    Guid Id,
    SubscriptionProvider Provider,
    string ExternalId,
    SubscriptionStatus Status,
    DateTimeOffset? ExpiresAt);
