namespace Life.Application.Workspaces;

public sealed record UpdateWorkspaceRequest(string Name, string? Description, byte[] RowVersion);
