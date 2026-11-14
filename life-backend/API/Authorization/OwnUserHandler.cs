using Microsoft.AspNetCore.Authorization;

namespace Life.API.Authorization;

public sealed class OwnUserHandler : AuthorizationHandler<OwnUserRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnUserRequirement requirement)
    {
        var userId = context.User.GetUserId();

        if (userId is null || context.Resource is not HttpContext http)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        var routeValue = http.Request.RouteValues[AuthorizationPolicies.UserRouteKey]?.ToString();

        if (Guid.TryParse(routeValue, out var ownerId) && ownerId == userId)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(this, AuthorizationFailureReasons.ResourceHidden));
        }

        return Task.CompletedTask;
    }
}
