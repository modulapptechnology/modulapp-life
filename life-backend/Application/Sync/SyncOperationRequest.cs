using System.Text.Json;
using System.Text.Json.Serialization;
using Life.Domain.Sync;

namespace Life.Application.Sync;

public sealed record SyncOperationRequest(
    Guid OperationId,
    string EntityType,
    Guid EntityId,
    [property: JsonConverter(typeof(JsonStringEnumConverter<SyncOperationKind>))] SyncOperationKind Kind,
    JsonElement? Payload);
