namespace Life.Application.Sync;

public sealed record SyncRequest(
    string? Cursor,
    int? PageSize,
    IReadOnlyList<SyncOperationRequest> Operations);
