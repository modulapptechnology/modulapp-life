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
