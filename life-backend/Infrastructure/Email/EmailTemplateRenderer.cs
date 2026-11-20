using Life.Application.Email;

namespace Life.Infrastructure.Email;

public sealed class EmailTemplateRenderer
{
    public const string DefaultLanguage = "es";

    private static readonly string _newLine = Environment.NewLine;

    private static readonly HashSet<string> _languages = ["es", "en"];

    private static readonly Dictionary<(EmailTemplate Template, string Language), RenderedEmail> _templates = new()
    {
        [(EmailTemplate.EmailVerification, "es")] = new RenderedEmail(
            "Tu código de verificación",
            string.Join(
                _newLine,
                "Tu código para confirmar tu correo electrónico es:",
                string.Empty,
                "{code}",
                string.Empty,
                "El código se puede usar una sola vez y vence en {minutes} minutos.")),
        [(EmailTemplate.EmailVerification, "en")] = new RenderedEmail(
            "Your verification code",
            string.Join(
                _newLine,
                "Your code to confirm your email address is:",
                string.Empty,
                "{code}",
                string.Empty,
                "The code can be used only once and expires in {minutes} minutes.")),
        [(EmailTemplate.PasswordReset, "es")] = new RenderedEmail(
            "Recupera el acceso a tu cuenta",
            string.Join(
                _newLine,
                "Recibimos una solicitud para restablecer tu contraseña. Abre este enlace para elegir una nueva:",
                string.Empty,
                "{link}",
                string.Empty,
                "El enlace se puede usar una sola vez y vence en {minutes} minutos.",
                "Si no fuiste tú, ignora este mensaje: tu contraseña no cambiará.")),
        [(EmailTemplate.PasswordReset, "en")] = new RenderedEmail(
            "Recover access to your account",
            string.Join(
                _newLine,
                "We received a request to reset your password. Open this link to choose a new one:",
                string.Empty,
                "{link}",
                string.Empty,
                "The link can be used only once and expires in {minutes} minutes.",
                "If it was not you, ignore this message: your password will not change.")),
        [(EmailTemplate.PasswordChanged, "es")] = new RenderedEmail(
            "Cambiaste tu contraseña",
            string.Join(
                _newLine,
                "La contraseña de tu cuenta se cambió y se cerraron tus otras sesiones.",
                string.Empty,
                "Si no fuiste tú, recupera el acceso a tu cuenta de inmediato desde la opción de recuperación.")),
        [(EmailTemplate.PasswordChanged, "en")] = new RenderedEmail(
            "You changed your password",
            string.Join(
                _newLine,
                "Your account password was changed and your other sessions were closed.",
                string.Empty,
                "If it was not you, recover access to your account right away from the recovery option.")),
    };

    public static string ResolveLanguage(string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return DefaultLanguage;
        }

        var separator = locale.AsSpan().IndexOfAny('-', '_');
        var language = (separator < 0 ? locale : locale[..separator]).Trim().ToLowerInvariant();

        return _languages.Contains(language) ? language : DefaultLanguage;
    }

    public RenderedEmail Render(EmailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var language = ResolveLanguage(message.Locale);
        var template = _templates[(message.Template, language)];

        return new RenderedEmail(
            Fill(template.Subject, message.Values),
            Fill(template.Body, message.Values));
    }

    private static string Fill(string text, IReadOnlyDictionary<string, string> values)
    {
        foreach (var (key, value) in values)
        {
            text = text.Replace($"{{{key}}}", value, StringComparison.Ordinal);
        }

        return text;
    }
}
