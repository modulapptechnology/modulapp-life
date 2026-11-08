using API.Extensions;
using Application.Users;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

public class AuthController(IConfiguration configuration) : ApiControllerBase
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

        return Ok(DeliverRefreshToken(response));
    }

    [HttpPost("refresh")]
    [EnableRateLimiting(ApiServiceCollectionExtensions.LoginRateLimitPolicy)]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Refresh(
        [FromBody] RefreshBody? body,
        [FromServices] RefreshSession refreshSession,
        [FromServices] IValidator<RefreshSessionRequest> validator,
        CancellationToken cancellationToken)
    {
        var request = new RefreshSessionRequest(body?.RefreshToken ?? RefreshTokenCookie.Read(Request) ?? string.Empty);
        var validation = await validator.ValidateAsync(request, cancellationToken);

        if (!validation.IsValid)
        {
            foreach (var failure in validation.Errors)
            {
                ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
            }

            return ValidationProblem(ModelState);
        }

        var response = await refreshSession.ExecuteAsync(request, cancellationToken);

        if (response is null)
        {
            if (RefreshTokenCookie.IsRequested(Request))
            {
                RefreshTokenCookie.Delete(HttpContext, configuration);
            }

            return Problem(
                title: "El token de renovación no es válido o ya venció.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        return Ok(DeliverRefreshToken(response));
    }

    [HttpPost("logout")]
    [EnableRateLimiting(ApiServiceCollectionExtensions.LoginRateLimitPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshBody? body,
        [FromServices] EndSession endSession,
        CancellationToken cancellationToken)
    {
        await endSession.ExecuteAsync(body?.RefreshToken ?? RefreshTokenCookie.Read(Request), cancellationToken);
        RefreshTokenCookie.Delete(HttpContext, configuration);

        return NoContent();
    }

    private LoginResponse DeliverRefreshToken(LoginResponse response)
    {
        return RefreshTokenCookie.IsRequested(Request)
            ? RefreshTokenCookie.Write(HttpContext, response, configuration)
            : response;
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

public sealed record RefreshBody(string? RefreshToken);
