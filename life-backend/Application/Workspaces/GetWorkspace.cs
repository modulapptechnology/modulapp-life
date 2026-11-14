namespace Life.Application.Workspaces;

public sealed class GetWorkspace(IWorkspaceRepository workspaces)
{
    public Task<WorkspaceSummaryResponse?> ExecuteAsync(
        Guid userId,
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        return workspaces.FindByMemberAsync(workspaceId, userId, cancellationToken);
    }
}
