using Application.Subscriptions;
using Application.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Application.Tests.Subscriptions;

public class ReconcileSubscriptionsTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 13, 12, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset _expiry = new(2026, 12, 13, 12, 0, 0, TimeSpan.Zero);

    private readonly IReconciliationAlerts _alerts = Substitute.For<IReconciliationAlerts>();

    private readonly IReconciliationLock _lock = Substitute.For<IReconciliationLock>();

    public ReconcileSubscriptionsTests()
    {
        _lock.TryAcquireAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IAsyncDisposable?>(Substitute.For<IAsyncDisposable>()));
    }

    private static LocalSubscription NewLocal(
        int number,
        SubscriptionStatus status,
        SubscriptionProvider provider = SubscriptionProvider.Apple)
    {
        return new LocalSubscription(
            Guid.Parse($"00000000-0000-0000-0000-{number:D12}"),
            provider,
            $"external-{number}",
            status,
            _expiry);
    }

    private static ISubscriptionProviderClient NewClient(
        SubscriptionProvider provider,
        Func<string, ProviderSubscriptionState?> answer)
    {
        var client = Substitute.For<ISubscriptionProviderClient>();
        client.Provider.Returns(provider);
        client.GetStateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(answer(call.Arg<string>())));

        return client;
    }

    private ReconcileSubscriptions CreateSut(
        InMemorySubscriptionStore store,
        IReadOnlyList<ISubscriptionProviderClient> clients,
        int batchSize = 100)
    {
        return new ReconcileSubscriptions(
            clients,
            store,
            _lock,
            _alerts,
            new PassThroughUnitOfWork(),
            new SubscriptionReconciliationOptions(true, TimeSpan.FromMinutes(60), batchSize),
            new FixedTimeProvider(_now),
            NullLogger<ReconcileSubscriptions>.Instance);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheProviderDiffers_CorrectsTheLocalStateAndRecordsIt()
    {
        var store = new InMemorySubscriptionStore([NewLocal(1, SubscriptionStatus.Active)]);
        var client = NewClient(
            SubscriptionProvider.Apple,
            _ => new ProviderSubscriptionState(SubscriptionStatus.Canceled, _expiry));
        var sut = CreateSut(store, [client]);

        var summary = await sut.ExecuteAsync();

        Assert.Equal(
            (
                new ReconciliationSummary(1, 1, 0, 0),
                SubscriptionStatus.Canceled,
                SubscriptionStatus.Active,
                _now),
            (
                summary,
                store.Subscriptions.Single().Status,
                store.Entries.Single().PreviousStatus,
                store.Entries.Single().ReconciledAt));
    }

    [Fact]
    public async Task ExecuteAsync_WhenOnlyTheExpirationDiffers_CorrectsTheExpiration()
    {
        var store = new InMemorySubscriptionStore([NewLocal(1, SubscriptionStatus.Active)]);
        var renewed = _expiry.AddMonths(1);
        var client = NewClient(
            SubscriptionProvider.Apple,
            _ => new ProviderSubscriptionState(SubscriptionStatus.Active, renewed));
        var sut = CreateSut(store, [client]);

        await sut.ExecuteAsync();

        Assert.Equal(renewed, store.Subscriptions.Single().ExpiresAt);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheStateAlreadyMatches_ChangesAndRecordsNothing()
    {
        var store = new InMemorySubscriptionStore([NewLocal(1, SubscriptionStatus.Active)]);
        var client = NewClient(
            SubscriptionProvider.Apple,
            _ => new ProviderSubscriptionState(SubscriptionStatus.Active, _expiry));
        var sut = CreateSut(store, [client]);

        var summary = await sut.ExecuteAsync();

        Assert.Equal((new ReconciliationSummary(1, 0, 0, 0), 0), (summary, store.Entries.Count));
    }

    [Fact]
    public async Task ExecuteAsync_WhenRunTwiceInARow_ProducesTheSameResultTheSecondTime()
    {
        var store = new InMemorySubscriptionStore([NewLocal(1, SubscriptionStatus.Active)]);
        var client = NewClient(
            SubscriptionProvider.Apple,
            _ => new ProviderSubscriptionState(SubscriptionStatus.Expired, _expiry));
        var sut = CreateSut(store, [client]);
        await sut.ExecuteAsync();
        var stateAfterFirstRun = store.Subscriptions.Single();

        var secondSummary = await sut.ExecuteAsync();

        Assert.Equal(
            (new ReconciliationSummary(1, 0, 0, 0), stateAfterFirstRun, 1),
            (secondSummary, store.Subscriptions.Single(), store.Entries.Count));
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheProviderDoesNotKnowTheSubscription_RaisesAnAlertAndKeepsTheState()
    {
        var local = NewLocal(1, SubscriptionStatus.Active);
        var store = new InMemorySubscriptionStore([local]);
        var client = NewClient(SubscriptionProvider.Apple, _ => null);
        var sut = CreateSut(store, [client]);

        var summary = await sut.ExecuteAsync();

        await _alerts.Received(1).RaiseAsync(
            Arg.Is<ReconciliationDiscrepancy>(alert => alert.SubscriptionId == local.Id),
            Arg.Any<CancellationToken>());
        Assert.Equal((new ReconciliationSummary(1, 0, 1, 0), local), (summary, store.Subscriptions.Single()));
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheProviderFailsForOneSubscription_DefersItAndContinuesWithTheRest()
    {
        var store = new InMemorySubscriptionStore(
            [NewLocal(1, SubscriptionStatus.Active), NewLocal(2, SubscriptionStatus.Active)]);
        var client = NewClient(
            SubscriptionProvider.Apple,
            externalId => externalId == "external-1"
                ? throw new HttpRequestException()
                : new ProviderSubscriptionState(SubscriptionStatus.Canceled, _expiry));
        var sut = CreateSut(store, [client]);

        var summary = await sut.ExecuteAsync();

        await _alerts.DidNotReceiveWithAnyArgs().RaiseAsync(default!, default);
        Assert.Equal(new ReconciliationSummary(2, 1, 0, 1), summary);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheSaveDetectsAConcurrentChange_DefersTheSubscription()
    {
        var store = new InMemorySubscriptionStore([NewLocal(1, SubscriptionStatus.Active)])
        {
            FailOnApply = true,
        };
        var client = NewClient(
            SubscriptionProvider.Apple,
            _ => new ProviderSubscriptionState(SubscriptionStatus.Canceled, _expiry));
        var sut = CreateSut(store, [client]);

        var summary = await sut.ExecuteAsync();

        Assert.Equal((new ReconciliationSummary(1, 0, 0, 1), 0), (summary, store.Entries.Count));
    }

    [Fact]
    public async Task ExecuteAsync_WhenAnotherRunHoldsTheLock_DoesNothing()
    {
        _lock.TryAcquireAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IAsyncDisposable?>(null));
        var client = NewClient(SubscriptionProvider.Apple, _ => null);
        var store = new InMemorySubscriptionStore([NewLocal(1, SubscriptionStatus.Active)]);
        var sut = CreateSut(store, [client]);

        var summary = await sut.ExecuteAsync();

        await client.DidNotReceiveWithAnyArgs().GetStateAsync(default!, default);
        Assert.Equal(ReconciliationSummary.Empty, summary);
    }

    [Fact]
    public async Task ExecuteAsync_WithMoreSubscriptionsThanTheBatchSize_ChecksAllOfThem()
    {
        var store = new InMemorySubscriptionStore(
            Enumerable.Range(1, 5).Select(number => NewLocal(number, SubscriptionStatus.Active)));
        var client = NewClient(
            SubscriptionProvider.Apple,
            _ => new ProviderSubscriptionState(SubscriptionStatus.Canceled, _expiry));
        var sut = CreateSut(store, [client], batchSize: 2);

        var summary = await sut.ExecuteAsync();

        Assert.Equal(new ReconciliationSummary(5, 5, 0, 0), summary);
    }

    [Fact]
    public async Task ExecuteAsync_WithSeveralProviders_ReconcilesEachAgainstItsOwnProvider()
    {
        var store = new InMemorySubscriptionStore(
        [
            NewLocal(1, SubscriptionStatus.Active, SubscriptionProvider.Apple),
            NewLocal(2, SubscriptionStatus.Active, SubscriptionProvider.Google),
        ]);
        var apple = NewClient(
            SubscriptionProvider.Apple,
            _ => new ProviderSubscriptionState(SubscriptionStatus.Canceled, _expiry));
        var google = NewClient(
            SubscriptionProvider.Google,
            _ => new ProviderSubscriptionState(SubscriptionStatus.Active, _expiry));
        var sut = CreateSut(store, [apple, google]);

        var summary = await sut.ExecuteAsync();

        Assert.Equal(
            (new ReconciliationSummary(2, 1, 0, 0), SubscriptionStatus.Canceled, SubscriptionStatus.Active),
            (summary, store.Subscriptions[0].Status, store.Subscriptions[1].Status));
    }
}
