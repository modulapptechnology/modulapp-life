using Life.Domain.Workspaces;

namespace Life.Application.Workspaces;

public sealed record WorkspaceMemberSummary(Guid UserId, string Name, string? AvatarUrl, WorkspaceRole Role);
