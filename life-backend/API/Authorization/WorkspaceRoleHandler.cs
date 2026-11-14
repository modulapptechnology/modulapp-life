using Life.Application.Workspaces;
using Microsoft.AspNetCore.Authorization;

namespace Life.API.Authorization;

public sealed class WorkspaceRoleHandler : AuthorizationHandler<WorkspaceRoleRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WorkspaceRoleRequirement requirement)
    {
        var userId = context.User.GetUserId();

        if (userId is null || context.Resource is not HttpContext http)
        {
            context.Fail();
            return;
        }

        var routeValue = http.Request.RouteValues[AuthorizationPolicies.WorkspaceRouteKey]?.ToString();

        if (!Guid.TryParse(routeValue, out var workspaceId))
        {
            context.Fail(HiddenResource());
            return;
        }

        var memberships = http.RequestServices.GetRequiredService<IWorkspaceMembershipReader>();
        var role = await memberships.FindRoleAsync(workspaceId, userId.Value, http.RequestAborted);

        if (role is null)
        {
            context.Fail(HiddenResource());
        }
        else if (role < requirement.MinimumRole)
        {
            context.Fail();
        }
        else
        {
            context.Succeed(requirement);
        }
    }

    private AuthorizationFailureReason HiddenResource()
    {
        return new AuthorizationFailureReason(this, AuthorizationFailureReasons.ResourceHidden);
    }
}
