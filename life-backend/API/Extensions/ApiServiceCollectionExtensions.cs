using System.Threading.RateLimiting;
using API.Filters;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Extensions;

public static class ApiServiceCollectionExtensions
{
    public const string CorsPolicyName = "AllowedOrigins";

    public const string RegistrationRateLimitPolicy = "Registration";

    public const string LoginRateLimitPolicy = "Login";

    public const string RateLimitingSectionName = "RateLimiting";

    public const int DefaultPermitLimit = 5;

    public const int DefaultWindowSeconds = 600;

    public static IServiceCollection AddApiStructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddControllers(options => options.Filters.Add<ValidationFilter>())
            .ConfigureApiBehaviorOptions(options =>
                options.InvalidModelStateResponseFactory = context =>
                    ValidationProblemFactory.Create(context, context.ModelState));

        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = false;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        services.AddOpenApi("v1");
        services.AddRateLimiting(configuration);

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options => options.AddPolicy(CorsPolicyName, policy =>
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

        return services;
    }

    private static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var registration = ReadRateLimit(configuration, RegistrationRateLimitPolicy);
        var login = ReadRateLimit(configuration, LoginRateLimitPolicy);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            AddRateLimitPolicy(options, RegistrationRateLimitPolicy, registration);
            AddRateLimitPolicy(options, LoginRateLimitPolicy, login);
        });

        return services;
    }

    private static (int PermitLimit, int WindowSeconds) ReadRateLimit(IConfiguration configuration, string policy)
    {
        var section = $"{RateLimitingSectionName}:{policy}";

        return (
            ReadPositiveNumber(configuration, $"{section}:PermitLimit", DefaultPermitLimit),
            ReadPositiveNumber(configuration, $"{section}:WindowSeconds", DefaultWindowSeconds));
    }

    private static void AddRateLimitPolicy(
        RateLimiterOptions options,
        string policy,
        (int PermitLimit, int WindowSeconds) limit)
    {
        options.AddPolicy(policy, context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limit.PermitLimit,
                Window = TimeSpan.FromSeconds(limit.WindowSeconds),
                QueueLimit = 0,
            }));
    }

    private static int ReadPositiveNumber(IConfiguration configuration, string key, int defaultValue)
    {
        var value = configuration[key];

        if (value is null)
        {
            return defaultValue;
        }

        if (!int.TryParse(value, out var number) || number <= 0)
        {
            throw new InvalidOperationException(
                $"The setting '{key}' must be a positive number, but was '{value}'.");
        }

        return number;
    }
}
