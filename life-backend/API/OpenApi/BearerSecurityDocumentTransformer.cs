using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Life.API.OpenApi;

public sealed class BearerSecurityDocumentTransformer : IOpenApiDocumentTransformer
{
    public const string SchemeName = "Bearer";

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Token de acceso devuelto por el inicio de sesión. Se escribe sin el prefijo Bearer.",
        };

        return Task.CompletedTask;
    }
}
