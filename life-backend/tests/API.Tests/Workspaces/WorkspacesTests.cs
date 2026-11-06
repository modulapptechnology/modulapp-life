using System.Net;
using System.Net.Http.Json;
using API.Tests.Support;
using Microsoft.Data.SqlClient;
using TestSupport;
using TestSupport.Seed;

namespace API.Tests.Workspaces;

[Collection(ApiCollection.Name)]
public class WorkspacesTests(ApiFactory api)
{
    [RequiresTestDatabaseFact]
    public async Task Get_Owner_Returns200WithWorkspaceData()
    {
        await using var workspace = await TemporaryWorkspace.CreateAsync(api, SeedData.Owner.Id);
        var client = api.CreateApiClient(SeedData.Owner.Id);

        var response = await client.GetAsync($"/api/v1/TestWorkspaces/{workspace.Workspace.Id}");

        var body = await response.Content.ReadFromJsonAsync<WorkspaceResponse>();
        Assert.Equal((HttpStatusCode.OK, workspace.Workspace.Id, workspace.Workspace.Name), (response.StatusCode, body?.Id, body?.Name));
    }

    [RequiresTestDatabaseFact]
    public async Task Get_NonExistentWorkspace_Returns404()
    {
        var client = api.CreateApiClient(SeedData.Owner.Id);

        var response = await client.GetAsync($"/api/v1/TestWorkspaces/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [RequiresTestDatabaseFact]
    public async Task Get_WorkspaceOfAnotherUser_Returns403WithoutExposingItsData()
    {
        await using var workspace = await TemporaryWorkspace.CreateAsync(api, SeedData.Owner.Id);
        var client = api.CreateApiClient(SeedData.Member.Id);

        var response = await client.GetAsync($"/api/v1/TestWorkspaces/{workspace.Workspace.Id}");

        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal((HttpStatusCode.Forbidden, false), (response.StatusCode, body.Contains(workspace.Workspace.Name)));
    }

    [RequiresTestDatabaseFact]
    public async Task Put_Owner_Returns204AndUpdatesTheName()
    {
        await using var workspace = await TemporaryWorkspace.CreateAsync(api, SeedData.Owner.Id);
        var client = api.CreateApiClient(SeedData.Owner.Id);

        var response = await client.PutAsJsonAsync($"/api/v1/TestWorkspaces/{workspace.Workspace.Id}", new { name = "Updated name" });

        Assert.Equal((HttpStatusCode.NoContent, "Updated name"), (response.StatusCode, await workspace.ReadNameAsync()));
    }

    [RequiresTestDatabaseFact]
    public async Task Put_EmptyName_Returns400AndDoesNotModifyTheWorkspace()
    {
        await using var workspace = await TemporaryWorkspace.CreateAsync(api, SeedData.Owner.Id);
        var client = api.CreateApiClient(SeedData.Owner.Id);

        var response = await client.PutAsJsonAsync($"/api/v1/TestWorkspaces/{workspace.Workspace.Id}", new { name = "" });

        Assert.Equal((HttpStatusCode.BadRequest, workspace.Workspace.Name), (response.StatusCode, await workspace.ReadNameAsync()));
    }

    [RequiresTestDatabaseFact]
    public async Task Put_NonExistentWorkspace_Returns404()
    {
        var client = api.CreateApiClient(SeedData.Owner.Id);

        var response = await client.PutAsJsonAsync($"/api/v1/TestWorkspaces/{Guid.NewGuid()}", new { name = "Name" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [RequiresTestDatabaseFact]
    public async Task Put_WorkspaceOfAnotherUser_Returns403AndDoesNotModifyTheWorkspace()
    {
        await using var workspace = await TemporaryWorkspace.CreateAsync(api, SeedData.Owner.Id);
        var client = api.CreateApiClient(SeedData.Member.Id);

        var response = await client.PutAsJsonAsync($"/api/v1/TestWorkspaces/{workspace.Workspace.Id}", new { name = "Foreign name" });

        Assert.Equal((HttpStatusCode.Forbidden, workspace.Workspace.Name), (response.StatusCode, await workspace.ReadNameAsync()));
    }

    [RequiresTestDatabaseFact]
    public async Task TemporaryWorkspace_OnDispose_LeavesNoResidualData()
    {
        var workspace = await TemporaryWorkspace.CreateAsync(api, SeedData.Owner.Id);

        await workspace.DisposeAsync();

        Assert.Equal(SeedData.Workspaces.Count, await CountWorkspacesAsync());
        Assert.Null(await workspace.ReadNameAsync());
    }

    private async Task<int> CountWorkspacesAsync()
    {
        await using var connection = await api.OpenConnectionAsync();
        await using var command = new SqlCommand("SELECT COUNT(*) FROM dbo.Workspaces", connection);

        return (int)(await command.ExecuteScalarAsync())!;
    }
}
