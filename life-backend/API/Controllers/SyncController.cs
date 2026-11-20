using Life.API.Authorization;
using Life.Application.Sync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Life.API.Controllers;

public class SyncController : ApiControllerBase
{
    [HttpPost("/api/v{version:apiVersion}/workspaces/{workspaceId:guid}/sync")]
    [Authorize(Policy = AuthorizationPolicies.WorkspaceMember)]
    [ProducesResponseType<SyncResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SyncResponse>> Synchronize(
        Guid workspaceId,
        [FromBody] SyncRequest request,
        [FromServices] SynchronizeChanges synchronizeChanges,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return NoSession();
        }

        return Ok(await synchronizeChanges.ExecuteAsync(userId.Value, workspaceId, request, cancellationToken));
    }

    private ObjectResult NoSession()
    {
        return Problem(
            title: "No hay una sesión válida.",
            statusCode: StatusCodes.Status401Unauthorized);
    }
}
