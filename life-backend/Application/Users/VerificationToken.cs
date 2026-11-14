namespace Life.Application.Users;

public sealed record VerificationToken(string Value, string Hash)
{
    public const int CodeLength = 6;
}
