using Application.Common;
using Domain.Workspaces;

namespace Application.Workspaces;

public sealed class ArchiveWorkspace(IWorkspaceRepository workspaces, IUnitOfWork unitOfWork)
{
    public async Task<WorkspaceChangeOutcome> ExecuteAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    var workspace = await workspaces.FindAsync(workspaceId, token);

                    if (workspace is null)
                    {
                        return WorkspaceChangeOutcome.NotFound;
                    }

                    workspace.Status = WorkspaceStatus.Archived;

                    return WorkspaceChangeOutcome.Succeeded;
                },
                cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return WorkspaceChangeOutcome.VersionConflict;
        }
    }
}
