using Life.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Life.API.Tests.Database;

public class MigrationsTests
{
    private const string _connectionString = "Server=localhost;Database=life_unit;User Id=none;Password=none;TrustServerCertificate=True";

    private static LifeDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LifeDbContext>().UseSqlServer(_connectionString).Options;

        return new LifeDbContext(options);
    }

    [Fact]
    public void GetMigrations_WhenTheProjectHasMigrations_IncludesTheInitialMigration()
    {
        using var context = CreateContext();

        var migrations = context.Database.GetMigrations();

        Assert.Contains(migrations, migration => migration.EndsWith("_InitialCreate", StringComparison.Ordinal));
    }

    [Fact]
    public void HasPendingModelChanges_WhenTheModelMatchesTheSnapshot_ReturnsFalse()
    {
        using var context = CreateContext();

        var hasPendingChanges = context.Database.HasPendingModelChanges();

        Assert.False(hasPendingChanges);
    }
}
