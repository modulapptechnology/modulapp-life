using System.Threading.RateLimiting;
using Asp.Versioning;
using Life.API.Authorization;
using Life.API.Filters;
using Life.API.OpenApi;
using Life.API.Subscriptions;
using Microsoft.AspNetCore.RateLimiting;

namespace Life.API.Extensions;

public static class ApiServiceCollectionExtensions
{
    public const string CorsPolicyName = "AllowedOrigins";

    public const string RegistrationRateLimitPolicy = "Registration";

    public const string LoginRateLimitPolicy = "Login";

    public const string PasswordRecoveryRateLimitPolicy = "PasswordRecovery";

    public const string PasswordChangeRateLimitPolicy = "PasswordChange";

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

        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer<BearerSecurityDocumentTransformer>();
            options.AddOperationTransformer<AuthorizationOperationTransformer>();
        });
        services.AddRateLimiting(configuration);

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options => options.AddPolicy(CorsPolicyName, policy =>
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

        return services;
    }

    public static IServiceCollection AddSubscriptionReconciliationWorker(this IServiceCollection services)
    {
        services.AddHostedService<SubscriptionReconciliationWorker>();

        return services;
    }

    private static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var registration = ReadRateLimit(configuration, RegistrationRateLimitPolicy);
        var login = ReadRateLimit(configuration, LoginRateLimitPolicy);
        var recovery = ReadRateLimit(configuration, PasswordRecoveryRateLimitPolicy);
        var change = ReadRateLimit(configuration, PasswordChangeRateLimitPolicy);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            AddRateLimitPolicy(options, RegistrationRateLimitPolicy, registration, PartitionByIp);
            AddRateLimitPolicy(options, LoginRateLimitPolicy, login, PartitionByIp);
            AddRateLimitPolicy(options, PasswordRecoveryRateLimitPolicy, recovery, PartitionByIp);
            AddRateLimitPolicy(options, PasswordChangeRateLimitPolicy, change, PartitionByUser);
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

    private static string PartitionByIp(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static string PartitionByUser(HttpContext context)
    {
        var userId = context.User.GetUserId();

        return userId is null ? PartitionByIp(context) : $"user:{userId}";
    }

    private static void AddRateLimitPolicy(
        RateLimiterOptions options,
        string policy,
        (int PermitLimit, int WindowSeconds) limit,
        Func<HttpContext, string> partition)
    {
        options.AddPolicy(policy, context => RateLimitPartition.GetFixedWindowLimiter(
            partition(context),
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
