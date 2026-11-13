namespace Application.Subscriptions;

public interface IReconciliationAlerts
{
    Task RaiseAsync(
        ReconciliationDiscrepancy discrepancy,
        CancellationToken cancellationToken = default);
}
