using Life.Application.Workspaces;
using Life.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace Life.Persistence;

public sealed class WorkspaceMembershipReader(LifeDbContext context) : IWorkspaceMembershipReader
{
    public async Task<WorkspaceRole?> FindRoleAsync(
        Guid workspaceId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var roles = await context.WorkspaceMembers
            .AsNoTracking()
            .Where(member => member.WorkspaceId == workspaceId && member.UserId == userId)
            .Select(member => (WorkspaceRole?)member.Role)
            .ToListAsync(cancellationToken);

        return roles.SingleOrDefault();
    }

    public async Task<IReadOnlyList<WorkspaceMemberSummary>> ListMembersAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        return await context.WorkspaceMembers
            .AsNoTracking()
            .Where(member => member.WorkspaceId == workspaceId)
            .OrderBy(member => member.User!.Name)
            .Select(member => new WorkspaceMemberSummary(
                member.UserId,
                member.User!.Name,
                member.User.AvatarUrl,
                member.Role))
            .ToListAsync(cancellationToken);
    }
}
