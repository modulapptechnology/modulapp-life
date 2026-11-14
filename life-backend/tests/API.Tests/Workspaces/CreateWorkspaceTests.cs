using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Life.API.Tests.Support;
using Life.Application.Common;
using Life.Application.Workspaces;
using Life.Domain.Workspaces;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Life.API.Tests.Workspaces;

[Collection(ApiCollection.Name)]
public class CreateWorkspaceTests(ApiFactory api)
{
    private const string _route = "/api/v1/Workspaces";

    private static readonly Guid _userId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private (HttpClient Client, InMemoryWorkspaceRepository Workspaces) CreateClient(int? limit = null)
    {
        var workspaces = new InMemoryWorkspaceRepository();
        var factory = api.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IWorkspaceRepository>(workspaces);
            services.AddSingleton<IUnitOfWork, PassThroughUnitOfWork>();

            if (limit is not null)
            {
                services.AddSingleton(new WorkspaceLimitOptions(limit.Value));
            }
        }));
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthentication.Header, _userId.ToString());

        return (client, workspaces);
    }

    [Fact]
    public async Task Post_WithValidData_Returns201WithTheIdOfTheNewWorkspace()
    {
        var (client, workspaces) = CreateClient();

        var response = await client.PostAsJsonAsync(
            _route,
            new { name = "Home", type = "Household", description = "Shared tasks" });

        var body = await response.Content.ReadFromJsonAsync<CreateWorkspaceResponse>();
        Assert.Equal(
            (HttpStatusCode.Created, workspaces.Workspaces.Single().Id),
            (response.StatusCode, body?.Id));
    }

    [Fact]
    public async Task Post_WithValidData_StoresTheCreatorAsOwnerAndMember()
    {
        var (client, workspaces) = CreateClient();

        await client.PostAsJsonAsync(_route, new { name = "Home", type = "Personal" });

        var stored = workspaces.Workspaces.Single();
        var member = Assert.Single(stored.Members);
        Assert.Equal(
            (_userId, _userId, WorkspaceRole.Owner),
            (stored.OwnerId, member.UserId, member.Role));
    }

    [Fact]
    public async Task Post_WithoutSession_Returns401()
    {
        var (client, _) = CreateClient();
        client.DefaultRequestHeaders.Remove(TestAuthentication.Header);

        var response = await client.PostAsJsonAsync(_route, new { name = "Home", type = "Personal" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithoutName_Returns400WithTheNameField()
    {
        var (client, workspaces) = CreateClient();

        var response = await client.PostAsJsonAsync(_route, new { name = "", type = "Personal" });

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(
            (HttpStatusCode.BadRequest, true, 0),
            (
                response.StatusCode,
                document.RootElement.GetProperty("errors").TryGetProperty("Name", out _),
                workspaces.Workspaces.Count));
    }

    [Fact]
    public async Task Post_WithUnknownType_Returns400()
    {
        var (client, _) = CreateClient();

        var response = await client.PostAsJsonAsync(_route, new { name = "Home", type = 99 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenThePlanLimitIsReached_Returns409AndStoresNothingMore()
    {
        var (client, workspaces) = CreateClient(limit: 1);
        await client.PostAsJsonAsync(_route, new { name = "First", type = "Personal" });

        var response = await client.PostAsJsonAsync(_route, new { name = "Second", type = "Personal" });

        Assert.Equal(
            (HttpStatusCode.Conflict, 1),
            (response.StatusCode, workspaces.Workspaces.Count));
    }
}
