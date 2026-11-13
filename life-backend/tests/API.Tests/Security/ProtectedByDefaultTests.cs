using API.Tests.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests.Security;

[Collection(ApiCollection.Name)]
public class ProtectedByDefaultTests(ApiFactory api)
{
    private static readonly string[] _publicEndpoints =
    [
        "GET api/v{version:apiVersion}/House",
        "POST api/v{version:apiVersion}/Auth/login",
        "POST api/v{version:apiVersion}/Auth/logout",
        "POST api/v{version:apiVersion}/Auth/refresh",
        "POST api/v{version:apiVersion}/Auth/register",
        "POST api/v{version:apiVersion}/Auth/verify-email",
    ];

    [Fact]
    public async Task FallbackPolicy_ByDefault_RequiresAnAuthenticatedUser()
    {
        var provider = api.Services.GetRequiredService<IAuthorizationPolicyProvider>();

        var policy = await provider.GetFallbackPolicyAsync();

        Assert.Contains(policy!.Requirements, requirement => requirement is DenyAnonymousAuthorizationRequirement);
    }

    [Fact]
    public void Endpoints_DeclaredAsPublic_AreExactlyTheExplicitList()
    {
        var endpoints = api.Services.GetRequiredService<EndpointDataSource>().Endpoints;

        var publicEndpoints = endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            .SelectMany(endpoint => (endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? [])
                .Select(method => $"{method} {endpoint.RoutePattern.RawText}"))
            .Distinct()
            .Order()
            .ToArray();

        Assert.Equal(_publicEndpoints.Order(), publicEndpoints);
    }
}
