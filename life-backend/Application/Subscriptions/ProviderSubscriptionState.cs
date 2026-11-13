namespace Application.Subscriptions;

public sealed record ProviderSubscriptionState(SubscriptionStatus Status, DateTimeOffset? ExpiresAt);
