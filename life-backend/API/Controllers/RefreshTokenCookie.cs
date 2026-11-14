using Life.Application.Users;

namespace Life.API.Controllers;

public static class RefreshTokenCookie
{
    public const string Name = "life_refresh";

    public const string DeliveryHeader = "X-Token-Delivery";

    public const string CookieDelivery = "cookie";

    public const string SameSiteKey = "Authentication:RefreshToken:CookieSameSite";

    private const string _path = "/api/v1/auth";

    public static bool IsRequested(HttpRequest request)
    {
        return string.Equals(request.Headers[DeliveryHeader], CookieDelivery, StringComparison.OrdinalIgnoreCase);
    }

    public static string? Read(HttpRequest request)
    {
        return request.Cookies.TryGetValue(Name, out var value) ? value : null;
    }

    public static LoginResponse Write(HttpContext context, LoginResponse response, IConfiguration configuration)
    {
        context.Response.Cookies.Append(Name, response.RefreshToken, CreateOptions(configuration, response.RefreshTokenExpiresAt));

        return response with { RefreshToken = string.Empty };
    }

    public static void Delete(HttpContext context, IConfiguration configuration)
    {
        context.Response.Cookies.Delete(Name, CreateOptions(configuration, null));
    }

    private static CookieOptions CreateOptions(IConfiguration configuration, DateTime? expiresAt)
    {
        var sameSite = Enum.TryParse<SameSiteMode>(configuration[SameSiteKey], true, out var parsed)
            ? parsed
            : SameSiteMode.Strict;

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = sameSite,
            Path = _path,
            Expires = expiresAt is null ? null : new DateTimeOffset(DateTime.SpecifyKind(expiresAt.Value, DateTimeKind.Utc)),
        };
    }
}
