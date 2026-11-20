using Life.Application.Email;
using Life.Infrastructure.Email;

namespace Life.Infrastructure.Tests.Email;

public class EmailTemplateRendererTests
{
    private static readonly Dictionary<string, string> _values = new()
    {
        [EmailValueKeys.Code] = "123456",
        [EmailValueKeys.Minutes] = "15",
    };

    private readonly EmailTemplateRenderer _renderer = new();

    private static EmailMessage Build(string? locale)
    {
        return new EmailMessage("person@life.test", EmailTemplate.EmailVerification, _values, locale);
    }

    [Theory]
    [InlineData("es", "Tu código de verificación")]
    [InlineData("es-CO", "Tu código de verificación")]
    [InlineData("en", "Your verification code")]
    [InlineData("en-US", "Your verification code")]
    [InlineData("EN_gb", "Your verification code")]
    public void Render_WithASupportedLocale_UsesThatLanguage(string locale, string subject)
    {
        var message = Build(locale);

        var rendered = _renderer.Render(message);

        Assert.Equal(subject, rendered.Subject);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("fr-FR")]
    [InlineData("*")]
    public void Render_WithoutASupportedLocale_UsesTheDefaultLanguage(string? locale)
    {
        var message = Build(locale);

        var rendered = _renderer.Render(message);

        Assert.Equal("Tu código de verificación", rendered.Subject);
    }

    [Theory]
    [InlineData("es", "Recupera el acceso a tu cuenta", "vence en 15 minutos")]
    [InlineData("en", "Recover access to your account", "expires in 15 minutes")]
    public void Render_WithThePasswordResetTemplate_FillsTheLinkAndTheMinutes(
        string locale,
        string subject,
        string expiration)
    {
        var values = new Dictionary<string, string>
        {
            [EmailValueKeys.Link] = "https://life.test/reset?token=abc",
            [EmailValueKeys.Minutes] = "15",
        };
        var message = new EmailMessage("person@life.test", EmailTemplate.PasswordReset, values, locale);

        var rendered = _renderer.Render(message);

        Assert.Equal(
            (subject, true, true, false),
            (
                rendered.Subject,
                rendered.Body.Contains("https://life.test/reset?token=abc", StringComparison.Ordinal),
                rendered.Body.Contains(expiration, StringComparison.Ordinal),
                rendered.Body.Contains('{', StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData("es", "vence en 15 minutos")]
    [InlineData("en", "expires in 15 minutes")]
    public void Render_WithValues_ReplacesEveryPlaceholder(string locale, string expectedPart)
    {
        var message = Build(locale);

        var rendered = _renderer.Render(message);

        Assert.Equal(
            (true, true, false),
            (
                rendered.Body.Contains("123456", StringComparison.Ordinal),
                rendered.Body.Contains(expectedPart, StringComparison.Ordinal),
                rendered.Body.Contains('{')));
    }

    [Fact]
    public void Render_WithNullMessage_ThrowsArgumentNullException()
    {
        var action = () => _renderer.Render(null!);

        Assert.Throws<ArgumentNullException>(action);
    }
}
