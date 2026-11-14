using System.Net;
using System.Net.Http.Json;
using Life.Domain.Workspaces;
using Life.Flows.Tests.Support;
using Life.Support;

namespace Life.Flows.Tests.Flows;

[Collection(FlowsCollection.Name)]
public class PermissionsFlowTests(FlowsFactory api)
{
    [RequiresTestDatabaseFact]
    public async Task CreateWorkspace_WithoutASession_Returns401()
    {
        var client = api.CreateClient();

        var response = await client.PostAsJsonAsync(WorkspaceFlow.Route, new { name = "Home", type = "Household" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [RequiresTestDatabaseFact]
    public async Task GetWorkspace_WithoutASession_Returns401()
    {
        var owner = await FlowUser.SignUpAsync(api, "owner401");
        var workspaceId = await WorkspaceFlow.CreateAsync(owner, "Private");

        var response = await api.CreateClient().GetAsync($"{WorkspaceFlow.Route}/{workspaceId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [RequiresTestDatabaseFact]
    public async Task UpdateWorkspace_AsAMember_Returns403()
    {
        var owner = await FlowUser.SignUpAsync(api, "owneredit");
        var member = await FlowUser.SignUpAsync(api, "memberedit");
        var workspaceId = await WorkspaceFlow.CreateAsync(owner, "Shared");
        await WorkspaceFlow.AddMemberAsync(api, workspaceId, member.Id, WorkspaceRole.Member);
        var current = await WorkspaceFlow.GetAsync(owner, workspaceId);

        var response = await member.Client.PutAsJsonAsync(
            $"{WorkspaceFlow.Route}/{workspaceId}",
            new { name = "Hijacked", description = current.Description, rowVersion = current.RowVersion });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [RequiresTestDatabaseFact]
    public async Task ArchiveWorkspace_AsAnAdmin_Returns403()
    {
        var owner = await FlowUser.SignUpAsync(api, "ownerarchive");
        var admin = await FlowUser.SignUpAsync(api, "adminarchive");
        var workspaceId = await WorkspaceFlow.CreateAsync(owner, "Shared");
        await WorkspaceFlow.AddMemberAsync(api, workspaceId, admin.Id, WorkspaceRole.Admin);

        var response = await admin.Client.PostAsync($"{WorkspaceFlow.Route}/{workspaceId}/archive", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [RequiresTestDatabaseFact]
    public async Task GetWorkspace_AsAMember_Returns200WithTheMemberRole()
    {
        var owner = await FlowUser.SignUpAsync(api, "ownerread");
        var member = await FlowUser.SignUpAsync(api, "memberread");
        var workspaceId = await WorkspaceFlow.CreateAsync(owner, "Shared");
        await WorkspaceFlow.AddMemberAsync(api, workspaceId, member.Id, WorkspaceRole.Member);

        var workspace = await WorkspaceFlow.GetAsync(member, workspaceId);

        Assert.Equal(WorkspaceRole.Member, workspace.Role);
    }
}
