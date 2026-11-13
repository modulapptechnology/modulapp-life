using Domain.Common;
using Domain.Users;

namespace Domain.Workspaces;

public class Workspace : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public WorkspaceType Type { get; set; } = WorkspaceType.Personal;

    public WorkspaceStatus Status { get; set; } = WorkspaceStatus.Active;

    public Guid OwnerId { get; set; }

    public User? Owner { get; set; }

    public ICollection<WorkspaceMember> Members { get; set; } = [];

    public ICollection<WorkspaceInvitation> Invitations { get; set; } = [];
}
