namespace Life.Application.Workspaces;

public sealed record WorkspaceLimitOptions(int MaxOwnedWorkspaces)
{
    public const string SectionName = "Workspaces:Limits";

    public const string MaxOwnedWorkspacesKey = "MaxOwnedWorkspaces";

    public const int DefaultMaxOwnedWorkspaces = 5;
}
