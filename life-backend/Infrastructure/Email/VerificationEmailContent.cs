namespace Life.Infrastructure.Email;

internal static class VerificationEmailContent
{
    public const string Subject = "Tu código de verificación";

    public static string BuildBody(string code, TimeSpan lifetime)
    {
        var newLine = Environment.NewLine;
        var minutes = (int)Math.Ceiling(lifetime.TotalMinutes);

        return $"Tu código para confirmar tu correo electrónico es:{newLine}{newLine}"
            + $"{code}{newLine}{newLine}"
            + $"El código se puede usar una sola vez y vence en {minutes} minutos.";
    }
}
