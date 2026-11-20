using System.Text.Json;

namespace Life.Application.Sync;

public sealed record SyncChange(
    string EntityType,
    Guid EntityId,
    bool IsDeleted,
    byte[] RowVersion,
    JsonElement? Data);
