using API.Authorization;
using Application.Workspaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class WorkspacesController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateWorkspaceResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateWorkspaceResponse>> Create(
        [FromBody] CreateWorkspaceRequest request,
        [FromServices] CreateWorkspace createWorkspace,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Problem(
                title: "No hay una sesión válida.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var response = await createWorkspace.ExecuteAsync(userId.Value, request, cancellationToken);

        if (response is null)
        {
            return Problem(
                title: "Se alcanzó el límite de espacios de trabajo del plan.",
                statusCode: StatusCodes.Status409Conflict);
        }

        return StatusCode(StatusCodes.Status201Created, response);
    }
}
