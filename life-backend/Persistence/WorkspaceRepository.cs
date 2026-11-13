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

    public Task<Workspace?> FindAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return context.Workspaces.SingleOrDefaultAsync(workspace => workspace.Id == workspaceId, cancellationToken);
    }

    public Task<WorkspaceSummaryResponse?> FindByMemberAsync(
        Guid workspaceId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return ProjectMemberWorkspaces(userId)
            .Where(workspace => workspace.Id == workspaceId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<WorkspaceSummaryResponse>> ListByMemberAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await ProjectMemberWorkspaces(userId)
            .OrderBy(workspace => workspace.Name)
            .ToListAsync(cancellationToken);
    }

    public void Add(Workspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        context.Workspaces.Add(workspace);
    }

    private IQueryable<WorkspaceSummaryResponse> ProjectMemberWorkspaces(Guid userId)
    {
        return context.Workspaces
            .AsNoTracking()
            .Where(workspace => workspace.Members.Any(member => member.UserId == userId))
            .Select(workspace => new WorkspaceSummaryResponse(
                workspace.Id,
                workspace.Name,
                workspace.Description,
                workspace.Type,
                workspace.Status,
                workspace.Members.First(member => member.UserId == userId).Role,
                workspace.RowVersion));
    }
}
