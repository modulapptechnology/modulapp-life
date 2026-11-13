using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace API.Authorization;

public sealed class ProblemAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Succeeded)
        {
            await _default.HandleAsync(next, context, policy, authorizeResult);
            return;
        }

        if (authorizeResult.Challenged)
        {
            await _default.HandleAsync(next, context, policy, authorizeResult);
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, "Se requiere una sesión válida.");
        }
        else if (IsHidden(authorizeResult))
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "El recurso solicitado no existe.");
        }
        else
        {
            await _default.HandleAsync(next, context, policy, authorizeResult);
            await WriteProblemAsync(
                context,
                StatusCodes.Status403Forbidden,
                "No tiene permisos para realizar esta operación.");
        }
    }

    private static bool IsHidden(PolicyAuthorizationResult authorizeResult)
    {
        return authorizeResult.AuthorizationFailure?.FailureReasons.Any(reason =>
            reason.Message == AuthorizationFailureReasons.ResourceHidden) ?? false;
    }

    private static Task WriteProblemAsync(HttpContext context, int statusCode, string title)
    {
        return Results.Problem(title: title, statusCode: statusCode).ExecuteAsync(context);
    }
}
