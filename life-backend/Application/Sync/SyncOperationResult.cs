using System.Text.Json.Serialization;
using Life.Domain.Sync;

namespace Life.Application.Sync;

public sealed record SyncOperationResult(
    Guid OperationId,
    [property: JsonConverter(typeof(JsonStringEnumConverter<SyncOperationStatus>))] SyncOperationStatus Status,
    string? ErrorCode);
