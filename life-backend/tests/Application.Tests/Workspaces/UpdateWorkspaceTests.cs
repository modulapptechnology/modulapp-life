using Life.Application.Common;
using Life.Application.Tests.Support;
using Life.Application.Workspaces;
using Life.Domain.Workspaces;
using NSubstitute;

namespace Life.Application.Tests.Workspaces;

public class UpdateWorkspaceTests
{
    private static readonly Guid _userId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid _workspaceId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly byte[] _version = [1, 2, 3, 4, 5, 6, 7, 8];

    private readonly IWorkspaceRepository _workspaces = Substitute.For<IWorkspaceRepository>();

    private static Workspace NewWorkspace(WorkspaceStatus status = WorkspaceStatus.Active)
    {
        return new Workspace
        {
            Id = _workspaceId,
            Name = "Home",
            Description = "Old",
            Status = status,
            RowVersion = _version,
        };
    }

    private static WorkspaceSummaryResponse NewSummary()
    {
        return new WorkspaceSummaryResponse(
            _workspaceId,
            "Casa",
            null,
            WorkspaceType.Personal,
            WorkspaceStatus.Active,
            WorkspaceRole.Admin,
            [8, 7, 6, 5, 4, 3, 2, 1]);
    }

    private UpdateWorkspace CreateSut(Workspace? stored, IUnitOfWork? unitOfWork = null)
    {
        _workspaces.FindAsync(_workspaceId, Arg.Any<CancellationToken>()).Returns(stored);
        _workspaces.FindByMemberAsync(_workspaceId, _userId, Arg.Any<CancellationToken>()).Returns(NewSummary());

        return new UpdateWorkspace(_workspaces, unitOfWork ?? new PassThroughUnitOfWork());
    }

    [Fact]
    public async Task ExecuteAsync_WithTheCurrentVersion_UpdatesTheNameAndTheDescriptionWithoutSurroundingSpaces()
    {
        var stored = NewWorkspace();
        var sut = CreateSut(stored);

        var result = await sut.ExecuteAsync(
            _userId,
            _workspaceId,
            new UpdateWorkspaceRequest("  Casa  ", "  Tareas  ", _version));

        Assert.Equal(
            (WorkspaceChangeOutcome.Succeeded, "Casa", "Tareas"),
            (result.Outcome, stored.Name, stored.Description));
    }

    [Fact]
    public async Task ExecuteAsync_WithTheCurrentVersion_ReturnsTheUpdatedWorkspace()
    {
        var sut = CreateSut(NewWorkspace());

        var result = await sut.ExecuteAsync(
            _userId,
            _workspaceId,
            new UpdateWorkspaceRequest("Casa", null, _version));

        Assert.Equal(NewSummary().RowVersion, result.Workspace?.RowVersion);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithBlankDescription_StoresNoDescription(string? description)
    {
        var stored = NewWorkspace();
        var sut = CreateSut(stored);

        await sut.ExecuteAsync(_userId, _workspaceId, new UpdateWorkspaceRequest("Casa", description, _version));

        Assert.Null(stored.Description);
    }

    [Fact]
    public async Task ExecuteAsync_WithAStaleVersion_ReturnsVersionConflictAndKeepsTheData()
    {
        var stored = NewWorkspace();
        var sut = CreateSut(stored);

        var result = await sut.ExecuteAsync(
            _userId,
            _workspaceId,
            new UpdateWorkspaceRequest("Casa", "Tareas", [9, 9, 9, 9, 9, 9, 9, 9]));

        Assert.Equal(
            (WorkspaceChangeOutcome.VersionConflict, "Home", "Old"),
            (result.Outcome, stored.Name, stored.Description));
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheSaveDetectsAConcurrentChange_ReturnsVersionConflict()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.ExecuteInTransactionAsync(
                Arg.Any<Func<CancellationToken, Task<WorkspaceChangeOutcome>>>(),
                Arg.Any<CancellationToken>())
            .Returns<Task<WorkspaceChangeOutcome>>(_ => throw new ConcurrencyConflictException(new Exception()));
        var sut = CreateSut(NewWorkspace(), unitOfWork);

        var result = await sut.ExecuteAsync(
            _userId,
            _workspaceId,
            new UpdateWorkspaceRequest("Casa", null, _version));

        Assert.Equal(WorkspaceChangeOutcome.VersionConflict, result.Outcome);
    }

    [Fact]
    public async Task ExecuteAsync_WithAnArchivedWorkspace_ReturnsArchivedAndKeepsTheData()
    {
        var stored = NewWorkspace(WorkspaceStatus.Archived);
        var sut = CreateSut(stored);

        var result = await sut.ExecuteAsync(
            _userId,
            _workspaceId,
            new UpdateWorkspaceRequest("Casa", null, _version));

        Assert.Equal((WorkspaceChangeOutcome.Archived, "Home"), (result.Outcome, stored.Name));
    }

    [Fact]
    public async Task ExecuteAsync_WithAnUnknownWorkspace_ReturnsNotFound()
    {
        var sut = CreateSut(stored: null);

        var result = await sut.ExecuteAsync(
            _userId,
            _workspaceId,
            new UpdateWorkspaceRequest("Casa", null, _version));

        Assert.Equal((WorkspaceChangeOutcome.NotFound, null), (result.Outcome, result.Workspace));
    }
}
