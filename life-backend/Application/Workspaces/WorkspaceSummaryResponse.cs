using System.Text.Json.Serialization;
using Life.Domain.Workspaces;

namespace Life.Application.Workspaces;

public sealed record WorkspaceSummaryResponse(
    Guid Id,
    string Name,
    string? Description,
    [property: JsonConverter(typeof(JsonStringEnumConverter<WorkspaceType>))] WorkspaceType Type,
    [property: JsonConverter(typeof(JsonStringEnumConverter<WorkspaceStatus>))] WorkspaceStatus Status,
    [property: JsonConverter(typeof(JsonStringEnumConverter<WorkspaceRole>))] WorkspaceRole Role,
    byte[] RowVersion);
