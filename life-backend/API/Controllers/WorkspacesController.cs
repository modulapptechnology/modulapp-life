using Life.API.Authorization;
using Life.Application.Workspaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Life.API.Controllers;

public class WorkspacesController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<WorkspaceSummaryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<WorkspaceSummaryResponse>>> List(
        [FromServices] ListWorkspaces listWorkspaces,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return NoSession();
        }

        return Ok(await listWorkspaces.ExecuteAsync(userId.Value, cancellationToken));
    }

    [HttpGet("{workspaceId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.WorkspaceMember)]
    [ProducesResponseType<WorkspaceSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkspaceSummaryResponse>> Get(
        Guid workspaceId,
        [FromServices] GetWorkspace getWorkspace,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return NoSession();
        }

        var workspace = await getWorkspace.ExecuteAsync(userId.Value, workspaceId, cancellationToken);

        return workspace is null ? NotFoundWorkspace() : Ok(workspace);
    }

    [HttpPut("{workspaceId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.WorkspaceAdmin)]
    [ProducesResponseType<WorkspaceSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WorkspaceSummaryResponse>> Update(
        Guid workspaceId,
        [FromBody] UpdateWorkspaceRequest request,
        [FromServices] UpdateWorkspace updateWorkspace,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return NoSession();
        }

        var result = await updateWorkspace.ExecuteAsync(userId.Value, workspaceId, request, cancellationToken);

        return result.Outcome switch
        {
            WorkspaceChangeOutcome.Succeeded => Ok(result.Workspace),
            WorkspaceChangeOutcome.NotFound => NotFoundWorkspace(),
            WorkspaceChangeOutcome.Archived => Problem(
                title: "El espacio de trabajo está archivado y no se puede editar.",
                statusCode: StatusCodes.Status409Conflict),
            _ => VersionConflict(),
        };
    }

    [HttpPost("{workspaceId:guid}/archive")]
    [Authorize(Policy = AuthorizationPolicies.WorkspaceOwner)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Archive(
        Guid workspaceId,
        [FromServices] ArchiveWorkspace archiveWorkspace,
        CancellationToken cancellationToken)
    {
        var outcome = await archiveWorkspace.ExecuteAsync(workspaceId, cancellationToken);

        return outcome switch
        {
            WorkspaceChangeOutcome.Succeeded => NoContent(),
            WorkspaceChangeOutcome.NotFound => NotFoundWorkspace(),
            _ => VersionConflict(),
        };
    }

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
            return NoSession();
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

    private ObjectResult NoSession()
    {
        return Problem(
            title: "No hay una sesión válida.",
            statusCode: StatusCodes.Status401Unauthorized);
    }

    private ObjectResult NotFoundWorkspace()
    {
        return Problem(
            title: "El recurso solicitado no existe.",
            statusCode: StatusCodes.Status404NotFound);
    }

    private ObjectResult VersionConflict()
    {
        return Problem(
            title: "El espacio de trabajo fue modificado por otra operación. Recargue e intente de nuevo.",
            statusCode: StatusCodes.Status409Conflict);
    }
}
