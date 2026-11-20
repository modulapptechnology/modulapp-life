using Life.Domain.Common;
using Life.Domain.Users;
using Life.Domain.Workspaces;

namespace Life.Domain.Sync;

public class SyncOperationLog : BaseEntity
{
    public Guid OperationId { get; set; }

    public Guid WorkspaceId { get; set; }

    public Workspace? Workspace { get; set; }

    public Guid UserId { get; set; }

    public User? User { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public SyncOperationKind Kind { get; set; }

    public SyncOperationStatus Status { get; set; }

    public string? ErrorCode { get; set; }
}
