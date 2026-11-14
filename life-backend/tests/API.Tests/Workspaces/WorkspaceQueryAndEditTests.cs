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
public class WorkspaceQueryAndEditTests(ApiFactory api)
{
    private const string _route = "/api/v1/Workspaces";

    private static readonly byte[] _version = [1, 2, 3, 4, 5, 6, 7, 8];

    private readonly Guid _ownerId = Guid.NewGuid();

    private readonly Guid _adminId = Guid.NewGuid();

    private readonly Guid _memberId = Guid.NewGuid();

    private readonly Guid _strangerId = Guid.NewGuid();

    private readonly InMemoryWorkspaceRepository _workspaces = new();

    private Guid AddWorkspace(string name, WorkspaceStatus status = WorkspaceStatus.Active)
    {
        var workspace = new Workspace
        {
            Name = name,
            Status = status,
            OwnerId = _ownerId,
            RowVersion = _version,
            Members =
            [
                new WorkspaceMember { UserId = _ownerId, Role = WorkspaceRole.Owner },
                new WorkspaceMember { UserId = _adminId, Role = WorkspaceRole.Admin },
                new WorkspaceMember { UserId = _memberId, Role = WorkspaceRole.Member },
            ],
        };
        _workspaces.Add(workspace);

        return workspace.Id;
    }

