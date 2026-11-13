namespace Application.Common;

public sealed class ConcurrencyConflictException(Exception innerException)
    : Exception("The record was modified by another operation.", innerException);
