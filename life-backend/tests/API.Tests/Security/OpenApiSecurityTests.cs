using Life.API.Tests.Support;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Life.API.Tests.Security;

[Collection(ApiCollection.Name)]
public class OpenApiSecurityTests(ApiFactory api)
{
    private async Task<OpenApiDocument> ReadDocumentAsync()
    {
        var provider = api.Services.GetRequiredKeyedService<IOpenApiDocumentProvider>("v1");

        return await provider.GetOpenApiDocumentAsync();
    }

    [Fact]
    public async Task Document_Always_DeclaresTheBearerJwtScheme()
    {
        var document = await ReadDocumentAsync();

        var scheme = document.Components!.SecuritySchemes!["Bearer"];
        Assert.Equal(
            (SecuritySchemeType.Http, "bearer", "JWT"),
            (scheme.Type, scheme.Scheme, scheme.BearerFormat));
    }

    [Fact]
    public async Task Document_ProtectedEndpoint_RequiresTheBearerScheme()
    {
        var document = await ReadDocumentAsync();

        var operation = document.Paths["/api/v1/TestPolicies/unmarked"].Operations![HttpMethod.Get];

        Assert.Single(operation.Security!);
    }

    [Theory]
    [InlineData("/api/v1/House", "Get")]
    [InlineData("/api/v1/Auth/login", "Post")]
    [InlineData("/api/v1/Auth/register", "Post")]
    public async Task Document_PublicEndpoint_DoesNotRequireAuthentication(string path, string method)
    {
        var document = await ReadDocumentAsync();

        var operation = document.Paths[path].Operations![new HttpMethod(method)];

        Assert.True(operation.Security is null || operation.Security.Count == 0);
    }
}