    private HttpClient CreateClient(Guid? userId)
    {
        var memberships = new InMemoryWorkspaceMembershipReader();

        foreach (var workspace in _workspaces.Workspaces)
        {
            foreach (var member in workspace.Members)
            {
                memberships.Add(workspace.Id, member.UserId, "Person", member.Role);
            }
        }

        var factory = api.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IWorkspaceRepository>(_workspaces);
            services.AddSingleton<IWorkspaceMembershipReader>(memberships);
            services.AddSingleton<IUnitOfWork, PassThroughUnitOfWork>();
        }));
        var client = factory.CreateClient();

        if (userId is not null)
        {
            client.DefaultRequestHeaders.Add(TestAuthentication.Header, userId.Value.ToString());
        }

        return client;
    }

    private static object Edit(string name, byte[]? version = null)
    {
        return new { name, description = "Updated", rowVersion = version ?? _version };
    }

    [Fact]
    public async Task Get_List_ReturnsOnlyTheWorkspacesOfWhichTheUserIsAMember()
    {
        AddWorkspace("Beta");
        AddWorkspace("Alpha");
        _workspaces.Add(new Workspace { Name = "Foreign", OwnerId = _strangerId });
        var client = CreateClient(_memberId);

        var response = await client.GetAsync(_route);

        var body = await response.Content.ReadFromJsonAsync<List<WorkspaceSummaryResponse>>();
        Assert.Equal(
            (HttpStatusCode.OK, "Alpha,Beta"),
            (response.StatusCode, string.Join(',', body!.Select(workspace => workspace.Name))));
    }

    [Fact]
    public async Task Get_List_ForAUserWithoutWorkspaces_ReturnsAnEmptyList()
    {
        AddWorkspace("Home");
        var client = CreateClient(_strangerId);

        var response = await client.GetAsync(_route);

        var body = await response.Content.ReadFromJsonAsync<List<WorkspaceSummaryResponse>>();
        Assert.Equal((HttpStatusCode.OK, 0), (response.StatusCode, body?.Count));
    }

    [Fact]
    public async Task Get_List_WithoutSession_Returns401()
    {
        var client = CreateClient(userId: null);

        var response = await client.GetAsync(_route);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_Detail_AsAMember_ReturnsTheWorkspaceWithTheOwnRoleAndVersion()
    {
        var id = AddWorkspace("Home");
        var client = CreateClient(_memberId);

        var response = await client.GetAsync($"{_route}/{id}");

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        Assert.Equal(
            (HttpStatusCode.OK, "Home", "Member", Convert.ToBase64String(_version)),
            (
                response.StatusCode,
                root.GetProperty("name").GetString(),
                root.GetProperty("role").GetString(),
                root.GetProperty("rowVersion").GetString()));
    }

    [Fact]
    public async Task Get_Detail_AsANonMember_Returns404()
    {
        var id = AddWorkspace("Home");
        var client = CreateClient(_strangerId);

        var response = await client.GetAsync($"{_route}/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_AsAnAdmin_Returns200WithTheNewData()
    {
        var id = AddWorkspace("Home");
        var client = CreateClient(_adminId);

        var response = await client.PutAsJsonAsync($"{_route}/{id}", Edit("Casa"));

        var body = await response.Content.ReadFromJsonAsync<WorkspaceSummaryResponse>();
        Assert.Equal(
            (HttpStatusCode.OK, "Casa", "Updated"),
            (response.StatusCode, body?.Name, body?.Description));
    }

    [Fact]
    public async Task Put_AsAMember_Returns403AndKeepsTheData()
    {
        var id = AddWorkspace("Home");
        var client = CreateClient(_memberId);

        var response = await client.PutAsJsonAsync($"{_route}/{id}", Edit("Casa"));

        Assert.Equal(
            (HttpStatusCode.Forbidden, "Home"),
            (response.StatusCode, _workspaces.Workspaces.Single().Name));
    }

    [Fact]
    public async Task Put_AsANonMember_Returns404AndKeepsTheData()
    {
        var id = AddWorkspace("Home");
        var client = CreateClient(_strangerId);

        var response = await client.PutAsJsonAsync($"{_route}/{id}", Edit("Casa"));

        Assert.Equal(
            (HttpStatusCode.NotFound, "Home"),
            (response.StatusCode, _workspaces.Workspaces.Single().Name));
    }

    [Fact]
    public async Task Put_WithAStaleVersion_Returns409AndKeepsTheData()
    {
        var id = AddWorkspace("Home");
        var client = CreateClient(_adminId);

        var response = await client.PutAsJsonAsync($"{_route}/{id}", Edit("Casa", [9, 9, 9, 9, 9, 9, 9, 9]));

        Assert.Equal(
            (HttpStatusCode.Conflict, "Home"),
            (response.StatusCode, _workspaces.Workspaces.Single().Name));
    }

    [Fact]
    public async Task Put_OnAnArchivedWorkspace_Returns409AndKeepsTheData()
    {
        var id = AddWorkspace("Home", WorkspaceStatus.Archived);
        var client = CreateClient(_adminId);

        var response = await client.PutAsJsonAsync($"{_route}/{id}", Edit("Casa"));

        Assert.Equal(
            (HttpStatusCode.Conflict, "Home"),
            (response.StatusCode, _workspaces.Workspaces.Single().Name));
    }

    [Fact]
    public async Task Put_WithoutName_Returns400WithTheNameField()
    {
        var id = AddWorkspace("Home");
        var client = CreateClient(_adminId);

        var response = await client.PutAsJsonAsync($"{_route}/{id}", Edit(string.Empty));

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(
            (HttpStatusCode.BadRequest, true),
            (response.StatusCode, document.RootElement.GetProperty("errors").TryGetProperty("Name", out _)));
    }

    [Fact]
    public async Task Put_WithoutVersion_Returns400WithTheRowVersionField()
    {
        var id = AddWorkspace("Home");
        var client = CreateClient(_adminId);

        var response = await client.PutAsJsonAsync($"{_route}/{id}", new { name = "Casa" });

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(
            (HttpStatusCode.BadRequest, true),
            (response.StatusCode, document.RootElement.GetProperty("errors").TryGetProperty("RowVersion", out _)));
    }

    [Fact]
    public async Task Post_Archive_AsTheOwner_Returns204AndArchivesTheWorkspace()
    {
        var id = AddWorkspace("Home");
        var client = CreateClient(_ownerId);

        var response = await client.PostAsync($"{_route}/{id}/archive", content: null);

        Assert.Equal(
            (HttpStatusCode.NoContent, WorkspaceStatus.Archived),
            (response.StatusCode, _workspaces.Workspaces.Single().Status));
    }

    [Fact]
    public async Task Post_Archive_AsAnAdmin_Returns403AndKeepsTheWorkspaceActive()
    {
        var id = AddWorkspace("Home");
        var client = CreateClient(_adminId);

        var response = await client.PostAsync($"{_route}/{id}/archive", content: null);

        Assert.Equal(
            (HttpStatusCode.Forbidden, WorkspaceStatus.Active),
            (response.StatusCode, _workspaces.Workspaces.Single().Status));
    }

    [Fact]
    public async Task Post_Archive_AsANonMember_Returns404AndKeepsTheWorkspaceActive()
    {
        var id = AddWorkspace("Home");
        var client = CreateClient(_strangerId);

        var response = await client.PostAsync($"{_route}/{id}/archive", content: null);

        Assert.Equal(
            (HttpStatusCode.NotFound, WorkspaceStatus.Active),
            (response.StatusCode, _workspaces.Workspaces.Single().Status));
    }
}
