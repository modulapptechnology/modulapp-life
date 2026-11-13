namespace Application.Workspaces;

public sealed record UpdateWorkspaceResult(WorkspaceChangeOutcome Outcome, WorkspaceSummaryResponse? Workspace);
