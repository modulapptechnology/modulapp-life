using System.Text.Json;
using Life.Application.Common;
using Life.Application.Sync;
using Life.Application.Tests.Support;
using Life.Domain.Sync;
using NSubstitute;

namespace Life.Application.Tests.Sync;

public class SynchronizeChangesTests
{
    private static readonly Guid _userId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid _workspaceId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly byte[] _versionOne = [0, 0, 0, 0, 0, 0, 0, 1];

    private static readonly byte[] _versionTwo = [0, 0, 0, 0, 0, 0, 0, 2];

    private static readonly byte[] _versionThree = [0, 0, 0, 0, 0, 0, 0, 3];

    private readonly FakeSyncOperationLog _log = new();

    private static ISyncEntityHandler NewHandler(string entityType, params SyncChange[] changes)
    {
        var handler = Substitute.For<ISyncEntityHandler>();
        handler.EntityType.Returns(entityType);
        handler.ApplyAsync(Arg.Any<SyncContext>(), Arg.Any<SyncOperationRequest>(), Arg.Any<CancellationToken>())
            .Returns(SyncApplyResult.Applied());
        handler.ListChangesAsync(
                Arg.Any<SyncContext>(),
                Arg.Any<byte[]>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(changes);

        return handler;
    }

    private static SyncChange NewChange(string entityType, byte[] version, bool isDeleted = false)
    {
        return new SyncChange(
            entityType,
            Guid.NewGuid(),
            isDeleted,
            version,
            isDeleted ? null : JsonSerializer.SerializeToElement(new { title = "A" }));
    }

    private static SyncOperationRequest NewOperation(
        string entityType = "task",
        SyncOperationKind kind = SyncOperationKind.Create,
        Guid? operationId = null)
    {
        return new SyncOperationRequest(
            operationId ?? Guid.NewGuid(),
            entityType,
            Guid.NewGuid(),
            kind,
            JsonSerializer.SerializeToElement(new { title = "A" }));
    }

    private SynchronizeChanges CreateSut(IUnitOfWork? unitOfWork = null, params ISyncEntityHandler[] handlers)
    {
        return new SynchronizeChanges(handlers, _log, unitOfWork ?? new PassThroughUnitOfWork());
    }

    [Fact]
    public async Task ExecuteAsync_WithABatch_ConfirmsEachOperationAndStoresItsResult()
    {
        var handler = NewHandler("task");
        var sut = CreateSut(null, handler);
        var first = NewOperation();
        var second = NewOperation(kind: SyncOperationKind.Update);

        var response = await sut.ExecuteAsync(_userId, _workspaceId, new SyncRequest(null, null, [first, second]));

        Assert.Equal(
            [
                (first.OperationId, SyncOperationStatus.Applied),
                (second.OperationId, SyncOperationStatus.Applied),
            ],
            response.Results.Select(result => (result.OperationId, result.Status)));
        Assert.Equal(2, _log.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ResendingTheSameBatch_AppliesEachOperationOnlyOnce()
    {
        var handler = NewHandler("task");
        var sut = CreateSut(null, handler);
        var request = new SyncRequest(null, null, [NewOperation(), NewOperation()]);
        await sut.ExecuteAsync(_userId, _workspaceId, request);

        await sut.ExecuteAsync(_userId, _workspaceId, request);

        await handler.Received(2).ApplyAsync(
            Arg.Any<SyncContext>(),
            Arg.Any<SyncOperationRequest>(),
            Arg.Any<CancellationToken>());
        Assert.Equal(2, _log.Count);
    }

    [Fact]
    public async Task ExecuteAsync_SendingTheSameOperationTwice_ReturnsTheOriginalResultTheSecondTime()
    {
        var handler = NewHandler("task");
        handler.ApplyAsync(Arg.Any<SyncContext>(), Arg.Any<SyncOperationRequest>(), Arg.Any<CancellationToken>())
            .Returns(SyncApplyResult.Rejected("invalid_payload"), SyncApplyResult.Applied());
        var sut = CreateSut(null, handler);
        var request = new SyncRequest(null, null, [NewOperation()]);
        var first = await sut.ExecuteAsync(_userId, _workspaceId, request);

        var second = await sut.ExecuteAsync(_userId, _workspaceId, request);

        Assert.Equal(first.Results, second.Results);
        Assert.Equal("invalid_payload", second.Results[0].ErrorCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithTheSameOperationRepeatedInOneBatch_AppliesItOnce()
    {
        var handler = NewHandler("task");
        var sut = CreateSut(null, handler);
        var operation = NewOperation();

        var response = await sut.ExecuteAsync(
            _userId,
            _workspaceId,
            new SyncRequest(null, null, [operation, operation]));

        await handler.Received(1).ApplyAsync(
            Arg.Any<SyncContext>(),
            Arg.Any<SyncOperationRequest>(),
            Arg.Any<CancellationToken>());
        Assert.Equal(response.Results[0], response.Results[1]);
    }

    [Fact]
    public async Task ExecuteAsync_WithTheSameOperationIdFromAnotherUser_AppliesItAgain()
    {
        var handler = NewHandler("task");
        var sut = CreateSut(null, handler);
        var request = new SyncRequest(null, null, [NewOperation()]);
        await sut.ExecuteAsync(_userId, _workspaceId, request);

        await sut.ExecuteAsync(Guid.NewGuid(), _workspaceId, request);

        Assert.Equal(2, _log.Count);
    }

    [Fact]
    public async Task ExecuteAsync_WithAnUnsupportedEntityType_RejectsTheOperationAndStoresIt()
    {
        var sut = CreateSut(null, NewHandler("task"));
        var operation = NewOperation(entityType: "unknown");

        var response = await sut.ExecuteAsync(_userId, _workspaceId, new SyncRequest(null, null, [operation]));

        var result = Assert.Single(response.Results);
        Assert.Equal(
            (SyncOperationStatus.Rejected, SynchronizeChanges.UnsupportedEntityTypeError, 1),
            (result.Status, result.ErrorCode, _log.Count));
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheHandlerRejects_ReturnsItsErrorCode()
    {
        var handler = NewHandler("task");
        handler.ApplyAsync(Arg.Any<SyncContext>(), Arg.Any<SyncOperationRequest>(), Arg.Any<CancellationToken>())
            .Returns(SyncApplyResult.Rejected("read_only"));
        var sut = CreateSut(null, handler);

        var response = await sut.ExecuteAsync(
            _userId,
            _workspaceId,
            new SyncRequest(null, null, [NewOperation()]));

        var result = Assert.Single(response.Results);
        Assert.Equal((SyncOperationStatus.Rejected, "read_only"), (result.Status, result.ErrorCode));
    }

    [Fact]
    public async Task ExecuteAsync_WhenAConcurrentRequestStoredTheOperationFirst_ReturnsTheStoredResult()
    {
        var operation = NewOperation();
        var stored = new SyncOperationLog
        {
            OperationId = operation.OperationId,
            Status = SyncOperationStatus.Applied,
        };
        var log = Substitute.For<ISyncOperationLog>();
        log.FindAsync(_workspaceId, _userId, operation.OperationId, Arg.Any<CancellationToken>())
            .Returns(null, stored);
        log.AddAsync(Arg.Any<SyncOperationLog>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new DuplicateSyncOperationException(new InvalidOperationException())));
        var sut = new SynchronizeChanges([NewHandler("task")], log, new PassThroughUnitOfWork());

        var response = await sut.ExecuteAsync(_userId, _workspaceId, new SyncRequest(null, null, [operation]));

        var result = Assert.Single(response.Results);
        Assert.Equal((operation.OperationId, SyncOperationStatus.Applied), (result.OperationId, result.Status));
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheRecordWasModifiedMeanwhile_RejectsWithAVersionConflict()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.ExecuteInTransactionAsync(
                Arg.Any<Func<CancellationToken, Task<SyncOperationResult>>>(),
                Arg.Any<CancellationToken>())
            .Returns<Task<SyncOperationResult>>(_ => throw new ConcurrencyConflictException(new Exception()));
        var sut = CreateSut(unitOfWork, NewHandler("task"));

        var response = await sut.ExecuteAsync(
            _userId,
            _workspaceId,
            new SyncRequest(null, null, [NewOperation()]));

        var result = Assert.Single(response.Results);
        Assert.Equal(
            (SyncOperationStatus.Rejected, SynchronizeChanges.VersionConflictError),
            (result.Status, result.ErrorCode));
    }

    [Fact]
    public async Task ExecuteAsync_WithACursor_RequestsOnlyTheChangesAfterIt()
    {
        var handler = NewHandler("task");
        var sut = CreateSut(null, handler);

        await sut.ExecuteAsync(
            _userId,
            _workspaceId,
            new SyncRequest(SyncCursor.Format(_versionTwo), 10, []));

        await handler.Received(1).ListChangesAsync(
            new SyncContext(_workspaceId, _userId),
            Arg.Is<byte[]>(after => after.SequenceEqual(_versionTwo)),
            11,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithChangesIncludingDeletions_ReturnsThemInVersionOrderWithTheNextCursor()
    {
        var tasks = NewHandler("task", NewChange("task", _versionThree, isDeleted: true));
        var lists = NewHandler("list", NewChange("list", _versionOne));
        var sut = CreateSut(null, tasks, lists);

        var response = await sut.ExecuteAsync(_userId, _workspaceId, new SyncRequest(null, null, []));

        Assert.Equal(
            [("list", false), ("task", true)],
            response.Changes.Select(change => (change.EntityType, change.IsDeleted)));
        Assert.Equal(SyncCursor.Format(_versionThree), response.NextCursor);
        Assert.False(response.HasMore);
    }

    [Fact]
    public async Task ExecuteAsync_WithMoreChangesThanThePageSize_ReturnsTheFirstPageAndMarksThereIsMore()
    {
        var tasks = NewHandler("task", NewChange("task", _versionOne), NewChange("task", _versionThree));
        var lists = NewHandler("list", NewChange("list", _versionTwo));
        var sut = CreateSut(null, tasks, lists);

        var response = await sut.ExecuteAsync(_userId, _workspaceId, new SyncRequest(null, 2, []));

        Assert.Equal(
            [_versionOne, _versionTwo],
            response.Changes.Select(change => change.RowVersion));
        Assert.Equal(SyncCursor.Format(_versionTwo), response.NextCursor);
        Assert.True(response.HasMore);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutNewChanges_KeepsTheCursorTheClientSent()
    {
        var sut = CreateSut(null, NewHandler("task"));
        var cursor = SyncCursor.Format(_versionTwo);

        var response = await sut.ExecuteAsync(_userId, _workspaceId, new SyncRequest(cursor, null, []));

        Assert.Equal((cursor, false, 0), (response.NextCursor, response.HasMore, response.Changes.Count));
    }
}
