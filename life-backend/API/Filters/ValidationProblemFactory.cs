using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace API.Filters;

public static class ValidationProblemFactory
{
    public const string InvalidValueMessage = "El valor no es válido.";

    private const string JsonPathPrefix = "$";

    public static BadRequestObjectResult Create(ActionContext context, ModelStateDictionary modelState)
    {
        var sanitized = new ModelStateDictionary();

        foreach (var (key, entry) in modelState)
        {
            // Los errores del cuerpo JSON (clave "$.campo") traen detalles internos como tipos, rutas y
            // posiciones.
            var isJsonError = key.StartsWith(JsonPathPrefix, StringComparison.Ordinal);
            var publicKey = isJsonError ? key.TrimStart('$', '.') : key;

            foreach (var error in entry.Errors)
            {
                var message = isJsonError || error.Exception is not null
                    ? InvalidValueMessage
                    : error.ErrorMessage;
                sanitized.AddModelError(publicKey, message);
            }
        }

        var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var details = factory.CreateValidationProblemDetails(
            context.HttpContext,
            sanitized,
            StatusCodes.Status400BadRequest);

        return new BadRequestObjectResult(details);
    }
}
