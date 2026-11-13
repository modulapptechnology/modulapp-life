using Domain.Workspaces;

namespace Application.Workspaces;

public interface IWorkspaceMembershipReader
{
    Task<WorkspaceRole?> FindRoleAsync(
        Guid workspaceId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkspaceMemberSummary>> ListMembersAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);
}
