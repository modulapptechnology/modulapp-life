using Life.Domain.Sync;

namespace Life.Application.Sync;

public sealed record SyncApplyResult(SyncOperationStatus Status, string? ErrorCode = null)
{
    public static SyncApplyResult Applied() => new(SyncOperationStatus.Applied);

    public static SyncApplyResult Rejected(string errorCode) => new(SyncOperationStatus.Rejected, errorCode);
}
