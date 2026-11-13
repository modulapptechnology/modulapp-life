using API.Authorization;
using API.Controllers;
using Application.Workspaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Tests.Support;

public class TestPoliciesController(IWorkspaceMembershipReader memberships) : ApiControllerBase
{
    [HttpGet("unmarked")]
    public IActionResult Unmarked()
    {
        return Ok();
    }

    [Authorize(Policy = AuthorizationPolicies.WorkspaceMember)]
    [HttpGet("workspaces/{workspaceId:guid}/member")]
    public IActionResult Member(Guid workspaceId)
    {
        return Ok();
    }

    [Authorize(Policy = AuthorizationPolicies.WorkspaceAdmin)]
    [HttpGet("workspaces/{workspaceId:guid}/admin")]
    public IActionResult Admin(Guid workspaceId)
    {
        return Ok();
    }

    [Authorize(Policy = AuthorizationPolicies.WorkspaceMember)]
    [HttpGet("workspaces/{workspaceId:guid}/members")]
    public async Task<ActionResult<IReadOnlyList<WorkspaceMemberSummary>>> Members(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        return Ok(await memberships.ListMembersAsync(workspaceId, cancellationToken));
    }

    [Authorize(Policy = AuthorizationPolicies.OwnUser)]
    [HttpGet("users/{userId:guid}")]
    public IActionResult OwnUser(Guid userId)
    {
        return Ok();
    }
}
