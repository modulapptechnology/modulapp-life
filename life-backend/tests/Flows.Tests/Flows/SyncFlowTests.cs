using System.Net;
using System.Net.Http.Json;
using Life.Application.Sync;
using Life.Domain.Sync;
using Life.Flows.Tests.Support;
using Life.Support;

namespace Life.Flows.Tests.Flows;

[Collection(FlowsCollection.Name)]
public class SyncFlowTests(FlowsFactory api)
{
    private static string Route(Guid workspaceId) => $"/api/v1/workspaces/{workspaceId}/sync";

    private static object NewTask(Guid operationId) => new
    {
        operationId,
        entityType = "task",
        entityId = Guid.NewGuid(),
        kind = "Create",
        payload = new { title = "A" },
    };

    [RequiresTestDatabaseFact]
    public async Task Sync_ReceivingTheWorkspaceFromAnEmptyCursor_ReturnsItAndANextCursor()
    {
        var owner = await FlowUser.SignUpAsync(api, "syncpull");
        var workspaceId = await WorkspaceFlow.CreateAsync(owner, "Synced");

        var response = await owner.Client.PostAsJsonAsync(Route(workspaceId), new { operations = Array.Empty<object>() });

        var body = await response.Content.ReadFromJsonAsync<SyncResponse>();
        var change = Assert.Single(body!.Changes);
        Assert.Equal(
            (HttpStatusCode.OK, "workspace", workspaceId, false),
            (response.StatusCode, change.EntityType, change.EntityId, body.HasMore));
    }

    [RequiresTestDatabaseFact]
    public async Task Sync_WithTheCursorOfThePreviousResponse_ReturnsNoRepeatedChanges()
    {
        var owner = await FlowUser.SignUpAsync(api, "synccursor");
        var workspaceId = await WorkspaceFlow.CreateAsync(owner, "Synced");
        var first = await owner.Client.PostAsJsonAsync(Route(workspaceId), new { operations = Array.Empty<object>() });
        var cursor = (await first.Content.ReadFromJsonAsync<SyncResponse>())!.NextCursor;

        var second = await owner.Client.PostAsJsonAsync(
            Route(workspaceId),
            new { cursor, operations = Array.Empty<object>() });

        var body = await second.Content.ReadFromJsonAsync<SyncResponse>();
        Assert.Equal((0, cursor), (body!.Changes.Count, body.NextCursor));
    }

    [RequiresTestDatabaseFact]
    public async Task Sync_SendingTheSameOperationTwice_ReturnsTheSameResultBothTimes()
    {
        var owner = await FlowUser.SignUpAsync(api, "syncidem");
        var workspaceId = await WorkspaceFlow.CreateAsync(owner, "Synced");
        var request = new { operations = new[] { NewTask(Guid.NewGuid()) } };
        var first = await owner.Client.PostAsJsonAsync(Route(workspaceId), request);
        var firstBody = await first.Content.ReadFromJsonAsync<SyncResponse>();

        var second = await owner.Client.PostAsJsonAsync(Route(workspaceId), request);

        var secondBody = await second.Content.ReadFromJsonAsync<SyncResponse>();
        Assert.Equal(firstBody!.Results, secondBody!.Results);
        Assert.Equal(SyncOperationStatus.Rejected, secondBody.Results[0].Status);
    }

    [RequiresTestDatabaseFact]
    public async Task Sync_AsAnotherUser_Returns404()
    {
        var owner = await FlowUser.SignUpAsync(api, "syncowner");
        var outsider = await FlowUser.SignUpAsync(api, "syncoutsider");
        var workspaceId = await WorkspaceFlow.CreateAsync(owner, "Private");

        var response = await outsider.Client.PostAsJsonAsync(Route(workspaceId), new { operations = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [RequiresTestDatabaseFact]
    public async Task Sync_WithAnInvalidCursor_Returns400()
    {
        var owner = await FlowUser.SignUpAsync(api, "syncbad");
        var workspaceId = await WorkspaceFlow.CreateAsync(owner, "Synced");

        var response = await owner.Client.PostAsJsonAsync(
            Route(workspaceId),
            new { cursor = "invalid", operations = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [RequiresTestDatabaseFact]
    public async Task Sync_WithoutSession_Returns401()
    {
        var anonymous = api.CreateClient();

        var response = await anonymous.PostAsJsonAsync(
            Route(Guid.NewGuid()),
            new { operations = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
