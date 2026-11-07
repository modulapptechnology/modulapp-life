using Application.Common;
using Application.VersionHistory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

namespace API.Tests.Database;

public class PersistenceRegistrationTests
{
    private const string _connectionString =
        "Server=localhost;Database=life_unit;User Id=none;Password=none;TrustServerCertificate=True";

    private static IConfiguration BuildConfiguration(string? connectionString, string? retentionDays = null)
    {
        var values = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = connectionString,
            ["VersionHistory:RetentionDays"] = retentionDays,
        };

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    [Fact]
    public void AddPersistence_WithConnectionString_RegistersDbContextAsScoped()
    {
        var services = new ServiceCollection();

        services.AddPersistence(BuildConfiguration(_connectionString));

        var descriptor = Assert.Single(services, service => service.ServiceType == typeof(LifeDbContext));
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddPersistence_WithConnectionString_EnablesRetryOnTransientFailures()
    {
        var services = new ServiceCollection();
        services.AddPersistence(BuildConfiguration(_connectionString));
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<LifeDbContext>();

        Assert.True(context.Database.CreateExecutionStrategy().RetriesOnFailure);
    }

    [Fact]
    public void AddPersistence_WithConnectionString_RegistersTheUnitOfWorkAsScoped()
    {
        var services = new ServiceCollection();

        services.AddPersistence(BuildConfiguration(_connectionString));

        var descriptor = Assert.Single(services, service => service.ServiceType == typeof(IUnitOfWork));
        Assert.Equal(
            (ServiceLifetime.Scoped, typeof(UnitOfWork)),
            (descriptor.Lifetime, descriptor.ImplementationType));
    }

    [Fact]
    public void AddPersistence_WithoutRetentionSetting_UsesNinetyDays()
    {
        var services = new ServiceCollection();
        services.AddPersistence(BuildConfiguration(_connectionString));
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<VersionHistoryOptions>();

        Assert.Equal(90, options.RetentionDays);
    }

    [Fact]
    public void AddPersistence_WithRetentionSetting_UsesTheConfiguredDays()
    {
        var services = new ServiceCollection();
        services.AddPersistence(BuildConfiguration(_connectionString, retentionDays: "30"));
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<VersionHistoryOptions>();

        Assert.Equal(30, options.RetentionDays);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("abc")]
    public void AddPersistence_WithInvalidRetention_ThrowsInvalidOperationException(string retentionDays)
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(_connectionString, retentionDays);

        var register = () => services.AddPersistence(configuration);

        Assert.Throws<InvalidOperationException>(register);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WithoutOperation_ThrowsArgumentNullException()
    {
        using var context = SampleDbContext.Create();
        var unitOfWork = new UnitOfWork(context);

        var execute = () => unitOfWork.ExecuteInTransactionAsync((Func<CancellationToken, Task>)null!);

        await Assert.ThrowsAsync<ArgumentNullException>(execute);
    }

    [Fact]
    public void AddPersistence_WithoutConnectionString_FailsWhenTheDbContextIsResolved()
    {
        var services = new ServiceCollection();
        services.AddPersistence(BuildConfiguration(connectionString: null));
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var resolve = () => scope.ServiceProvider.GetRequiredService<LifeDbContext>();

        Assert.Throws<InvalidOperationException>(resolve);
    }
}
