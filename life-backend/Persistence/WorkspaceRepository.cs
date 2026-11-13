using Application.Workspaces;
using Domain.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public sealed class WorkspaceRepository(LifeDbContext context) : IWorkspaceRepository
{
    public Task<int> CountOwnedAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return context.Workspaces.CountAsync(workspace => workspace.OwnerId == ownerId, cancellationToken);
    }

    public void Add(Workspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        context.Workspaces.Add(workspace);
    }
}
