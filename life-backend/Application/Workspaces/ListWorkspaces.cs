namespace Life.Application.Workspaces;

public sealed class ListWorkspaces(IWorkspaceRepository workspaces)
{
    public Task<IReadOnlyList<WorkspaceSummaryResponse>> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return workspaces.ListByMemberAsync(userId, cancellationToken);
    }
}
