using System.Threading.RateLimiting;
using API.Filters;
using Asp.Versioning;

namespace API.Extensions;

public static class ApiServiceCollectionExtensions
{
    public const string CorsPolicyName = "AllowedOrigins";

    public const string RegistrationRateLimitPolicy = "Registration";

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
        services.AddRegistrationRateLimiter(configuration);

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options => options.AddPolicy(CorsPolicyName, policy =>
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

        return services;
    }

    private static IServiceCollection AddRegistrationRateLimiter(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = $"{RateLimitingSectionName}:Registration";
        var permitLimit = ReadPositiveNumber(configuration, $"{section}:PermitLimit", DefaultPermitLimit);
        var windowSeconds = ReadPositiveNumber(
            configuration,
            $"{section}:WindowSeconds",
            DefaultWindowSeconds);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(RegistrationRateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = TimeSpan.FromSeconds(windowSeconds),
                    QueueLimit = 0,
                }));
        });

        return services;
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
