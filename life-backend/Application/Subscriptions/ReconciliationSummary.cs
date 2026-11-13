namespace Application.Subscriptions;

public sealed record ReconciliationSummary(int Checked, int Corrected, int Unresolved, int Deferred)
{
    public static ReconciliationSummary Empty { get; } = new(0, 0, 0, 0);
}
