using System.Net;
using System.Net.Http.Json;
using Life.Application.Workspaces;
using Life.Domain.Workspaces;

namespace Life.Flows.Tests.Support;

public static class WorkspaceFlow
{
    public const string Route = "/api/v1/Workspaces";

    public static async Task<Guid> CreateAsync(FlowUser user, string name)
    {
        var response = await user.Client
            .PostAsJsonAsync(Route, new { name, type = "Household", description = "Flow workspace" })
            .ExpectAsync("create workspace", HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CreateWorkspaceResponse>();

        return body!.Id;
    }

    public static async Task<WorkspaceSummaryResponse> GetAsync(FlowUser user, Guid workspaceId)
    {
        var response = await user.Client
            .GetAsync($"{Route}/{workspaceId}")
            .ExpectAsync("get workspace", HttpStatusCode.OK);

        return (await response.Content.ReadFromJsonAsync<WorkspaceSummaryResponse>())!;
    }

    public static async Task AddMemberAsync(
        FlowsFactory api,
        Guid workspaceId,
        Guid userId,
        WorkspaceRole role)
    {
        await using var context = api.CreateContext();
        context.WorkspaceMembers.Add(new WorkspaceMember
        {
            WorkspaceId = workspaceId,
            UserId = userId,
            Role = role,
        });
        await context.SaveChangesAsync();
    }
}
