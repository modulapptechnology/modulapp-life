using Application.Workspaces;
using Domain.Workspaces;

namespace API.Tests.Support;

public sealed class InMemoryWorkspaceRepository : IWorkspaceRepository
{
    private readonly List<Workspace> _workspaces = [];

    public IReadOnlyList<Workspace> Workspaces => _workspaces;

    public Task<int> CountOwnedAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_workspaces.Count(workspace => workspace.OwnerId == ownerId));
    }

    public void Add(Workspace workspace)
    {
        workspace.Id = Guid.NewGuid();
        _workspaces.Add(workspace);
    }
}
