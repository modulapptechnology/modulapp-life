using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

namespace API.Tests.Database;

public class PersistenceRegistrationTests
{
    private const string _connectionString =
        "Server=localhost;Database=life_unit;User Id=none;Password=none;TrustServerCertificate=True";

    private static IConfiguration BuildConfiguration(string? connectionString)
    {
        var values = new Dictionary<string, string?> { ["ConnectionStrings:Default"] = connectionString };

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
