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

    public Task<Workspace?> FindAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_workspaces.SingleOrDefault(workspace => workspace.Id == workspaceId));
    }

    public Task<WorkspaceSummaryResponse?> FindByMemberAsync(
        Guid workspaceId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var workspace = _workspaces.SingleOrDefault(candidate => candidate.Id == workspaceId);

        return Task.FromResult(workspace is null ? null : Summarize(workspace, userId));
    }

    public Task<IReadOnlyList<WorkspaceSummaryResponse>> ListByMemberAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<WorkspaceSummaryResponse> summaries = _workspaces
            .Where(workspace => workspace.Members.Any(member => member.UserId == userId))
            .Select(workspace => Summarize(workspace, userId)!)
            .OrderBy(summary => summary.Name)
            .ToList();

        return Task.FromResult(summaries);
    }

    public void Add(Workspace workspace)
    {
        workspace.Id = Guid.NewGuid();
        _workspaces.Add(workspace);
    }

    private static WorkspaceSummaryResponse? Summarize(Workspace workspace, Guid userId)
    {
        var member = workspace.Members.FirstOrDefault(candidate => candidate.UserId == userId);

        return member is null
            ? null
            : new WorkspaceSummaryResponse(
                workspace.Id,
                workspace.Name,
                workspace.Description,
                workspace.Type,
                workspace.Status,
                member.Role,
                workspace.RowVersion);
    }
}
