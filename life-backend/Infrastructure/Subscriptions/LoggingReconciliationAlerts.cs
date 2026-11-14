using Life.Application.Subscriptions;
using Microsoft.Extensions.Logging;

namespace Life.Infrastructure.Subscriptions;

public sealed class LoggingReconciliationAlerts(ILogger<LoggingReconciliationAlerts> logger)
    : IReconciliationAlerts
{
    public Task RaiseAsync(
        ReconciliationDiscrepancy discrepancy,
        CancellationToken cancellationToken = default)
    {
        logger.LogError(
            "Unresolved subscription discrepancy for {Provider} subscription {SubscriptionId}: {Reason}",
            discrepancy.Provider,
            discrepancy.SubscriptionId,
            discrepancy.Reason);

        return Task.CompletedTask;
    }
}
