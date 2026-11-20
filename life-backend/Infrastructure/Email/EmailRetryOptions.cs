namespace Life.Infrastructure.Email;

public sealed record EmailRetryOptions(int MaxAttempts, TimeSpan InitialDelay)
{
    public const string SectionName = "Email:Retry";

    public const string MaxAttemptsKey = "MaxAttempts";

    public const string InitialDelayMillisecondsKey = "InitialDelayMilliseconds";

    public const int DefaultMaxAttempts = 3;

    public const int DefaultInitialDelayMilliseconds = 500;
}
