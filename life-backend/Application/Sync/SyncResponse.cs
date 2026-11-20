namespace Life.Application.Sync;

public sealed record SyncResponse(
    IReadOnlyList<SyncOperationResult> Results,
    IReadOnlyList<SyncChange> Changes,
    string NextCursor,
    bool HasMore);
