using Life.Persistence.Configuration;
using Microsoft.Net.Http.Headers;

namespace Life.API.Extensions;

public static class HttpRequestExtensions
{
    public static string? GetPreferredLocale(this HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var preferred = request.GetTypedHeaders().AcceptLanguage
            .Where(language => language.Quality is null or > 0)
            .OrderByDescending(language => language.Quality ?? 1)
            .Select(language => language.Value.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value) && value != "*");

        return preferred is { Length: > 0 and <= UserConfiguration.LocaleMaxLength } ? preferred : null;
    }
}
