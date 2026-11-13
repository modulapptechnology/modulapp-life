using System.Net;
using System.Net.Http.Json;
using Application.Workspaces;
using Domain.Workspaces;
using Flows.Tests.Support;
using Support;

namespace Flows.Tests.Flows;

[Collection(FlowsCollection.Name)]
public class WorkspaceFlowTests(FlowsFactory api)
{
    [RequiresTestDatabaseFact]
    public async Task Create_AfterSigningUp_StoresAWorkspaceWhereTheCreatorIsOwner()
    {
        var user = await FlowUser.SignUpAsync(api, "creator");

        var workspaceId = await WorkspaceFlow.CreateAsync(user, "Home");

        var workspace = await WorkspaceFlow.GetAsync(user, workspaceId);
        Assert.Equal(
            ("Home", WorkspaceRole.Owner, WorkspaceStatus.Active),
            (workspace.Name, workspace.Role, workspace.Status));
    }

    [RequiresTestDatabaseFact]
    public async Task List_AfterCreatingAWorkspace_IncludesIt()
    {
        var user = await FlowUser.SignUpAsync(api, "lister");
        var workspaceId = await WorkspaceFlow.CreateAsync(user, "Listed");

        var workspaces = await user.Client.GetFromJsonAsync<List<WorkspaceSummaryResponse>>(WorkspaceFlow.Route);

        Assert.Contains(workspaces!, workspace => workspace.Id == workspaceId);
    }

    [RequiresTestDatabaseFact]
    public async Task Update_AsTheOwner_ChangesTheNameAndReturns200()
    {
        var user = await FlowUser.SignUpAsync(api, "editor");
        var workspaceId = await WorkspaceFlow.CreateAsync(user, "Before");
        var current = await WorkspaceFlow.GetAsync(user, workspaceId);

        var response = await user.Client
            .PutAsJsonAsync(
                $"{WorkspaceFlow.Route}/{workspaceId}",
                new { name = "After", description = current.Description, rowVersion = current.RowVersion })
            .ExpectAsync("update workspace", HttpStatusCode.OK);

        var updated = await response.Content.ReadFromJsonAsync<WorkspaceSummaryResponse>();
        Assert.Equal("After", updated?.Name);
    }

    [RequiresTestDatabaseFact]
    public async Task Create_WithoutAName_Returns400()
    {
        var user = await FlowUser.SignUpAsync(api, "invalid");

        var response = await user.Client
            .PostAsJsonAsync(WorkspaceFlow.Route, new { name = string.Empty, type = "Household" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
