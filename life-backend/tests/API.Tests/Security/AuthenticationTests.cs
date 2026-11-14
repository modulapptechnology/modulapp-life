using System.Net;
using System.Net.Http.Json;
using Life.API.Tests.Support;
using Life.Support;
using Life.Support.Seed;

namespace Life.API.Tests.Security;

[Collection(ApiCollection.Name)]
public class AuthenticationTests(ApiFactory api)
{
    [Fact]
    public async Task Get_WithoutAuthentication_Returns401()
    {
        var client = api.CreateApiClient();

        var response = await client.GetAsync($"/api/v1/TestWorkspaces/{SeedData.MainWorkspace.Id}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Put_WithoutAuthentication_Returns401()
    {
        var client = api.CreateApiClient();

        var response = await client.PutAsJsonAsync(
            $"/api/v1/TestWorkspaces/{SeedData.MainWorkspace.Id}",
            new { name = "New name" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [RequiresTestDatabaseFact]
    public async Task Get_UserWithoutPermission_Returns403()
    {
        var client = api.CreateApiClient(SeedData.Member.Id);

        var response = await client.GetAsync($"/api/v1/TestWorkspaces/{SeedData.MainWorkspace.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [RequiresTestDatabaseFact]
    public async Task Put_UserWithoutPermission_Returns403()
    {
        var client = api.CreateApiClient(SeedData.Member.Id);

        var response = await client.PutAsJsonAsync(
            $"/api/v1/TestWorkspaces/{SeedData.MainWorkspace.Id}",
            new { name = "New name" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
