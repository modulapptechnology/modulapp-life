using System.Text.Json.Serialization;
using Domain.Workspaces;

namespace Application.Workspaces;

public sealed record CreateWorkspaceRequest(
    string Name,
    [property: JsonConverter(typeof(JsonStringEnumConverter<WorkspaceType>))] WorkspaceType Type,
    string? Description);
