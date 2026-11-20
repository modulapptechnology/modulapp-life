namespace Life.Application.Sync;

public sealed class DuplicateSyncOperationException(Exception innerException)
    : Exception("The sync operation was already processed.", innerException);
