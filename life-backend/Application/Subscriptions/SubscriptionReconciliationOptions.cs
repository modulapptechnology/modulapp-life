namespace Life.Application.Subscriptions;

public sealed record SubscriptionReconciliationOptions(bool Enabled, TimeSpan Interval, int BatchSize)
{
    public const string SectionName = "Subscriptions:Reconciliation";

    public const string EnabledKey = "Enabled";

    public const string IntervalMinutesKey = "IntervalMinutes";

    public const string BatchSizeKey = "BatchSize";

    public const int DefaultIntervalMinutes = 60;

    public const int DefaultBatchSize = 100;
}
