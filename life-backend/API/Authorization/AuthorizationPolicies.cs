namespace API.Authorization;

public static class AuthorizationPolicies
{
    public const string WorkspaceMember = "WorkspaceMember";

    public const string WorkspaceAdmin = "WorkspaceAdmin";

    public const string WorkspaceOwner = "WorkspaceOwner";

    public const string OwnUser = "OwnUser";

    public const string WorkspaceRouteKey = "workspaceId";

    public const string UserRouteKey = "userId";
}
