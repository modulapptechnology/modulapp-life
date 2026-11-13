using Domain.Workspaces;

namespace Application.Workspaces;

public sealed record WorkspaceMemberSummary(Guid UserId, string Name, string? AvatarUrl, WorkspaceRole Role);
