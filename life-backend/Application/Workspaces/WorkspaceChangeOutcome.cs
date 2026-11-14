namespace Life.Application.Workspaces;

public enum WorkspaceChangeOutcome
{
    Succeeded,
    NotFound,
    VersionConflict,
    Archived,
}
