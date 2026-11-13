using Infrastructure.Subscriptions;

namespace Infrastructure.Tests.Subscriptions;

public class InProcessReconciliationLockTests
{
    [Fact]
    public async Task TryAcquireAsync_WhileAnotherHolderHasIt_ReturnsNull()
    {
        var sut = new InProcessReconciliationLock();
        await using var first = await sut.TryAcquireAsync();

        var second = await sut.TryAcquireAsync();

        Assert.Null(second);
    }

    [Fact]
    public async Task TryAcquireAsync_AfterTheHolderReleasesIt_ReturnsANewHandle()
    {
        var sut = new InProcessReconciliationLock();
        var first = await sut.TryAcquireAsync();
        await first!.DisposeAsync();

        var second = await sut.TryAcquireAsync();

        Assert.NotNull(second);
    }

    [Fact]
    public async Task DisposeAsync_CalledTwice_ReleasesTheLockOnlyOnce()
    {
        var sut = new InProcessReconciliationLock();
        var first = await sut.TryAcquireAsync();
        await first!.DisposeAsync();
        var second = await sut.TryAcquireAsync();

        await first.DisposeAsync();

        Assert.Null(await sut.TryAcquireAsync());
        await second!.DisposeAsync();
    }
}
