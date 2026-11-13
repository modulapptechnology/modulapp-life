using Application.Workspaces;
using Domain.Workspaces;

namespace API.Tests.Support;

public sealed class InMemoryWorkspaceMembershipReader : IWorkspaceMembershipReader
{
    private readonly Dictionary<(Guid WorkspaceId, Guid UserId), WorkspaceMemberSummary> _members = [];

    public void Add(Guid workspaceId, Guid userId, string name, WorkspaceRole role)
    {
        _members[(workspaceId, userId)] = new WorkspaceMemberSummary(userId, name, null, role);
    }

    public Task<WorkspaceRole?> FindRoleAsync(
        Guid workspaceId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            _members.TryGetValue((workspaceId, userId), out var member) ? (WorkspaceRole?)member.Role : null);
    }

    public Task<IReadOnlyList<WorkspaceMemberSummary>> ListMembersAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<WorkspaceMemberSummary> members = _members
            .Where(entry => entry.Key.WorkspaceId == workspaceId)
            .Select(entry => entry.Value)
            .ToList();

        return Task.FromResult(members);
    }
}
