using Domain.Common;

namespace Domain.Workspaces;

public class WorkspaceInvitation : BaseEntity
{
    public Guid WorkspaceId { get; set; }

    public Workspace? Workspace { get; set; }

    public string Email { get; set; } = string.Empty;

    public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;

    public string TokenHash { get; set; } = string.Empty;

    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    public DateTime ExpiresAt { get; set; }
}
