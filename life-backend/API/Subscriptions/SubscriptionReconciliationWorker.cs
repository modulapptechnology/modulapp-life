using Life.Application.Subscriptions;

namespace Life.API.Subscriptions;

public sealed class SubscriptionReconciliationWorker(
    IServiceScopeFactory scopeFactory,
    SubscriptionReconciliationOptions options,
    TimeProvider timeProvider,
    ILogger<SubscriptionReconciliationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enabled)
        {
            logger.LogInformation("Subscription reconciliation is disabled.");

            return;
        }

        using var timer = new PeriodicTimer(options.Interval, timeProvider);

        try
        {
            do
            {
                await RunOnceAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    public async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var reconcile = scope.ServiceProvider.GetRequiredService<ReconcileSubscriptions>();
            var summary = await reconcile.ExecuteAsync(cancellationToken);

            logger.LogInformation(
                "Subscription reconciliation finished: {Checked} checked, {Corrected} corrected, "
                    + "{Unresolved} unresolved, {Deferred} deferred.",
                summary.Checked,
                summary.Corrected,
                summary.Unresolved,
                summary.Deferred);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Subscription reconciliation failed; it will run again on schedule.");
        }
    }
}
