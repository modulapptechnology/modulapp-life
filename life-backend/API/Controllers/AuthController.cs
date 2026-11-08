using API.Extensions;
using Application.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

public class AuthController : ApiControllerBase
{
    [HttpPost("register")]
    [EnableRateLimiting(ApiServiceCollectionExtensions.RegistrationRateLimitPolicy)]
    [ProducesResponseType<RegisterUserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterUserResponse>> Register(
        [FromBody] RegisterUserRequest request,
        [FromServices] RegisterUser registerUser,
        CancellationToken cancellationToken)
    {
        var response = await registerUser.ExecuteAsync(request, cancellationToken);

        if (response is null)
        {
            return Problem(
                title: "No se pudo completar el registro con los datos indicados.",
                statusCode: StatusCodes.Status409Conflict);
        }

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("login")]
    [EnableRateLimiting(ApiServiceCollectionExtensions.LoginRateLimitPolicy)]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        [FromServices] LoginUser loginUser,
        CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var response = await loginUser.ExecuteAsync(request, ipAddress, cancellationToken);

        if (response is null)
        {
            return Problem(
                title: "El correo electrónico o la contraseña no son correctos.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        return Ok(response);
    }

    [HttpPost("refresh")]
    [EnableRateLimiting(ApiServiceCollectionExtensions.LoginRateLimitPolicy)]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Refresh(
        [FromBody] RefreshSessionRequest request,
        [FromServices] RefreshSession refreshSession,
        CancellationToken cancellationToken)
    {
        var response = await refreshSession.ExecuteAsync(request, cancellationToken);

        if (response is null)
        {
            return Problem(
                title: "El token de renovación no es válido o ya venció.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        return Ok(response);
    }

    [HttpGet("verify-email")]
    [EnableRateLimiting(ApiServiceCollectionExtensions.RegistrationRateLimitPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail(
        [FromQuery] VerifyEmailRequest request,
        [FromServices] VerifyEmail verifyEmail,
        CancellationToken cancellationToken)
    {
        var verified = await verifyEmail.ExecuteAsync(request, cancellationToken);

        if (!verified)
        {
            return Problem(
                title: "El enlace de verificación no es válido o ya venció.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return NoContent();
    }
}
