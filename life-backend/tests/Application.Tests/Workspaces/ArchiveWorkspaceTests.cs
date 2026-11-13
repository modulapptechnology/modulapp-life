using Application.Common;
using Application.Tests.Support;
using Application.Workspaces;
using Domain.Workspaces;
using NSubstitute;

namespace Application.Tests.Workspaces;

public class ArchiveWorkspaceTests
{
    private static readonly Guid _workspaceId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly IWorkspaceRepository _workspaces = Substitute.For<IWorkspaceRepository>();

    private ArchiveWorkspace CreateSut(Workspace? stored, IUnitOfWork? unitOfWork = null)
    {
        _workspaces.FindAsync(_workspaceId, Arg.Any<CancellationToken>()).Returns(stored);

        return new ArchiveWorkspace(_workspaces, unitOfWork ?? new PassThroughUnitOfWork());
    }

    [Fact]
    public async Task ExecuteAsync_WithAnActiveWorkspace_MarksItAsArchived()
    {
        var stored = new Workspace { Id = _workspaceId, Status = WorkspaceStatus.Active };
        var sut = CreateSut(stored);

        var outcome = await sut.ExecuteAsync(_workspaceId);

        Assert.Equal(
            (WorkspaceChangeOutcome.Succeeded, WorkspaceStatus.Archived),
            (outcome, stored.Status));
    }

    [Fact]
    public async Task ExecuteAsync_WithAnArchivedWorkspace_KeepsItArchivedAndSucceeds()
    {
        var stored = new Workspace { Id = _workspaceId, Status = WorkspaceStatus.Archived };
        var sut = CreateSut(stored);

        var outcome = await sut.ExecuteAsync(_workspaceId);

        Assert.Equal(
            (WorkspaceChangeOutcome.Succeeded, WorkspaceStatus.Archived),
            (outcome, stored.Status));
    }

    [Fact]
    public async Task ExecuteAsync_WithAnUnknownWorkspace_ReturnsNotFound()
    {
        var sut = CreateSut(stored: null);

        var outcome = await sut.ExecuteAsync(_workspaceId);

        Assert.Equal(WorkspaceChangeOutcome.NotFound, outcome);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheSaveDetectsAConcurrentChange_ReturnsVersionConflict()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.ExecuteInTransactionAsync(
                Arg.Any<Func<CancellationToken, Task<WorkspaceChangeOutcome>>>(),
                Arg.Any<CancellationToken>())
            .Returns<Task<WorkspaceChangeOutcome>>(_ => throw new ConcurrencyConflictException(new Exception()));
        var sut = CreateSut(new Workspace { Id = _workspaceId }, unitOfWork);

        var outcome = await sut.ExecuteAsync(_workspaceId);

        Assert.Equal(WorkspaceChangeOutcome.VersionConflict, outcome);
    }
}
