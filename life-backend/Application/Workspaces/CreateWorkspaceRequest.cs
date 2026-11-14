using System.Text.Json.Serialization;
using Life.Domain.Workspaces;

namespace Life.Application.Workspaces;

public sealed record CreateWorkspaceRequest(
    string Name,
    [property: JsonConverter(typeof(JsonStringEnumConverter<WorkspaceType>))] WorkspaceType Type,
    string? Description);
