using Domain.Common;
using Domain.Users;

namespace Domain.Workspaces;

public class WorkspaceMember : BaseEntity
{
    public Guid WorkspaceId { get; set; }

    public Workspace? Workspace { get; set; }

    public Guid UserId { get; set; }

    public User? User { get; set; }

    public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;
}
