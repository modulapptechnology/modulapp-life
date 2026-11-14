using System.Net;
using System.Text.Json;
using Life.API.Tests.Support;
using Life.Application.Workspaces;
using Life.Domain.Workspaces;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Life.API.Tests.Security;

[Collection(ApiCollection.Name)]
public class AuthorizationPolicyTests(ApiFactory api)
{
    private const string _routes = "/api/v1/TestPolicies";

    private readonly Guid _workspaceId = Guid.NewGuid();

    private readonly Guid _adminId = Guid.NewGuid();

    private readonly Guid _memberId = Guid.NewGuid();

    private HttpClient CreateClient(Guid? userId)
    {
        var memberships = new InMemoryWorkspaceMembershipReader();
        memberships.Add(_workspaceId, _adminId, "Admin person", WorkspaceRole.Admin);
        memberships.Add(_workspaceId, _memberId, "Member person", WorkspaceRole.Member);

        var factory = api.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddSingleton<IWorkspaceMembershipReader>(memberships)));
        var client = factory.CreateClient();

        if (userId is not null)
        {
            client.DefaultRequestHeaders.Add(TestAuthentication.Header, userId.Value.ToString());
        }

        return client;
    }

    [Fact]
    public async Task Get_UnmarkedEndpointWithoutToken_Returns401WithProblemDetails()
    {
        var client = CreateClient(userId: null);

        var response = await client.GetAsync($"{_routes}/unmarked");

        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(
            (HttpStatusCode.Unauthorized, "application/problem+json", 401),
            (response.StatusCode, response.Content.Headers.ContentType?.MediaType, ReadStatus(body)));
    }

    [Fact]
    public async Task Get_UnmarkedEndpointWithAnIdentity_Returns200()
    {
        var client = CreateClient(_memberId);

        var response = await client.GetAsync($"{_routes}/unmarked");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_WorkspaceEndpointWithoutToken_Returns401()
    {
        var client = CreateClient(userId: null);

        var response = await client.GetAsync($"{_routes}/workspaces/{_workspaceId}/member");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_MemberOfTheWorkspace_Returns200()
    {
        var client = CreateClient(_memberId);

        var response = await client.GetAsync($"{_routes}/workspaces/{_workspaceId}/member");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_UserOutsideTheWorkspace_Returns404WithProblemDetails()
    {
        var client = CreateClient(Guid.NewGuid());

        var response = await client.GetAsync($"{_routes}/workspaces/{_workspaceId}/member");

        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(
            (HttpStatusCode.NotFound, "application/problem+json", 404),
            (response.StatusCode, response.Content.Headers.ContentType?.MediaType, ReadStatus(body)));
    }

    [Fact]
    public async Task Get_MemberWithoutTheRequiredRole_Returns403WithProblemDetails()
    {
        var client = CreateClient(_memberId);

        var response = await client.GetAsync($"{_routes}/workspaces/{_workspaceId}/admin");

        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(
            (HttpStatusCode.Forbidden, "application/problem+json", 403),
            (response.StatusCode, response.Content.Headers.ContentType?.MediaType, ReadStatus(body)));
    }

    [Fact]
    public async Task Get_AdminOfTheWorkspace_Returns200()
    {
        var client = CreateClient(_adminId);

        var response = await client.GetAsync($"{_routes}/workspaces/{_workspaceId}/admin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_PersonalDataOfTheSameUser_Returns200()
    {
        var client = CreateClient(_memberId);

        var response = await client.GetAsync($"{_routes}/users/{_memberId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_PersonalDataOfAnotherUser_Returns404()
    {
        var client = CreateClient(_memberId);

        var response = await client.GetAsync($"{_routes}/users/{_adminId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_Members_ExposesOnlyThePublicFields()
    {
        var client = CreateClient(_memberId);

        var response = await client.GetAsync($"{_routes}/workspaces/{_workspaceId}/members");

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var fields = body.RootElement.EnumerateArray()
            .SelectMany(member => member.EnumerateObject().Select(property => property.Name))
            .Distinct()
            .Order()
            .ToArray();
        Assert.Equal(["avatarUrl", "name", "role", "userId"], fields);
    }

    private static int ReadStatus(string body)
    {
        using var document = JsonDocument.Parse(body);

        return document.RootElement.GetProperty("status").GetInt32();
    }
}
