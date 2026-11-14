using Life.Domain.Workspaces;
using Microsoft.AspNetCore.Authorization;

namespace Life.API.Authorization;

public static class ApiAuthorizationExtensions
{
    public static IServiceCollection AddApiAuthorization(this IServiceCollection services)
    {
        services
            .AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(AuthorizationPolicies.OwnUser, policy => policy
                .RequireAuthenticatedUser()
                .AddRequirements(new OwnUserRequirement()))
            .AddPolicy(AuthorizationPolicies.WorkspaceMember, policy => RequireRole(policy, WorkspaceRole.Member))
            .AddPolicy(AuthorizationPolicies.WorkspaceAdmin, policy => RequireRole(policy, WorkspaceRole.Admin))
            .AddPolicy(AuthorizationPolicies.WorkspaceOwner, policy => RequireRole(policy, WorkspaceRole.Owner));

        services.AddSingleton<IAuthorizationHandler, WorkspaceRoleHandler>();
        services.AddSingleton<IAuthorizationHandler, OwnUserHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, ProblemAuthorizationResultHandler>();

        return services;
    }

    private static void RequireRole(AuthorizationPolicyBuilder policy, WorkspaceRole minimumRole)
    {
        policy.RequireAuthenticatedUser().AddRequirements(new WorkspaceRoleRequirement(minimumRole));
    }
}
