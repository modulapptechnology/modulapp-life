using Life.API.Extensions;
using Microsoft.AspNetCore.Http;

namespace Life.API.Tests.Validation;

public class HttpRequestExtensionsTests
{
    private static HttpRequest BuildRequest(string? acceptLanguage)
    {
        var context = new DefaultHttpContext();

        if (acceptLanguage is not null)
        {
            context.Request.Headers.AcceptLanguage = acceptLanguage;
        }

        return context.Request;
    }

    [Theory]
    [InlineData("en-US", "en-US")]
    [InlineData("es-CO,es;q=0.9,en;q=0.8", "es-CO")]
    [InlineData("fr;q=0.5,en;q=0.9", "en")]
    [InlineData("*;q=0.1,es;q=0.2", "es")]
    public void GetPreferredLocale_WithTheHeader_ReturnsTheLanguageWithTheHighestQuality(
        string header,
        string expected)
    {
        var request = BuildRequest(header);

        var locale = request.GetPreferredLocale();

        Assert.Equal(expected, locale);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("*")]
    [InlineData("en;q=0")]
    public void GetPreferredLocale_WithoutAUsableLanguage_ReturnsNull(string? header)
    {
        var request = BuildRequest(header);

        var locale = request.GetPreferredLocale();

        Assert.Null(locale);
    }

    [Fact]
    public void GetPreferredLocale_WithATagLongerThanTheColumn_ReturnsNull()
    {
        var request = BuildRequest(new string('a', 36));

        var locale = request.GetPreferredLocale();

        Assert.Null(locale);
    }

    [Fact]
    public void GetPreferredLocale_WithNullRequest_ThrowsArgumentNullException()
    {
        var action = () => Life.API.Extensions.HttpRequestExtensions.GetPreferredLocale(null!);

        Assert.Throws<ArgumentNullException>(action);
    }
}
