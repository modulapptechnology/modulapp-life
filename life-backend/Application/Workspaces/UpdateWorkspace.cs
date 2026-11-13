using Application.Common;
using Domain.Workspaces;

namespace Application.Workspaces;

public sealed class UpdateWorkspace(IWorkspaceRepository workspaces, IUnitOfWork unitOfWork)
{
    public async Task<UpdateWorkspaceResult> ExecuteAsync(
        Guid userId,
        Guid workspaceId,
        UpdateWorkspaceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        WorkspaceChangeOutcome outcome;

        try
        {
            outcome = await unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    var workspace = await workspaces.FindAsync(workspaceId, token);

                    if (workspace is null)
                    {
                        return WorkspaceChangeOutcome.NotFound;
                    }

                    if (workspace.Status == WorkspaceStatus.Archived)
                    {
                        return WorkspaceChangeOutcome.Archived;
                    }

                    if (!request.RowVersion.SequenceEqual(workspace.RowVersion))
                    {
                        return WorkspaceChangeOutcome.VersionConflict;
                    }

                    workspace.Name = request.Name.Trim();
                    workspace.Description = string.IsNullOrWhiteSpace(request.Description)
                        ? null
                        : request.Description.Trim();

                    return WorkspaceChangeOutcome.Succeeded;
                },
                cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            outcome = WorkspaceChangeOutcome.VersionConflict;
        }

        if (outcome != WorkspaceChangeOutcome.Succeeded)
        {
            return new UpdateWorkspaceResult(outcome, null);
        }

        var updated = await workspaces.FindByMemberAsync(workspaceId, userId, cancellationToken);

        return updated is null
            ? new UpdateWorkspaceResult(WorkspaceChangeOutcome.NotFound, null)
            : new UpdateWorkspaceResult(WorkspaceChangeOutcome.Succeeded, updated);
    }
}
