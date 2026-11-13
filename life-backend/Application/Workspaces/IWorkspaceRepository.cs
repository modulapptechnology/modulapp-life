using Domain.Workspaces;

namespace Application.Workspaces;

public interface IWorkspaceRepository
{
    Task<int> CountOwnedAsync(Guid ownerId, CancellationToken cancellationToken = default);

    void Add(Workspace workspace);
}
