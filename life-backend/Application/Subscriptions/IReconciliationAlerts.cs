namespace Life.Application.Subscriptions;

public interface IReconciliationAlerts
{
    Task RaiseAsync(
        ReconciliationDiscrepancy discrepancy,
        CancellationToken cancellationToken = default);
}
