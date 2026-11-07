using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace API.Filters;

public static class ValidationProblemFactory
{
    public const string InvalidValueMessage = "El valor no es válido.";

    private const string _jsonPathPrefix = "$";

    public static BadRequestObjectResult Create(ActionContext context, ModelStateDictionary modelState)
    {
        var sanitized = new ModelStateDictionary();

        foreach (var (key, entry) in modelState)
        {
            var isJsonError = key.StartsWith(_jsonPathPrefix, StringComparison.Ordinal);
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
