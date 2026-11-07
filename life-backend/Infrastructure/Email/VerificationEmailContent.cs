namespace Infrastructure.Email;

internal static class VerificationEmailContent
{
    public const string Subject = "Confirma tu correo electrónico";

    public static string BuildLink(string verificationUrl, string token)
    {
        var separator = verificationUrl.Contains('?', StringComparison.Ordinal) ? '&' : '?';

        return $"{verificationUrl}{separator}token={Uri.EscapeDataString(token)}";
    }

    public static string BuildBody(string link)
    {
        var newLine = Environment.NewLine;

        return $"Para confirmar tu correo electrónico abre este enlace:{newLine}{newLine}"
            + $"{link}{Environment.NewLine}{Environment.NewLine}"
            + "El enlace se puede usar una sola vez y tiene una vigencia limitada.";
    }
}
