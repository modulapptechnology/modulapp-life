using Application.Common;
using Application.VersionHistory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Default";

    public const int MaxRetryCount = 6;

    public static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(30);

    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<LifeDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException(
                    $"The connection string '{ConnectionStringName}' is not configured. "
                        + "Set it with user-secrets, appsettings.Development.local.json or the "
                        + $"ConnectionStrings__{ConnectionStringName} environment variable.");

            options.UseSqlServer(
                connectionString,
                sqlServer => sqlServer.EnableRetryOnFailure(
                    MaxRetryCount,
                    MaxRetryDelay,
                    errorNumbersToAdd: null));
        });

        services.AddSingleton(BuildVersionHistoryOptions(configuration));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static VersionHistoryOptions BuildVersionHistoryOptions(IConfiguration configuration)
    {
        var key = $"{VersionHistoryOptions.SectionName}:{VersionHistoryOptions.RetentionDaysKey}";
        var value = configuration[key];

        if (value is null)
        {
            return new VersionHistoryOptions();
        }

        if (!int.TryParse(value, out var retentionDays) || retentionDays <= 0)
        {
            throw new InvalidOperationException(
                $"The setting '{key}' must be a positive number of days, but was '{value}'.");
        }

        return new VersionHistoryOptions(retentionDays);
    }
}
