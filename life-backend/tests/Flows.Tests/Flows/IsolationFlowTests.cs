using System.Net;
using System.Net.Http.Json;
using Application.Workspaces;
using Flows.Tests.Support;
using Support;

namespace Flows.Tests.Flows;

[Collection(FlowsCollection.Name)]
public class IsolationFlowTests(FlowsFactory api)
{
    [RequiresTestDatabaseFact]
    public async Task GetWorkspace_OfAnotherWorkspaceOwner_Returns404()
    {
        var owner = await FlowUser.SignUpAsync(api, "ownerget");
        var outsider = await FlowUser.SignUpAsync(api, "outsiderget");
        var workspaceId = await WorkspaceFlow.CreateAsync(owner, "Private");

        var response = await outsider.Client.GetAsync($"{WorkspaceFlow.Route}/{workspaceId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [RequiresTestDatabaseFact]
    public async Task UpdateWorkspace_OfAnotherWorkspaceOwner_Returns404()
    {
        var owner = await FlowUser.SignUpAsync(api, "ownerput");
        var outsider = await FlowUser.SignUpAsync(api, "outsiderput");
        var workspaceId = await WorkspaceFlow.CreateAsync(owner, "Private");
        var current = await WorkspaceFlow.GetAsync(owner, workspaceId);

        var response = await outsider.Client.PutAsJsonAsync(
            $"{WorkspaceFlow.Route}/{workspaceId}",
            new { name = "Hijacked", description = current.Description, rowVersion = current.RowVersion });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [RequiresTestDatabaseFact]
    public async Task ArchiveWorkspace_OfAnotherWorkspaceOwner_Returns404()
    {
        var owner = await FlowUser.SignUpAsync(api, "ownerarch");
        var outsider = await FlowUser.SignUpAsync(api, "outsiderarch");
        var workspaceId = await WorkspaceFlow.CreateAsync(owner, "Private");

        var response = await outsider.Client.PostAsync($"{WorkspaceFlow.Route}/{workspaceId}/archive", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [RequiresTestDatabaseFact]
    public async Task List_AsAnotherUser_DoesNotIncludeTheWorkspacesOfTheOwner()
    {
        var owner = await FlowUser.SignUpAsync(api, "ownerlist");
        var outsider = await FlowUser.SignUpAsync(api, "outsiderlist");
        var workspaceId = await WorkspaceFlow.CreateAsync(owner, "Private");

        var workspaces = await outsider.Client.GetFromJsonAsync<List<WorkspaceSummaryResponse>>(WorkspaceFlow.Route);

        Assert.DoesNotContain(workspaces!, workspace => workspace.Id == workspaceId);
    }
}
