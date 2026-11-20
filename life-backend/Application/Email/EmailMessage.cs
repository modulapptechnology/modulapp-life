namespace Life.Application.Email;

public sealed record EmailMessage(
    string To,
    EmailTemplate Template,
    IReadOnlyDictionary<string, string> Values,
    string? Locale = null);
