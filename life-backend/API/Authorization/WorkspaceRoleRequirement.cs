using Life.Domain.Workspaces;
using Microsoft.AspNetCore.Authorization;

namespace Life.API.Authorization;

public sealed class WorkspaceRoleRequirement(WorkspaceRole minimumRole) : IAuthorizationRequirement
{
    public WorkspaceRole MinimumRole { get; } = minimumRole;
}
