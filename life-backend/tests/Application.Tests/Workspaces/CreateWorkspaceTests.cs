using Life.Application.Common;
using Life.Application.Tests.Support;
using Life.Application.Workspaces;
using Life.Domain.Workspaces;
using NSubstitute;

namespace Life.Application.Tests.Workspaces;

public class CreateWorkspaceTests
{
    private const int _maxOwnedWorkspaces = 3;

    private static readonly Guid _ownerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly IWorkspaceRepository _workspaces = Substitute.For<IWorkspaceRepository>();

    private CreateWorkspace CreateSut(int ownedWorkspaces = 0, IUnitOfWork? unitOfWork = null)
    {
        _workspaces.CountOwnedAsync(_ownerId, Arg.Any<CancellationToken>()).Returns(ownedWorkspaces);

        return new CreateWorkspace(
            _workspaces,
            unitOfWork ?? new PassThroughUnitOfWork(),
            new WorkspaceLimitOptions(_maxOwnedWorkspaces));
    }

    private Workspace CaptureStoredWorkspace()
    {
        return (Workspace)_workspaces.ReceivedCalls()
            .Single(call => call.GetMethodInfo().Name == nameof(IWorkspaceRepository.Add))
            .GetArguments()[0]!;
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_StoresTheWorkspaceOwnedByTheCreator()
    {
        var sut = CreateSut();

        await sut.ExecuteAsync(
            _ownerId,
            new CreateWorkspaceRequest("  Home  ", WorkspaceType.Household, "  Shared tasks  "));

        var stored = CaptureStoredWorkspace();
        Assert.Equal(
            ("Home", WorkspaceType.Household, "Shared tasks", _ownerId),
            (stored.Name, stored.Type, stored.Description, stored.OwnerId));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_AddsTheCreatorAsTheOnlyMemberWithTheOwnerRole()
    {
        var sut = CreateSut();

        await sut.ExecuteAsync(_ownerId, new CreateWorkspaceRequest("Home", WorkspaceType.Personal, null));

        var members = CaptureStoredWorkspace().Members;
        Assert.Equal(
            [(_ownerId, WorkspaceRole.Owner)],
            members.Select(member => (member.UserId, member.Role)));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ReturnsTheIdOfTheStoredWorkspace()
    {
        var generatedId = Guid.NewGuid();
        _workspaces.When(repository => repository.Add(Arg.Any<Workspace>()))
            .Do(call => call.Arg<Workspace>().Id = generatedId);
        var sut = CreateSut();

        var response = await sut.ExecuteAsync(
            _ownerId,
            new CreateWorkspaceRequest("Home", WorkspaceType.Personal, null));

        Assert.Equal(generatedId, response?.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithBlankDescription_StoresNoDescription(string? description)
    {
        var sut = CreateSut();

        await sut.ExecuteAsync(_ownerId, new CreateWorkspaceRequest("Home", WorkspaceType.Other, description));

        Assert.Null(CaptureStoredWorkspace().Description);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTheCreatorIsOneBelowTheLimit_StoresTheWorkspace()
    {
        var sut = CreateSut(_maxOwnedWorkspaces - 1);

        var response = await sut.ExecuteAsync(
            _ownerId,
            new CreateWorkspaceRequest("Home", WorkspaceType.Personal, null));

        Assert.NotNull(response);
    }

    [Theory]
    [InlineData(_maxOwnedWorkspaces)]
    [InlineData(_maxOwnedWorkspaces + 1)]
    public async Task ExecuteAsync_WhenTheCreatorReachedTheLimit_ReturnsNullAndStoresNothing(int owned)
    {
        var sut = CreateSut(owned);

        var response = await sut.ExecuteAsync(
            _ownerId,
            new CreateWorkspaceRequest("Home", WorkspaceType.Personal, null));

        Assert.Null(response);
        _workspaces.DidNotReceive().Add(Arg.Any<Workspace>());
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ChecksTheLimitAndStoresInASingleTransaction()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.ExecuteInTransactionAsync(
                Arg.Any<Func<CancellationToken, Task<bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Func<CancellationToken, Task<bool>>>()(CancellationToken.None));
        var sut = CreateSut(unitOfWork: unitOfWork);

        await sut.ExecuteAsync(_ownerId, new CreateWorkspaceRequest("Home", WorkspaceType.Personal, null));

        await unitOfWork.Received(1).ExecuteInTransactionAsync(
            Arg.Any<Func<CancellationToken, Task<bool>>>(),
            Arg.Any<CancellationToken>());
    }
}
