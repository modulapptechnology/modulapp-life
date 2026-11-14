using Life.Application.Common;
using Life.Domain.Workspaces;

namespace Life.Application.Workspaces;

public sealed class CreateWorkspace(
    IWorkspaceRepository workspaces,
    IUnitOfWork unitOfWork,
    WorkspaceLimitOptions options)
{
    public async Task<CreateWorkspaceResponse?> ExecuteAsync(
        Guid ownerId,
        CreateWorkspaceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var workspace = new Workspace
        {
            Name = request.Name.Trim(),
            Type = request.Type,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            OwnerId = ownerId,
            Members = [new WorkspaceMember { UserId = ownerId, Role = WorkspaceRole.Owner }],
        };

        var created = await unitOfWork.ExecuteInTransactionAsync(
            async token =>
            {
                if (await workspaces.CountOwnedAsync(ownerId, token) >= options.MaxOwnedWorkspaces)
                {
                    return false;
                }

                workspaces.Add(workspace);

                return true;
            },
            cancellationToken);

        return created ? new CreateWorkspaceResponse(workspace.Id) : null;
    }
}
